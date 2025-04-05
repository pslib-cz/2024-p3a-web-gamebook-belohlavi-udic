FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["GamebookApp.Backend/GamebookApp.Backend.csproj", "GamebookApp.Backend/"]
RUN dotnet restore "GamebookApp.Backend/GamebookApp.Backend.csproj"
COPY . .
WORKDIR "/src/GamebookApp.Backend"
RUN dotnet build "GamebookApp.Backend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GamebookApp.Backend.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GamebookApp.Backend.dll"]