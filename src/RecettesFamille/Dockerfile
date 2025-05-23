# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Installer le client SSH pour pouvoir ex�cuter des commandes SSH vers l�h�te
USER root
# Ajouter le d�p�t officiel PostgreSQL pour Debian 12 (Bookworm)
RUN apt-get update && apt-get install -y wget gnupg \
    && mkdir -p /etc/apt/keyrings \
    && wget -qO - https://www.postgresql.org/media/keys/ACCC4CF8.asc | gpg --batch --yes --dearmor -o /etc/apt/keyrings/pgdg-keyring.gpg \
    && echo "deb [signed-by=/etc/apt/keyrings/pgdg-keyring.gpg] http://apt.postgresql.org/pub/repos/apt bookworm-pgdg main" | tee /etc/apt/sources.list.d/pgdg.list \
    && apt-get update \
    && apt-get install -y postgresql-client-17 \
    && rm -rf /var/lib/apt/lists/*
USER $APP_UID  # Revenir � l'utilisateur non root apr�s l'installation

# �tape 2 : Build l�application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["../RecettesFamille/RecettesFamille.csproj", "RecettesFamille/"]
RUN dotnet restore "./RecettesFamille/RecettesFamille.csproj"
COPY . .
WORKDIR "/src/RecettesFamille"
RUN dotnet build "./RecettesFamille.csproj" -c $BUILD_CONFIGURATION -o /app/build

# �tape 3 : Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RecettesFamille.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# �tape 4 : Conteneur final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
VOLUME ["/app/data"]

ENTRYPOINT ["dotnet", "RecettesFamille.dll"]