# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Install packages needed for code modification
RUN apt-get update && apt-get install -y sqlite3 && rm -rf /var/lib/apt/lists/*

# Copy everything
COPY . .

# Add SQLite package
RUN dotnet add /src/GameBook/GamebookApp.Backend/GamebookApp.Backend.csproj package Microsoft.EntityFrameworkCore.Sqlite

# Explicitly modify Program.cs to use SQLite - create backup first
RUN cp /src/GameBook/GamebookApp.Backend/Program.cs /src/GameBook/GamebookApp.Backend/Program.cs.bak

# Search for the SQL Server connection string configuration and replace it with SQLite
RUN grep -n "UseSqlServer" /src/GameBook/GamebookApp.Backend/Program.cs || echo "UseSqlServer not found"

# Modify Program.cs to use SQLite instead of SQL Server
RUN sed -i 's/UseSqlServer/UseSqlite/g' /src/GameBook/GamebookApp.Backend/Program.cs
RUN sed -i 's/GetConnectionString("DefaultConnection")/\"Data Source=\/data\/gamebook.db\"/g' /src/GameBook/GamebookApp.Backend/Program.cs

# Verify changes
RUN cat /src/GameBook/GamebookApp.Backend/Program.cs

# Build the project
WORKDIR "/src/GameBook/GamebookApp.Backend"
RUN dotnet restore
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install nginx and sqlite3
RUN apt-get update && apt-get install -y nginx sqlite3 && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=build /app/publish .

# Create data directory
RUN mkdir -p /data/nginx /data/db
VOLUME ["/data"]

# Completely replace nginx.conf to use only /data paths
RUN echo 'worker_processes auto;\n\
pid /data/nginx/nginx.pid;\n\
events {\n\
    worker_connections 768;\n\
}\n\
http {\n\
    sendfile on;\n\
    tcp_nopush on;\n\
    tcp_nodelay on;\n\
    keepalive_timeout 65;\n\
    types_hash_max_size 2048;\n\
    include /etc/nginx/mime.types;\n\
    default_type application/octet-stream;\n\
    access_log /data/nginx/access.log;\n\
    error_log /data/nginx/error.log;\n\
    client_body_temp_path /data/nginx/body;\n\
    proxy_temp_path /data/nginx/proxy;\n\
    fastcgi_temp_path /data/nginx/fastcgi;\n\
    uwsgi_temp_path /data/nginx/uwsgi;\n\
    scgi_temp_path /data/nginx/scgi;\n\
    include /etc/nginx/conf.d/*.conf;\n\
    include /etc/nginx/sites-enabled/*;\n\
}\n' > /etc/nginx/nginx.conf

# Simple config file for Nginx site
RUN echo 'server {\n\
    listen 80;\n\
    location / {\n\
        proxy_pass http://localhost:5000;\n\
        proxy_http_version 1.1;\n\
        proxy_set_header Upgrade $http_upgrade;\n\
        proxy_set_header Connection keep-alive;\n\
        proxy_set_header Host $host;\n\
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;\n\
        proxy_set_header X-Forwarded-Proto $scheme;\n\
    }\n\
}\n' > /etc/nginx/sites-available/default

# Simple startup script that ensures directories exist
RUN echo '#!/bin/bash\n\
mkdir -p /data/nginx/body /data/nginx/proxy /data/nginx/fastcgi /data/nginx/uwsgi /data/nginx/scgi\n\
chmod -R 777 /data\n\
nginx\n\
dotnet GamebookApp.Backend.dll\n' > /app/start.sh

RUN chmod +x /app/start.sh

EXPOSE 80
CMD ["/app/start.sh"]
