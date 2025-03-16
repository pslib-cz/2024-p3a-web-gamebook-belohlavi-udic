### BACKEND BUILD STAGE ###
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /src

# Copy the solution and project files first for better layer caching
COPY GameBook/GamebookApp.Backend.sln ./
COPY GameBook/GamebookApp.Backend/*.csproj ./GamebookApp.Backend/

# Install PostgreSQL client and database tools
RUN apt-get update && apt-get install -y postgresql-client

# Add PostgreSQL and Entity Framework packages
RUN dotnet add ./GamebookApp.Backend/GamebookApp.Backend.csproj package Npgsql.EntityFrameworkCore.PostgreSQL
RUN dotnet add ./GamebookApp.Backend/GamebookApp.Backend.csproj package Microsoft.EntityFrameworkCore.Design
RUN dotnet add ./GamebookApp.Backend/GamebookApp.Backend.csproj package Microsoft.EntityFrameworkCore.Sqlite

# Restore dependencies
RUN dotnet restore

# Copy backend source code
COPY GameBook/GamebookApp.Backend/. ./GamebookApp.Backend/

# Build and publish
WORKDIR "/src/GamebookApp.Backend"
RUN dotnet publish -c Release -o /app/publish

### FRONTEND BUILD STAGE ###
FROM node:20-alpine AS frontend-build
WORKDIR /app

# Copy package.json files for better layer caching
COPY GameBook/gamebook.client/package*.json ./

# Install dependencies
RUN npm ci

# Copy frontend source code
COPY GameBook/gamebook.client/. ./

# Set API URL for production
RUN echo "VITE_API_URL=/api" > .env

# Build the frontend
RUN npm run build

### FINAL STAGE ###
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Install nginx and PostgreSQL client
RUN apt-get update && \
    apt-get install -y nginx postgresql-client && \
    rm -rf /var/lib/apt/lists/*

# Set up Nginx directories and permissions
RUN mkdir -p /var/log/nginx /var/lib/nginx/body /run && \
    chmod -R 755 /var/log/nginx /var/lib/nginx /run

# Copy the published backend
WORKDIR /app/backend
COPY --from=backend-build /app/publish .

# Copy the built frontend
WORKDIR /app/frontend
COPY --from=frontend-build /app/dist .

# Copy nginx configuration
COPY nginx.conf /etc/nginx/sites-available/default

# Create and set up data directory
RUN mkdir -p /data && chmod 777 /data
VOLUME ["/data"]

# Copy startup script
WORKDIR /app
COPY startup.sh .
RUN chmod +x startup.sh

# Ensure proper file permissions
RUN chmod -R 555 /app/backend && chmod -R 555 /app/frontend

# Environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV DatabasePath=/data

EXPOSE 80

# Start the application
CMD ["/app/startup.sh"]
