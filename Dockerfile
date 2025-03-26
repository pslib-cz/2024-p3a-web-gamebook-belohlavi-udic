# BACKEND BUILD STAGE
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /src

# Debug: List directory to see the structure
RUN ls -la

# Copy entire solution directory first
COPY . .

# Debug: List directory to see what was copied
RUN ls -la

# Find the .sln file (more robust)
RUN find . -name "*.sln" | head -1 > sln_path.txt
RUN cat sln_path.txt

# Build the solution
RUN dotnet restore $(cat sln_path.txt)
RUN dotnet build $(cat sln_path.txt) -c Release -o /app/build

# Publish the application using the solution path
RUN dotnet publish $(cat sln_path.txt) -c Release -o /app/publish

# FINAL STAGE
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Install nginx and SQLite
RUN apt-get update && apt-get install -y nginx sqlite3 && rm -rf /var/lib/apt/lists/*

# Copy backend build
WORKDIR /app/backend
COPY --from=backend-build /app/publish .

# Setup frontend if it exists
WORKDIR /app/frontend
COPY wwwroot ./

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
}' > /etc/nginx/sites-available/default

# Create necessary directories and set up volumes in writable /data
RUN mkdir -p /data && chmod 777 /data
VOLUME ["/data"]

# Set environment variables for database (SQLite)
ENV ConnectionStrings__DefaultConnection="Data Source=/data/gamebook.db"

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
