# Deploying TubeMailGorilla API to a Linux VPS (Docker)

This runs the ASP.NET Core API and a MySQL database as Docker containers on the VPS.
(The API is a .NET 8 app; these images are built and run entirely on Linux.)

## What's included

| File                  | Purpose                                                        |
|-----------------------|----------------------------------------------------------------|
| `Dockerfile`          | Multi-stage build: SDK 8.0 build → ASP.NET 8.0 runtime, non-root |
| `.dockerignore`       | Keeps the build context small (excludes MAUI/web/bin/obj/.env)  |
| `docker-compose.yml`  | `api` + `db` (MySQL 8) services with a healthcheck              |
| `.env.example`        | Template for all secrets/config (copy to `.env`)                |
| `deploy.sh`           | One-shot build & start helper (run on the VPS)                  |

## Prerequisites on the VPS

- Docker Engine + Compose v2 (`docker compose version`)
- The repo copied to the VPS (e.g. `git clone` or `rsync`)

## 1. Get the code onto the VPS

```bash
git clone https://github.com/XhantiMzozoyana123/TubeMailGorilla.git
cd TubeMailGorillaOfficial
```

## 2. Create secrets

```bash
cp .env.example .env
nano .env
```

Fill in at minimum:
- `MYSQL_ROOT_PASSWORD`, `MYSQL_PASSWORD`
- `JWT_SECRET` (generate: `openssl rand -base64 48`)
- `PAYPAL_CLIENT_ID`, `PAYPAL_SECRET`, `PAYPAL_PLAN_ID`
- `CORS_ORIGIN_0` = your web app origin, e.g. `https://app.tubemailgorilla.com`

`.env` is gitignored and never committed.

## 3. Build & start

```bash
chmod +x deploy.sh
./deploy.sh
```

Or manually:

```bash
docker compose up -d --build
docker compose ps
docker compose logs -f api
```

## 4. Verify

```bash
curl http://127.0.0.1:8080/swagger/v1/swagger.json    # OpenAPI (dev only)
# Health / plans are public JSON:
curl http://127.0.0.1:8080/api/payments/plans
```

## 5. Put it behind a reverse proxy with TLS

The API listens on `http://0.0.0.0:8080` in the container (mapped to `${API_PORT:-8080}`
on the host). Terminate TLS in front of it with nginx, Caddy, or Traefik and forward to
the host API port. Example Caddyfile:

```
api.tubemailgorilla.com {
    reverse_proxy 127.0.0.1:8080
}
```

## Notes & gotchas

- **First run:** the API calls `EnsureCreatedAsync` and creates the `Subscriptions`
  table automatically; the MySQL database itself is created by the `db` container from
  `MYSQL_DATABASE`. The `api` container waits for the DB healthcheck before starting.
- **Config:** appsettings are overridden by environment variables in `docker-compose.yml`
  (e.g. `ConnectionStrings__DefaultConnection`, `JwtSettings__Secret`). The committed
  `appsettings.json` values are placeholders / local defaults.
- **Updating:** `git pull && ./deploy.sh` rebuilds and restarts.
- **Backups:** the MySQL data lives in the `mysql_data` Docker volume. Back it up with
  `docker compose exec db mysqldump -u root -p$MYSQL_ROOT_PASSWORD TubeMailGorillaDB`.
- **Swagger** only renders when `ASPNETCORE_ENVIRONMENT=Production` is false; production
  builds disable it. If you want it behind TLS in prod, set `ASPNETCORE_ENVIRONMENT` to
  `Development` (not recommended).