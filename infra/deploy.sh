#!/bin/bash
set -euo pipefail

# =============================================================================
# Deploy infrastructure via Bicep
# For initial setup use setup.sh instead
# Docker image builds are handled by GitHub Actions (deploy.yml)
# =============================================================================

RESOURCE_GROUP="${1:?Usage: deploy.sh <resource-group> <postgres-password> <keycloak-password> <service-api-key> <rabbitmq-password> <session-secret> [ghcr-token]}"
POSTGRES_PASSWORD="${2:?}"
KEYCLOAK_PASSWORD="${3:?}"
SERVICE_API_KEY="${4:?}"
RABBITMQ_PASSWORD="${5:?}"
SESSION_SECRET="${6:?}"
GHCR_TOKEN="${7:-}"

az deployment group create \
  --resource-group "$RESOURCE_GROUP" \
  --template-file infra/main.bicep \
  --parameters infra/parameters/dev.bicepparam \
  --parameters postgresAdminPassword="$POSTGRES_PASSWORD" \
  --parameters keycloakAdminPassword="$KEYCLOAK_PASSWORD" \
  --parameters serviceApiKey="$SERVICE_API_KEY" \
  --parameters rabbitmqPassword="$RABBITMQ_PASSWORD" \
  --parameters sessionSecret="$SESSION_SECRET" \
  --parameters ghcrToken="$GHCR_TOKEN"
