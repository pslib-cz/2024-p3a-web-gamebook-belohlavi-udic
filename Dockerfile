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

# Setup nginx configuration - creating inline
RUN echo 'server { \
    listen 80; \
    server_name localhost; \
    root /app/frontend; \
    index index.html; \
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

# Create necessary directories and set up volumes
RUN mkdir -p /data && chmod 777 /data
VOLUME ["/data"]

# Direct command to run both services
EXPOSE 80
CMD bash -c "mkdir -p /data/logs && chmod -R 777 /data && mkdir -p /var/log/nginx /var/lib/nginx/body /run && nginx && cd /app/backend && dotnet GamebookApp.Backend.dll"