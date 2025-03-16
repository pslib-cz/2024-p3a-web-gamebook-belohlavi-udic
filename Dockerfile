### BACKEND BUILD STAGE ###
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /src

# Copy solution and project files
COPY GameBook/GamebookApp.Backend.sln ./
COPY GameBook/GamebookApp.Backend/*.csproj ./GamebookApp.Backend/

# Install PostgreSQL client for database connectivity
RUN apt-get update && apt-get install -y postgresql-client

# Install EF Core tools and PostgreSQL package
RUN dotnet add ./GamebookApp.Backend/GamebookApp.Backend.csproj package Npgsql.EntityFrameworkCore.PostgreSQL
RUN dotnet add ./GamebookApp.Backend/GamebookApp.Backend.csproj package Microsoft.EntityFrameworkCore.Design

# Restore dependencies
RUN dotnet restore

# Copy everything else and build
COPY GameBook/GamebookApp.Backend/. ./GamebookApp.Backend/
WORKDIR "/src/GamebookApp.Backend"
RUN dotnet build "GamebookApp.Backend.csproj" -c Release -o /app/build

# Publish backend
FROM backend-build AS backend-publish
RUN dotnet publish "GamebookApp.Backend.csproj" -c Release -o /app/publish

### FRONTEND BUILD STAGE ###
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

### FINAL STAGE ###
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Install nginx for serving frontend and PostgreSQL client for DB connectivity
RUN apt-get update && apt-get install -y nginx postgresql-client && rm -rf /var/lib/apt/lists/*

# Create required nginx directories with proper permissions
RUN mkdir -p /var/log/nginx /var/lib/nginx/body /run && \
    chmod -R 755 /var/log/nginx /var/lib/nginx /run

# Copy backend build
WORKDIR /app/backend
COPY --from=backend-publish /app/publish .

# Copy frontend build
COPY --from=frontend-build /app/dist /app/frontend

# Setup nginx configuration
COPY ./nginx.conf /etc/nginx/sites-available/default

# Create necessary directories and set up volumes
RUN mkdir -p /data && chmod 777 /data
VOLUME ["/data"]

# Make sure application can only write to /data
RUN chmod -R 555 /app/backend && chmod -R 555 /app/frontend

# Create startup script
WORKDIR /app
COPY ./startup.sh /app/startup.sh
RUN chmod +x /app/startup.sh

EXPOSE 80
ENV ASPNETCORE_URLS=http://+:5000
ENV DatabasePath=/data

CMD ["/app/startup.sh"]
