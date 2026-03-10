#!/bin/bash
# Start all development services (Keycloak + PostgreSQL)
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
docker compose -f "$SCRIPT_DIR/docker-compose.yml" up -d
echo "Services starting..."
echo "  Keycloak:    http://localhost:8081 (admin/admin)"
echo "  PostgreSQL:  localhost:5432 (risk/risk)"
