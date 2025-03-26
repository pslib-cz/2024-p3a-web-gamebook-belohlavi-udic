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

# Frontend build stage
FROM node:18-alpine AS frontend-build
WORKDIR /src

# Copy frontend files
COPY GameBook/gamebook.client/ .

# Install dependencies and build (using conditional execution to prevent failures)
RUN if [ -f "package.json" ]; then \
        npm install && \
        npm run build; \
    else \
        echo "No package.json found, skipping frontend build"; \
        mkdir -p dist; \
    fi

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install sqlite3
RUN apt-get update && apt-get install -y sqlite3 && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=build /app/publish .

# Copy frontend build to wwwroot (if it exists)
COPY --from=frontend-build /src/dist /app/wwwroot/

# Create data directory
RUN mkdir -p /data
VOLUME ["/data"]

# Set environment variable for SQLite database
ENV ConnectionStrings__DefaultConnection="Data Source=/data/gamebook.db"

EXPOSE 80
ENTRYPOINT ["dotnet", "GamebookApp.Backend.dll"]
