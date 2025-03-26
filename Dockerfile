# BACKEND BUILD STAGE
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /src

# Copy solution and project files
COPY GameBook/GamebookApp.Backend.sln ./
COPY GameBook/GamebookApp.Backend/*.csproj ./GamebookApp.Backend/
RUN dotnet restore

# Copy everything else and build
COPY GameBook/GamebookApp.Backend/. ./GamebookApp.Backend/
WORKDIR "/src/GamebookApp.Backend"
RUN dotnet build "GamebookApp.Backend.csproj" -c Release -o /app/build

# Publish backend
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

# Install nginx for serving frontend
RUN apt-get update && apt-get install -y nginx && rm -rf /var/lib/apt/lists/*

# Copy backend build
WORKDIR /app/backend
COPY --from=backend-publish /app/publish .

# Copy frontend build
COPY --from=frontend-build /app/dist /app/frontend

# Setup nginx configuration - using /data for writable paths
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

# Direct command with corrected paths
EXPOSE 80
CMD bash -c "mkdir -p /data/logs /data/nginx/body /data/nginx/proxy /data/nginx/fastcgi /data/nginx/uwsgi /data/nginx/scgi && \
    chmod -R 777 /data && \
    nginx && \
    cd /app/backend && \
    dotnet GamebookApp.Backend.dll"
