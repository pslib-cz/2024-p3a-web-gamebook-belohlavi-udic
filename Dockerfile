# BACKEND BUILD STAGE
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /src

# Copy everything
COPY . .

# Debug: List directory
RUN ls -la

# Restore and build backend
WORKDIR "/src/GameBook/GamebookApp.Backend"
RUN dotnet restore
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish

# FRONTEND BUILD STAGE
FROM node:20-alpine AS frontend-build
WORKDIR /app

# Copy frontend code from correct location
COPY GameBook/gamebook.client/. ./

# Install dependencies and build frontend
RUN npm ci || echo "No package.json found, skipping npm install"
RUN npm run build || echo "No build script found, skipping build"

# FINAL STAGE
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Install nginx and SQLite
RUN apt-get update && apt-get install -y nginx sqlite3 && rm -rf /var/lib/apt/lists/*

# Copy backend build
WORKDIR /app/backend
COPY --from=backend-build /app/publish .

# Copy frontend build if it exists
WORKDIR /app/frontend
COPY --from=frontend-build /app/dist . || echo "No frontend build to copy"

# Modify nginx main configuration
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

# Set environment variable for SQLite database
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

# Direct command
EXPOSE 80
CMD ["/app/start.sh"]
