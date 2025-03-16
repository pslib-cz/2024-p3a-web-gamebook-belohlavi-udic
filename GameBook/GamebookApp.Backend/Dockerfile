FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["GamebookApp.Backend/GamebookApp.Backend.csproj", "GamebookApp.Backend/"]
RUN dotnet restore "GamebookApp.Backend/GamebookApp.Backend.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/GamebookApp.Backend"
RUN dotnet build "GamebookApp.Backend.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "GamebookApp.Backend.csproj" -c Release -o /app/publish

# Final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create a non-root user to run the app
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# Make sure we can write to /data as specified in the requirements
VOLUME ["/data"]
RUN mkdir -p /data && chown -R appuser /data

# Add PostgreSQL client libraries
RUN apt-get update && apt-get install -y postgresql-client && rm -rf /var/lib/apt/lists/*

EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "GamebookApp.Backend.dll"]