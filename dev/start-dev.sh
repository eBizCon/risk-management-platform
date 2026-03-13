#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
BACKEND_DIR="$ROOT_DIR/backend/RiskManagement.Api"
FRONTEND_DIR="$ROOT_DIR/frontend"

cleanup() {
  echo ""
  echo "Shutting down..."
  kill $BACKEND_PID $FRONTEND_PID 2>/dev/null
  wait $BACKEND_PID $FRONTEND_PID 2>/dev/null
  echo "All processes stopped."
}
trap cleanup EXIT INT TERM

echo "=== Starting Dev Services (Docker) ==="
docker compose -f "$SCRIPT_DIR/docker-compose.yml" up -d
echo "  Keycloak:    http://localhost:8081 (admin/admin)"
echo "  PostgreSQL:  localhost:5432 (risk/risk)"
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
