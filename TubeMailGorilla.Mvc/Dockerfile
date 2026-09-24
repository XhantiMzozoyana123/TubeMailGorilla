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

# Non-root user for security (UID 1654 = the standard `app` user in the dotnet images).
USER 1654:1654

# Port is set via ASPNETCORE_URLS at runtime (compose/env). Default for Kestrel on non-root.
EXPOSE 5000

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TubeMailGorilla.Mvc.dll"]

