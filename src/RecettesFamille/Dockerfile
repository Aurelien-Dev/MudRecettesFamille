# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Installer le client SSH pour pouvoir exécuter des commandes SSH vers l’hôte
RUN apt-get update && apt-get install -y openssh-client

# Étape 2 : Build l’application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["../RecettesFamille/RecettesFamille.csproj", "RecettesFamille/"]
RUN dotnet restore "./RecettesFamille/RecettesFamille.csproj"
COPY . .
WORKDIR "/src/RecettesFamille"
RUN dotnet build "./RecettesFamille.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Étape 3 : Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RecettesFamille.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Étape 4 : Conteneur final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
VOLUME ["/app/data"]

ENTRYPOINT ["dotnet", "RecettesFamille.dll"]