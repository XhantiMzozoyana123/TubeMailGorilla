# syntax=docker/dockerfile:1

# ---------------------------------------------------------------------------
# TubeMailGorilla MVC website - production Linux image
# Builds the ASP.NET Core MVC site (net8.0) so it runs inside a container on
# a Linux VPS. The MVC site shares the same Domain / Application / Infrastructure
# libraries as the API, and uses the same MySQL Identity database.
#
# Build context must be the repo ROOT because the Mvc project references the
# other projects in sibling folders. Build from the repo root:
#   docker build -t tubemailgorilla-mvc -f TubeMailGorilla.Mvc/Dockerfile .
# ---------------------------------------------------------------------------

############################## BUILD STAGE ##############################
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore with just the project files first for better layer caching.
COPY ["TubeMailGorilla.Mvc/TubeMailGorilla.Mvc.csproj",         "TubeMailGorilla.Mvc/"]
COPY ["TubeMailGorilla.Application/TubeMailGorilla.Application.csproj", "TubeMailGorilla.Application/"]
COPY ["TubeMailGorilla.Domain/TubeMailGorilla.Domain.csproj",     "TubeMailGorilla.Domain/"]
COPY ["TubeMailGorilla.Infrastructure/TubeMailGorilla.Infrastructure.csproj", "TubeMailGorilla.Infrastructure/"]
RUN dotnet restore "TubeMailGorilla.Mvc/TubeMailGorilla.Mvc.csproj"

# Copy the rest and publish (self-contained=false; the runtime image provides .NET).
COPY . .
WORKDIR "/src/TubeMailGorilla.Mvc"
RUN dotnet publish "TubeMailGorilla.Mvc.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

############################## RUNTIME STAGE ##############################
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Two external binaries the extraction pipeline shells out to. Neither ships in
# the repo (both are large binaries, and .gitignore keeps them out), so they are
# installed into the image here:
#
#   ffmpeg  - decodes the video frames behind a lead's snapshots
#             (VideoSnapshotService). The desktop edition uses its platform
#             decoder instead - Media Foundation on Windows, AVFoundation on
#             macOS - but this image runs on Linux, where neither exists.
#   yt-dlp  - fetches transcripts/subtitles and video metadata
#             (YouTubeTranscriptService, ExtractService). It is a standalone
#             binary, not an apt package, hence the direct download.
#
# Both are resolved from PATH at startup (Ffmpeg.Configure / YtDlp.Configure in
# Program.cs). If either is missing the site still starts and degrades quietly:
# snapshots are skipped and transcripts come back empty. That is a bad failure
# mode - the site looks healthy while extraction returns nothing - so verify
# both are present after the first build:
#   docker compose -f docker-compose.mvc.yml exec mvc sh -c 'which ffmpeg yt-dlp'
#
# curl is already present in the base image; python3 is a runtime dependency of
# the yt-dlp binary itself and is installed alongside it.
ARG YTDLP_URL=https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp

RUN apt-get update \
    && apt-get install -y --no-install-recommends ffmpeg python3 ca-certificates curl \
    && curl -fsSL "${YTDLP_URL}" -o /usr/local/bin/yt-dlp \
    && chmod +x /usr/local/bin/yt-dlp \
    && yt-dlp --version \
    && rm -rf /var/lib/apt/lists/*

# Ensure data directory and default App_Data exist and are owned by the non-root app user
RUN mkdir -p /app/data /app/App_Data && chown -R 1654:1654 /app

# Non-root user for security (UID 1654 = the standard `app` user in the dotnet images).
USER 1654:1654

# Port is set via ASPNETCORE_URLS at runtime (compose/env). Default for Kestrel on non-root.
EXPOSE 5000

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TubeMailGorilla.Mvc.dll"]

