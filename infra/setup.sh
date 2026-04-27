#!/bin/bash
set -euo pipefail

# =============================================================================
# Full environment setup for Risk Management Platform on Azure Container Apps
# Creates all infrastructure, builds and deploys the app, configures Keycloak
# =============================================================================

RESOURCE_GROUP="${1:?Usage: setup.sh <resource-group> <postgres-password> <keycloak-password> <rabbitmq-password> <session-secret> <service-api-key> [environment-name]}"
POSTGRES_PASSWORD="${2:?}"
KEYCLOAK_PASSWORD="${3:?}"
RABBITMQ_PASSWORD="${4:?}"
SESSION_SECRET="${5:?}"
SERVICE_API_KEY="${6:?}"
ENVIRONMENT_NAME="${7:-dev}"

PREFIX="riskmgmt-${ENVIRONMENT_NAME}"
ACR_NAME="${PREFIX//\-/}acr"

echo "=== Step 1/7: Creating Resource Group ==="
az group create --name "$RESOURCE_GROUP" --location northeurope --output none
echo "Resource group '$RESOURCE_GROUP' created in northeurope."

echo ""
echo "=== Step 2/7: Deploying Infrastructure (Bicep) ==="
DEPLOY_OUTPUT=$(az deployment group create \
  --resource-group "$RESOURCE_GROUP" \
  --template-file infra/main.bicep \
  --parameters infra/parameters/dev.bicepparam \
  --parameters environmentName="$ENVIRONMENT_NAME" \
  --parameters postgresAdminPassword="$POSTGRES_PASSWORD" \
  --parameters keycloakAdminPassword="$KEYCLOAK_PASSWORD" \
  --parameters rabbitmqPassword="$RABBITMQ_PASSWORD" \
  --parameters sessionSecret="$SESSION_SECRET" \
  --parameters serviceApiKey="$SERVICE_API_KEY" \
  --query 'properties.outputs' \
  --output json)

ACR_LOGIN_SERVER=$(echo "$DEPLOY_OUTPUT" | jq -r '.acrLoginServer.value')
KEYCLOAK_FQDN=$(echo "$DEPLOY_OUTPUT" | jq -r '.keycloakFqdn.value')
FRONTEND_FQDN=$(echo "$DEPLOY_OUTPUT" | jq -r '.frontendFqdn.value')
RISK_API_FQDN=$(echo "$DEPLOY_OUTPUT" | jq -r '.riskApiFqdn.value')
CUSTOMER_API_FQDN=$(echo "$DEPLOY_OUTPUT" | jq -r '.customerApiFqdn.value')
POSTGRES_FQDN=$(echo "$DEPLOY_OUTPUT" | jq -r '.postgresFqdn.value')

echo "Infrastructure deployed:"
echo "  ACR:            $ACR_LOGIN_SERVER"
echo "  Keycloak:       https://$KEYCLOAK_FQDN"
echo "  Frontend:       https://$FRONTEND_FQDN"
echo "  Risk API:       https://$RISK_API_FQDN"
echo "  Customer API:   https://$CUSTOMER_API_FQDN"
echo "  PostgreSQL:     $POSTGRES_FQDN"

echo ""
echo "=== Step 3/7: Building and Pushing Docker Images ==="
ACR_USERNAME=$(az acr credential show --name "$ACR_NAME" --resource-group "$RESOURCE_GROUP" --query 'username' -o tsv)
ACR_PASSWORD=$(az acr credential show --name "$ACR_NAME" --resource-group "$RESOURCE_GROUP" --query 'passwords[0].value' -o tsv)

echo "$ACR_PASSWORD" | docker login "$ACR_LOGIN_SERVER" -u "$ACR_USERNAME" --password-stdin
IMAGE_TAG="$(date +%s)"

echo "  Building frontend..."
docker build -t "${ACR_LOGIN_SERVER}/frontend:${IMAGE_TAG}" -t "${ACR_LOGIN_SERVER}/frontend:latest" \
  -f src/frontend/Dockerfile src/frontend
docker push "${ACR_LOGIN_SERVER}/frontend:${IMAGE_TAG}"
docker push "${ACR_LOGIN_SERVER}/frontend:latest"

echo "  Building risk-api..."
docker build -t "${ACR_LOGIN_SERVER}/risk-api:${IMAGE_TAG}" -t "${ACR_LOGIN_SERVER}/risk-api:latest" \
  -f src/backend/RiskManagement.Api/Dockerfile src/backend
docker push "${ACR_LOGIN_SERVER}/risk-api:${IMAGE_TAG}"
docker push "${ACR_LOGIN_SERVER}/risk-api:latest"

echo "  Building customer-api..."
docker build -t "${ACR_LOGIN_SERVER}/customer-api:${IMAGE_TAG}" -t "${ACR_LOGIN_SERVER}/customer-api:latest" \
  -f src/backend/CustomerManagement.Api/Dockerfile src/backend
docker push "${ACR_LOGIN_SERVER}/customer-api:${IMAGE_TAG}"
docker push "${ACR_LOGIN_SERVER}/customer-api:latest"

echo "Images pushed with tag: ${IMAGE_TAG}"

echo ""
echo "=== Step 4/7: Updating Container Apps with new images ==="
az containerapp update \
  --name "${PREFIX}-frontend" \
  --resource-group "$RESOURCE_GROUP" \
  --image "${ACR_LOGIN_SERVER}/frontend:${IMAGE_TAG}" \
  --output none

az containerapp update \
  --name "${PREFIX}-risk-api" \
  --resource-group "$RESOURCE_GROUP" \
  --image "${ACR_LOGIN_SERVER}/risk-api:${IMAGE_TAG}" \
  --output none

az containerapp update \
  --name "${PREFIX}-customer-api" \
  --resource-group "$RESOURCE_GROUP" \
  --image "${ACR_LOGIN_SERVER}/customer-api:${IMAGE_TAG}" \
  --output none

echo "All container apps updated."

echo ""
echo "=== Step 5/7: Waiting for Keycloak to be ready ==="
KEYCLOAK_URL="https://${KEYCLOAK_FQDN}"
for i in $(seq 1 60); do
  if curl -sf "${KEYCLOAK_URL}/health/ready" > /dev/null 2>&1; then
    echo "Keycloak is ready."
    break
  fi
  if [ "$i" -eq 60 ]; then
    echo "ERROR: Keycloak did not become ready within 5 minutes."
    exit 1
  fi
  echo "  Waiting for Keycloak... (attempt $i/60)"
  sleep 5
done

echo ""
echo "=== Step 6/7: Configuring Keycloak (Realm, Client, Users, Roles) ==="

# Get admin token
TOKEN=$(curl -sf -X POST "${KEYCLOAK_URL}/realms/master/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "username=admin" \
  -d "password=${KEYCLOAK_PASSWORD}" \
  -d "grant_type=password" \
  -d "client_id=admin-cli" | jq -r '.access_token')

if [ -z "$TOKEN" ] || [ "$TOKEN" = "null" ]; then
  echo "ERROR: Failed to get Keycloak admin token."
  exit 1
fi

# Create realm
echo "  Creating realm 'risk-management'..."
curl -sf -X POST "${KEYCLOAK_URL}/admin/realms" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"realm":"risk-management","enabled":true}' || true

# Create client
echo "  Creating client 'risk-management-app'..."
curl -sf -X POST "${KEYCLOAK_URL}/admin/realms/risk-management/clients" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"clientId\": \"risk-management-app\",
    \"name\": \"Risk Management App\",
    \"protocol\": \"openid-connect\",
    \"enabled\": true,
    \"publicClient\": false,
    \"standardFlowEnabled\": true,
    \"directAccessGrantsEnabled\": true,
    \"serviceAccountsEnabled\": false,
    \"redirectUris\": [\"https://${FRONTEND_FQDN}/*\"],
    \"webOrigins\": [\"https://${FRONTEND_FQDN}\"],
    \"defaultClientScopes\": [\"profile\", \"roles\", \"email\"],
    \"attributes\": {
      \"post.logout.redirect.uris\": \"https://${FRONTEND_FQDN}/*\"
    }
  }" || true

# Get client ID (internal UUID)
CLIENT_UUID=$(curl -sf "${KEYCLOAK_URL}/admin/realms/risk-management/clients?clientId=risk-management-app" \
  -H "Authorization: Bearer $TOKEN" | jq -r '.[0].id')

# Get client secret
CLIENT_SECRET=$(curl -sf "${KEYCLOAK_URL}/admin/realms/risk-management/clients/${CLIENT_UUID}/client-secret" \
  -H "Authorization: Bearer $TOKEN" | jq -r '.value')

echo "  Client secret: ${CLIENT_SECRET:0:8}..."

# Create client roles
echo "  Creating client roles..."
for ROLE in applicant processor; do
  curl -sf -X POST "${KEYCLOAK_URL}/admin/realms/risk-management/clients/${CLIENT_UUID}/roles" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d "{\"name\":\"${ROLE}\"}" || true
done

# Get role IDs
APPLICANT_ROLE=$(curl -sf "${KEYCLOAK_URL}/admin/realms/risk-management/clients/${CLIENT_UUID}/roles/applicant" \
  -H "Authorization: Bearer $TOKEN")
PROCESSOR_ROLE=$(curl -sf "${KEYCLOAK_URL}/admin/realms/risk-management/clients/${CLIENT_UUID}/roles/processor" \
  -H "Authorization: Bearer $TOKEN")

# Create users and assign roles
create_user() {
  local USERNAME="$1"
  local EMAIL="$2"
  local FIRST_NAME="$3"
  local LAST_NAME="$4"
  local PASSWORD="$5"
  local ROLE_JSON="$6"

  echo "  Creating user '${USERNAME}'..."
  curl -sf -X POST "${KEYCLOAK_URL}/admin/realms/risk-management/users" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d "{
      \"username\": \"${USERNAME}\",
      \"enabled\": true,
      \"email\": \"${EMAIL}\",
      \"emailVerified\": true,
      \"firstName\": \"${FIRST_NAME}\",
      \"lastName\": \"${LAST_NAME}\",
      \"credentials\": [{\"type\":\"password\",\"value\":\"${PASSWORD}\",\"temporary\":false}]
    }" || true

  # Get user ID
  local USER_ID
  USER_ID=$(curl -sf "${KEYCLOAK_URL}/admin/realms/risk-management/users?username=${USERNAME}" \
    -H "Authorization: Bearer $TOKEN" | jq -r '.[0].id')

  # Assign client role
  curl -sf -X POST "${KEYCLOAK_URL}/admin/realms/risk-management/users/${USER_ID}/role-mappings/clients/${CLIENT_UUID}" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d "[${ROLE_JSON}]" || true
}

create_user "applicant" "applicant@example.com" "Applicant" "User" "applicant" "$APPLICANT_ROLE"
create_user "processor" "processor@example.com" "Processor" "User" "processor" "$PROCESSOR_ROLE"

echo ""
echo "=== Step 7/7: Setting OIDC client secret on frontend ==="
az containerapp update \
  --name "${PREFIX}-frontend" \
  --resource-group "$RESOURCE_GROUP" \
  --set-env-vars "OIDC_CLIENT_SECRET=${CLIENT_SECRET}" \
  --output none

echo ""
echo "============================================="
echo "  Setup complete!"
echo "============================================="
echo ""
echo "  Frontend URL:   https://${FRONTEND_FQDN}"
echo "  Keycloak URL:   https://${KEYCLOAK_FQDN}"
echo "  Risk API:       https://${RISK_API_FQDN}"
echo "  Customer API:   https://${CUSTOMER_API_FQDN}"
echo ""
echo "  Test users:"
echo "    applicant / applicant  (role: applicant)"
echo "    processor / processor  (role: processor)"
echo ""
echo "  Keycloak Admin:"
echo "    admin / <your keycloak password>"
echo ""
