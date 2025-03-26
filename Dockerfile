# Backend build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything
COPY . .

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
RUN mkdir -p /data
VOLUME ["/data"]

# Set up environment
ENV ConnectionStrings__DefaultConnection="Data Source=/data/gamebook.db"

# Simple config file for nginx
RUN echo 'server { \
    listen 80; \
    location / { \
        proxy_pass http://localhost:5000; \
        proxy_http_version 1.1; \
        proxy_set_header Upgrade $http_upgrade; \
        proxy_set_header Connection keep-alive; \
        proxy_set_header Host $host; \
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for; \
        proxy_set_header X-Forwarded-Proto $scheme; \
    } \
}' > /etc/nginx/sites-available/default

# Simple startup script
RUN echo '#!/bin/bash\n\
mkdir -p /data\n\
chmod 777 /data\n\
nginx &\n\
dotnet GamebookApp.Backend.dll\n' > /app/start.sh

RUN chmod +x /app/start.sh

EXPOSE 80
CMD ["/app/start.sh"]
