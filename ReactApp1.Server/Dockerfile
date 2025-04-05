FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
RUN apt update && apt install nodejs npm -y
WORKDIR /src
COPY ["ReactApp1.Server/ReactApp1.Server.csproj", "ReactApp1.Server/"]
COPY ["reactapp1.client/reactapp1.client.esproj", "reactapp1.client/"]
RUN dotnet restore "ReactApp1.Server/ReactApp1.Server.csproj"
COPY . .
WORKDIR "/src/ReactApp1.Server"
RUN dotnet build "ReactApp1.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "ReactApp1.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false /p:PublishSingleFile=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ReactApp1.Server.dll"]
