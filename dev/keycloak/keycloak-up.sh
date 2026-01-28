#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR=$(cd "$(dirname "$0")" && pwd)

docker compose -f "$SCRIPT_DIR/docker-compose.yml" up -d

echo "Keycloak is starting on http://localhost:8081"
