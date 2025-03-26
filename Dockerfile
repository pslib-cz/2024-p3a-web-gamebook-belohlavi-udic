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

# Jednoduchý sed pro nahrazení SQL Server za SQLite
RUN sed -i 's/UseSqlServer/UseSqlite/g' Program.cs
RUN sed -i 's/GetConnectionString("DefaultConnection")/"Data Source=\/data\/gamebook.db"/g' Program.cs

# Zobrazíme změny
RUN grep -A 3 -B 3 "UseSqlite" Program.cs || echo "Not found"

# Obnovení balíčků a sestavení
RUN dotnet restore
RUN dotnet build -c Release

# Publikování aplikace
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app

# Kopírování publikované backend aplikace
COPY --from=backend-build /app/publish .

# Vytvoření adresáře pro data
RUN mkdir -p /data && chmod 777 /data

# Konfigurace webového serveru
ENV ASPNETCORE_URLS="http://+:80"
EXPOSE 80

# Spuštění aplikace
ENTRYPOINT ["dotnet", "GamebookApp.Backend.dll"]
