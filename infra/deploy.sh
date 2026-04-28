#!/bin/bash
set -euo pipefail

RESOURCE_GROUP="${1:?Usage: deploy.sh <resource-group> <postgres-password> <keycloak-password> <service-api-key> <rabbitmq-password> [ghcr-token]}"
POSTGRES_PASSWORD="${2:?}"
KEYCLOAK_PASSWORD="${3:?}"
SERVICE_API_KEY="${4:?}"
RABBITMQ_PASSWORD="${5:?}"
GHCR_TOKEN="${6:-}"

az group create --name "$RESOURCE_GROUP" --location germanywestcentral
az deployment group create \
  --resource-group "$RESOURCE_GROUP" \
  --template-file infra/main.bicep \
  --parameters infra/parameters/dev.bicepparam \
  --parameters postgresAdminPassword="$POSTGRES_PASSWORD" \
  --parameters keycloakAdminPassword="$KEYCLOAK_PASSWORD" \
  --parameters serviceApiKey="$SERVICE_API_KEY" \
  --parameters rabbitmqPassword="$RABBITMQ_PASSWORD" \
  --parameters ghcrToken="$GHCR_TOKEN"
