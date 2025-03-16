### BACKEND BUILD STAGE ###
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

# Install nginx for serving frontend
RUN apt-get update && apt-get install -y nginx postgresql-client && rm -rf /var/lib/apt/lists/*

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

# Create startup script
WORKDIR /app
COPY ./startup.sh /app/startup.sh
RUN chmod +x /app/startup.sh

EXPOSE 80
CMD ["/app/startup.sh"]
