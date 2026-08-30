# syntax=docker/dockerfile:1

# ---------------------------------------------------------------------------
# TubeMailGorilla API - production Linux image
# Builds the ASP.NET Core API (net8.0) so it runs inside a container on a VPS.
#
# Build context must be the repo ROOT (the Api references the Application,
# Domain and Infrastructure projects). Build from the repo root:
#   docker build -t tubemailgorilla-api -f Dockerfile .
# ---------------------------------------------------------------------------

############################## BUILD STAGE ##############################
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore with just the project files first for better layer caching.
COPY ["TubeMailGorilla.Api/TubeMailGorilla.Api.csproj",              "TubeMailGorilla.Api/"]
COPY ["TubeMailGorilla.Application/TubeMailGorilla.Application.csproj", "TubeMailGorilla.Application/"]
COPY ["TubeMailGorilla.Domain/TubeMailGorilla.Domain.csproj",         "TubeMailGorilla.Domain/"]
COPY ["TubeMailGorilla.Infrastructure/TubeMailGorilla.Infrastructure.csproj", "TubeMailGorilla.Infrastructure/"]
RUN dotnet restore "TubeMailGorilla.Api/TubeMailGorilla.Api.csproj"

# Copy the rest and publish (self-contained=false; the runtime image provides .NET).
COPY . .
WORKDIR "/src/TubeMailGorilla.Api"
RUN dotnet publish "TubeMailGorilla.Api.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

############################## RUNTIME STAGE ##############################
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Non-root user for security (UID 1654 = the standard `app` user in the dotnet images).
USER 1654:1654

# Port is set via ASPNETCORE_URLS at runtime (compose/env). Default for Kestrel on non-root.
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TubeMailGorilla.Api.dll"]