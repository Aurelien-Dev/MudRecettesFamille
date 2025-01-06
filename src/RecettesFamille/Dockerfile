# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app

# Configure UTF-8 locale
ENV LANG en_US.UTF-8
ENV LANGUAGE en_US:en
ENV LC_ALL en_US.UTF-8
RUN apt-get update && apt-get install -y locales && \
    locale-gen en_US.UTF-8 && \
    dpkg-reconfigure locales && \
    apt-get clean && rm -rf /var/lib/apt/lists/*

USER root
# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["../RecettesFamille/RecettesFamille.csproj", "RecettesFamille/"]
RUN dotnet restore "./RecettesFamille/RecettesFamille.csproj"
COPY . .
WORKDIR "/src/RecettesFamille"
RUN dotnet build "./RecettesFamille.csproj" -c $BUILD_CONFIGURATION -o /app/build

USER $APP_UID
# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RecettesFamille.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
VOLUME ["/app/data"]

ENTRYPOINT ["dotnet", "RecettesFamille.dll"]