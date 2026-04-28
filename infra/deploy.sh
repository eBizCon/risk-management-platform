#!/bin/bash
set -euo pipefail

# =============================================================================
# Deploy infrastructure via Bicep
# For initial setup use setup.sh instead
# Docker image builds are handled by GitHub Actions (deploy.yml)
# =============================================================================

RESOURCE_GROUP="${1:?Usage: deploy.sh <resource-group> <postgres-password> <keycloak-password> <oidc-client-secret> <service-api-key> <rabbitmq-password> <session-secret> [ghcr-token]}"
POSTGRES_PASSWORD="${2:?}"
KEYCLOAK_PASSWORD="${3:?}"
OIDC_CLIENT_SECRET="${4:?}"
SERVICE_API_KEY="${5:?}"
RABBITMQ_PASSWORD="${6:?}"
SESSION_SECRET="${7:?}"
GHCR_TOKEN="${8:-}"

az deployment group create \
  --resource-group "$RESOURCE_GROUP" \
  --template-file infra/main.bicep \
  --parameters environmentName="dev" \
  --parameters location="northeurope" \
  --parameters postgresAdminPassword="$POSTGRES_PASSWORD" \
  --parameters keycloakAdminPassword="$KEYCLOAK_PASSWORD" \
  --parameters oidcClientSecret="$OIDC_CLIENT_SECRET" \
  --parameters serviceApiKey="$SERVICE_API_KEY" \
  --parameters rabbitmqPassword="$RABBITMQ_PASSWORD" \
  --parameters sessionSecret="$SESSION_SECRET" \
  --parameters ghcrToken="$GHCR_TOKEN"
