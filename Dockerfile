# Build stage pro backend
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS backend-build

# Nastavení pracovního adresáře
WORKDIR /src

# Kopírování celého řešení
COPY . .

# Zobrazení struktury souborů pro debugging
RUN ls -la
RUN find . -name "*.csproj"

# Nastavení korektní cesty k projektu podle solution souboru
WORKDIR "/src/GamebookApp.Backend"

# Instalace Microsoft.EntityFrameworkCore.Sqlite
RUN dotnet add package Microsoft.EntityFrameworkCore.Sqlite

# Úprava Program.cs pro použití SQLite místo SQL Serveru
# Nejprve vytvořme zálohu
RUN cp Program.cs Program.cs.backup || echo "Program.cs not found"

# Jednoduchý sed pro nahrazení SQL Server za SQLite
RUN sed -i 's/UseSqlServer/UseSqlite/g' Program.cs || echo "Unable to replace UseSqlServer"
RUN sed -i 's/GetConnectionString("DefaultConnection")/"Data Source=\/data\/gamebook.db"/g' Program.cs || echo "Unable to replace connection string"

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
