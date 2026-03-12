#!/bin/bash
set -euo pipefail

RESOURCE_GROUP="${1:?Usage: deploy.sh <resource-group> <postgres-password> <keycloak-password>}"
POSTGRES_PASSWORD="${2:?}"
KEYCLOAK_PASSWORD="${3:?}"

az group create --name "$RESOURCE_GROUP" --location germanywestcentral
az deployment group create \
  --resource-group "$RESOURCE_GROUP" \
  --template-file infra/main.bicep \
  --parameters infra/parameters/dev.bicepparam \
  --parameters postgresAdminPassword="$POSTGRES_PASSWORD" \
  --parameters keycloakAdminPassword="$KEYCLOAK_PASSWORD"
