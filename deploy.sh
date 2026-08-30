#!/usr/bin/env bash
# ---------------------------------------------------------------------------
# Deploy the TubeMailGorilla API to a Linux VPS using Docker Compose.
#
# Usage (run ON the VPS, from the repo root):
#   chmod +x deploy.sh
#   ./deploy.sh            # build + start
#   ./deploy.sh --rebuild  # force a fresh image build (no cache)
#   ./deploy.sh --stop     # stop and remove containers
#   ./deploy.sh --logs     # tail API logs (Ctrl+C to detach)
#   ./deploy.sh --status   # show service status
#
# Prerequisites:
#   - Docker Engine + Docker Compose v2
#   - A .env file copied from .env.example with real secrets filled in:
#       cp .env.example .env && nano .env
# ---------------------------------------------------------------------------
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

# -- helpers -----------------------------------------------------------------

log()  { echo -e "\033[1;36m==>\033[0m $*"; }
warn() { echo -e "\033[1;33m!!\033[0m $*"; }
ok()   { echo -e "\033[1;32m OK\033[0m $*"; }
die()  { echo -e "\033[1;31mXX\033[0m $*" >&2; exit 1; }

check_docker() {
    command -v docker >/dev/null 2>&1 || die "Docker is not installed. Install it first: https://docs.docker.com/engine/install/"
    docker compose version >/dev/null 2>&1 || die "Docker Compose v2 is not available. Install Docker Compose: https://docs.docker.com/compose/"
}

check_env() {
    if [ ! -f .env ]; then
        warn "Missing .env file."
        echo "   Create it from the template and fill in real values:"
        echo "   cp .env.example .env && nano .env"
        exit 1
    fi
    # Verify required secrets are not left as placeholders.
    local missing=0
    for var in MYSQL_ROOT_PASSWORD MYSQL_PASSWORD JWT_SECRET; do
        local val
        val="$(grep -E "^${var}=" .env | head -1 | cut -d= -f2- || true)"
        if [ -z "$val" ] || [[ "$val" == "CHANGE_ME"* ]]; then
            warn "Please set a real value for $var in .env"
            missing=1
        fi
    done
    [ "$missing" -eq 0 ] || die "One or more required secrets are still set to placeholder values in .env."
}

wait_for_api() {
    local port="${API_PORT:-8080}"
    local max=30
    log "Waiting for API health on http://127.0.0.1:${port}/swagger/v1/swagger.json ..."
    for i in $(seq 1 "$max"); do
        if curl -sf "http://127.0.0.1:${port}/swagger/v1/swagger.json" >/dev/null 2>&1; then
            ok "API is healthy."
            return 0
        fi
        sleep 2
    done
    warn "API did not become healthy within $((max * 2)) seconds."
    echo "    Recent logs:"
    docker compose logs --tail 30 api 2>/dev/null || true
}

# -- subcommands --------------------------------------------------------------

cmd_stop() {
    log "Stopping containers..."
    docker compose stop
    log "Removing containers..."
    docker compose rm -f
}

cmd_logs() {
    docker compose logs -f --tail 100 api
}

cmd_status() {
    docker compose ps
}

cmd_deploy() {
    check_docker
    check_env
    export COMPOSE_PROJECT_NAME="${COMPOSE_PROJECT_NAME:-tubemailgorilla}"

    local build_args=""
    if [ "${1:-}" = "--rebuild" ]; then
        build_args="--build --force-recreate"
        log "Force-rebuilding images..."
    else
        log "Building images and starting services..."
    fi

    docker compose up -d ${build_args}

    log "Service status:"
    docker compose ps

    wait_for_api

    log "Deployment complete!"
    echo
    echo "  API is listening on: http://0.0.0.0:${API_PORT:-8080}"
    echo "  Swagger UI:          http://<VPS-IP>:${API_PORT:-8080}/swagger"
    echo "  Health (plans):      curl http://<VPS-IP>:${API_PORT:-8080}/api/payments/plans"
    echo
    echo "  Put this behind a TLS-terminating reverse proxy (nginx/Caddy/Traefik)."
    echo "  See DEPLOY.md for full details."
}

# -- main ---------------------------------------------------------------------

case "${1:-deploy}" in
    --rebuild) cmd_deploy --rebuild ;;
    --stop)    cmd_stop ;;
    --logs)    cmd_logs ;;
    --status)  cmd_status ;;
    deploy|"") cmd_deploy ;;
    *)         die "Unknown option: $1" ;;
esac