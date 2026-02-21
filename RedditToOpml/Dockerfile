FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

# Install dependencies (ffmpeg, python3, curl, nodejs, npm) and yt-dlp binary
USER root
RUN apt-get update && \
    apt-get install -y curl && \
    rm -rf /var/lib/apt/lists/*

USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RedditToOpml/RedditToOpml.csproj", "RedditToOpml/"]
RUN dotnet restore "RedditToOpml/RedditToOpml.csproj"
COPY . .
WORKDIR "/src/RedditToOpml"
RUN dotnet build "./RedditToOpml.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RedditToopml.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --chown=1000:1000 --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RedditToOpml.dll"]