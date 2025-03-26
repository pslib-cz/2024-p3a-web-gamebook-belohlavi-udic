# Build stage pro backend
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS backend-build

# Nastavení pracovního adresáře
WORKDIR /src

# Kopírování celého řešení
COPY . .

# Instalace Microsoft.EntityFrameworkCore.Sqlite
WORKDIR "/src/GameBook/GamebookApp.Backend"
RUN dotnet add package Microsoft.EntityFrameworkCore.Sqlite

# Úprava Program.cs pro použití SQLite místo SQL Serveru
# Nejprve vytvořme zálohu
RUN cp Program.cs Program.cs.backup

# Vytvoříme jednoduchý skript, který upraví konfiguraci
RUN echo '#!/bin/bash\n\
# Zjistíme, kde je volání UseSqlServer\n\
LINE_NUMBER=$(grep -n "UseSqlServer" Program.cs | cut -d ":" -f1)\n\
if [ -n "$LINE_NUMBER" ]; then\n\
  # Nahradíme UseSqlServer za UseSqlite\n\
  sed -i "${LINE_NUMBER}s/UseSqlServer/UseSqlite/g" Program.cs\n\
  # Nahradíme GetConnectionString za pevný string\n\
  sed -i "${LINE_NUMBER}s/GetConnectionString(\\\"DefaultConnection\\\")/\\\"Data Source=\\/data\\/gamebook.db\\\"/g" Program.cs\n\
fi\n\
' > update_program.sh

RUN chmod +x update_program.sh
RUN ./update_program.sh

# Zobrazíme změny
RUN grep -A 3 -B 3 "UseSqlite" Program.cs || echo "UseSqlite not found in Program.cs"

# Obnovení balíčků a sestavení
RUN dotnet restore
RUN dotnet build -c Release

# Publikování aplikace
RUN dotnet publish -c Release -o /app/publish

# Build stage pro frontend
FROM node:16 AS frontend-build
WORKDIR /src

# Kopírování frontend kódu
COPY GameBook/gamebook.client/. .

# Instalace závislostí a sestavení (s ochranou proti selhání)
RUN if [ -f "package.json" ]; then \
      echo "Installing frontend dependencies..." && \
      npm install && \
      echo "Building frontend..." && \
      npm run build || echo "Frontend build failed, continuing anyway"; \
    else \
      echo "No package.json found, skipping frontend build"; \
      mkdir -p dist; \
    fi

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app

# Kopírování publikované backend aplikace
COPY --from=backend-build /app/publish .

# Kopírování frontendu do wwwroot (pokud existuje)
RUN mkdir -p wwwroot
COPY --from=frontend-build /src/dist/. ./wwwroot/ || echo "No frontend dist folder to copy"

# Pro kontrolu zobrazíme obsah
RUN echo "Backend files:" && ls -la
RUN echo "Frontend files:" && ls -la wwwroot || echo "wwwroot is empty"

# Vytvoření adresáře pro data
RUN mkdir -p /data && chmod 777 /data

# Konfigurace webového serveru
ENV ASPNETCORE_URLS="http://+:80"
EXPOSE 80

# Spuštění aplikace
ENTRYPOINT ["dotnet", "GamebookApp.Backend.dll"]
