# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# ===== Runtime (Alpine) =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
USER root
# Client PostgreSQL 17 + SSH (pour les backups)
RUN apk add --no-cache postgresql17-client openssh-client
# repasser à l'utilisateur applicatif
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Étape 2 : Build l’application
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
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