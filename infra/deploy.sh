#!/bin/bash
set -euo pipefail

# =============================================================================
# Deploy infrastructure and update container images
# For initial setup use setup.sh instead
# =============================================================================

RESOURCE_GROUP="${1:?Usage: deploy.sh <resource-group> <postgres-password> <keycloak-password> <service-api-key> <rabbitmq-password> <oidc-client-secret> [environment-name]}"
POSTGRES_PASSWORD="${2:?}"
KEYCLOAK_PASSWORD="${3:?}"
SERVICE_API_KEY="${4:?}"
RABBITMQ_PASSWORD="${5:?}"
OIDC_CLIENT_SECRET="${6:?}"
ENVIRONMENT_NAME="${7:-dev}"

PREFIX="riskmgmt-${ENVIRONMENT_NAME}"
ACR_NAME="${PREFIX//\-/}acr"

echo "=== Step 1/3: Deploying Infrastructure (Bicep) ==="
az deployment group create \
  --resource-group "$RESOURCE_GROUP" \
  --template-file infra/main.bicep \
  --parameters infra/parameters/dev.bicepparam \
  --parameters environmentName="$ENVIRONMENT_NAME" \
  --parameters postgresAdminPassword="$POSTGRES_PASSWORD" \
  --parameters keycloakAdminPassword="$KEYCLOAK_PASSWORD" \
  --parameters serviceApiKey="$SERVICE_API_KEY" \
  --parameters rabbitmqPassword="$RABBITMQ_PASSWORD" \
  --parameters oidcClientSecret="$OIDC_CLIENT_SECRET"

echo ""
echo "=== Step 2/3: Building and Pushing Docker Images ==="
ACR_LOGIN_SERVER=$(az acr show --name "$ACR_NAME" --resource-group "$RESOURCE_GROUP" --query 'loginServer' -o tsv)
ACR_USERNAME=$(az acr credential show --name "$ACR_NAME" --resource-group "$RESOURCE_GROUP" --query 'username' -o tsv)
ACR_PASSWORD=$(az acr credential show --name "$ACR_NAME" --resource-group "$RESOURCE_GROUP" --query 'passwords[0].value' -o tsv)

echo "$ACR_PASSWORD" | docker login "$ACR_LOGIN_SERVER" -u "$ACR_USERNAME" --password-stdin
IMAGE_TAG="$(date +%s)"

docker build -t "${ACR_LOGIN_SERVER}/risk-management-app:${IMAGE_TAG}" -t "${ACR_LOGIN_SERVER}/risk-management-app:latest" \
  -f src/frontend/Dockerfile src/frontend
docker push "${ACR_LOGIN_SERVER}/risk-management-app:${IMAGE_TAG}"
docker push "${ACR_LOGIN_SERVER}/risk-management-app:latest"

docker build -t "${ACR_LOGIN_SERVER}/riskmanagement-api:${IMAGE_TAG}" -t "${ACR_LOGIN_SERVER}/riskmanagement-api:latest" \
  -f src/backend/RiskManagement.Api/Dockerfile src/backend
docker push "${ACR_LOGIN_SERVER}/riskmanagement-api:${IMAGE_TAG}"
docker push "${ACR_LOGIN_SERVER}/riskmanagement-api:latest"

docker build -t "${ACR_LOGIN_SERVER}/customermanagement-api:${IMAGE_TAG}" -t "${ACR_LOGIN_SERVER}/customermanagement-api:latest" \
  -f src/backend/CustomerManagement.Api/Dockerfile src/backend
docker push "${ACR_LOGIN_SERVER}/customermanagement-api:${IMAGE_TAG}"
docker push "${ACR_LOGIN_SERVER}/customermanagement-api:latest"

echo ""
echo "=== Step 3/3: Updating Container Apps ==="
az containerapp update --name "${PREFIX}-app" --resource-group "$RESOURCE_GROUP" --image "${ACR_LOGIN_SERVER}/risk-management-app:${IMAGE_TAG}" --output none
az containerapp update --name "${PREFIX}-risk-api" --resource-group "$RESOURCE_GROUP" --image "${ACR_LOGIN_SERVER}/riskmanagement-api:${IMAGE_TAG}" --output none
az containerapp update --name "${PREFIX}-customer-api" --resource-group "$RESOURCE_GROUP" --image "${ACR_LOGIN_SERVER}/customermanagement-api:${IMAGE_TAG}" --output none

echo ""
echo "Deployment complete. All 3 container apps updated with tag: ${IMAGE_TAG}"
