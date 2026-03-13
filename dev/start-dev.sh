#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
BACKEND_DIR="$ROOT_DIR/src/backend/RiskManagement.Api"
FRONTEND_DIR="$ROOT_DIR/src/frontend"

cleanup() {
  echo ""
  echo "Shutting down..."
  pkill -P $BACKEND_PID 2>/dev/null
  pkill -P $FRONTEND_PID 2>/dev/null
  kill $BACKEND_PID $FRONTEND_PID 2>/dev/null
  lsof -ti:5227 -ti:5173 | xargs kill -9 2>/dev/null
  wait $BACKEND_PID $FRONTEND_PID 2>/dev/null
  echo "All processes stopped."
}
trap cleanup EXIT INT TERM

echo "=== Starting Dev Services (Docker) ==="
docker compose -f "$SCRIPT_DIR/docker-compose.yml" up -d
echo "  Keycloak:    http://localhost:8081 (admin/admin)"
echo "  PostgreSQL:  localhost:5432 (risk/risk)"
echo ""

echo "=== Loading .env ==="
set -a
source "$ROOT_DIR/.env"
set +a

if [[ "$DATABASE_URL" =~ ^postgresql://([^:]+):([^@]+)@([^:]+):([0-9]+)/(.+)$ ]]; then
  export DATABASE_URL="Host=${BASH_REMATCH[3]};Port=${BASH_REMATCH[4]};Database=${BASH_REMATCH[5]};Username=${BASH_REMATCH[1]};Password=${BASH_REMATCH[2]}"
fi
echo "  Loaded env vars from $ROOT_DIR/.env"
echo ""

echo "=== Starting Backend (dotnet watch) ==="
cd "$BACKEND_DIR"
dotnet watch run --launch-profile http &
BACKEND_PID=$!
echo "  Backend PID: $BACKEND_PID  →  http://localhost:5227"
echo ""

echo "=== Starting Frontend (vite dev) ==="
cd "$FRONTEND_DIR"
npm run dev &
FRONTEND_PID=$!
echo "  Frontend PID: $FRONTEND_PID  →  http://localhost:5173"
echo ""

echo "=== All services running. Press Ctrl+C to stop. ==="
wait
