# BACKEND BUILD STAGE
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /src

# Copy solution and project files
COPY GameBook/GamebookApp.Backend.sln ./
COPY GameBook/GamebookApp.Backend/*.csproj ./GamebookApp.Backend/

# Install SQLite package
RUN dotnet add ./GamebookApp.Backend/GamebookApp.Backend.csproj package Microsoft.EntityFrameworkCore.Sqlite

# Restore dependencies
RUN dotnet restore

# Copy everything else
COPY GameBook/GamebookApp.Backend/. ./GamebookApp.Backend/

# Modify Program.cs to use SQLite instead of SQL Server
RUN sed -i 's/builder.Services.AddDbContext<AppDbContext>(options =>/builder.Services.AddDbContext<AppDbContext>(options =>/' ./GamebookApp.Backend/Program.cs
RUN sed -i 's/options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));/options.UseSqlite("Data Source=\/data\/gamebook.db"));/' ./GamebookApp.Backend/Program.cs

# Update connection string in appsettings.json 
RUN sed -i 's/"DefaultConnection": ".*"/"DefaultConnection": "Data Source=\/data\/gamebook.db"/' ./GamebookApp.Backend/appsettings.json

# Build the application
WORKDIR "/src/GamebookApp.Backend"
RUN dotnet build "GamebookApp.Backend.csproj" -c Release -o /app/build

# Publish the application
FROM backend-build AS backend-publish
RUN dotnet publish "GamebookApp.Backend.csproj" -c Release -o /app/publish

# FRONTEND BUILD STAGE
FROM node:20-alpine AS frontend-build
WORKDIR /app

# Copy package.json and install dependencies
COPY GameBook/gamebook.client/package*.json ./
RUN npm ci

# Copy frontend code and build
COPY GameBook/gamebook.client/. ./
# Ensure we have the correct environment configuration
RUN echo "VITE_API_URL=/api" > .env
RUN npm run build

# FINAL STAGE
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Install nginx and SQLite
RUN apt-get update && apt-get install -y nginx sqlite3 && rm -rf /var/lib/apt/lists/*

# Copy backend build
WORKDIR /app/backend
COPY --from=backend-publish /app/publish .

# Copy frontend build
COPY --from=frontend-build /app/dist /app/frontend

# Modify nginx main configuration to use /data for logs and PID
RUN sed -i 's|access_log /var/log/nginx/access.log;|access_log /data/nginx/logs/access.log;|g' /etc/nginx/nginx.conf && \
    sed -i 's|error_log /var/log/nginx/error.log;|error_log /data/nginx/logs/error.log;|g' /etc/nginx/nginx.conf && \
    sed -i 's|pid /run/nginx.pid;|pid /data/nginx/nginx.pid;|g' /etc/nginx/nginx.conf && \
    sed -i 's|user www-data;|# user www-data;|g' /etc/nginx/nginx.conf

# Setup nginx configuration for our application
RUN echo 'server { \
    listen 80; \
    server_name localhost; \
    root /app/frontend; \
    index index.html; \
    \
    client_body_temp_path /data/nginx/body; \
    proxy_temp_path /data/nginx/proxy; \
    fastcgi_temp_path /data/nginx/fastcgi; \
    uwsgi_temp_path /data/nginx/uwsgi; \
    scgi_temp_path /data/nginx/scgi; \
    \
    access_log /data/nginx/logs/access.log; \
    error_log /data/nginx/logs/error.log; \
    \
    location / { \
        try_files $uri $uri/ /index.html; \
    } \
    \
    location /api/ { \
        proxy_pass http://localhost:5000/api/; \
        proxy_http_version 1.1; \
        proxy_set_header Upgrade $http_upgrade; \
        proxy_set_header Connection keep-alive; \
        proxy_set_header Host $host; \
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for; \
        proxy_set_header X-Forwarded-Proto $scheme; \
    } \
    \
    location /images/ { \
        alias /app/backend/wwwroot/images/; \
    } \
}' > /etc/nginx/sites-available/default

# Create necessary directories and set up volumes in writable /data
RUN mkdir -p /data && chmod 777 /data
VOLUME ["/data"]

# Create startup script
RUN echo '#!/bin/bash\n\
mkdir -p /data/logs /data/nginx/logs /data/nginx/body /data/nginx/proxy /data/nginx/fastcgi /data/nginx/uwsgi /data/nginx/scgi\n\
chmod -R 777 /data\n\
ln -sf /dev/stdout /data/nginx/logs/access.log\n\
ln -sf /dev/stderr /data/nginx/logs/error.log\n\
cd /app/backend\n\
nginx -g "daemon off;" &\n\
dotnet GamebookApp.Backend.dll\n' > /app/start.sh && \
chmod +x /app/start.sh

# Direct command with corrected paths
EXPOSE 80
CMD ["/app/start.sh"]
