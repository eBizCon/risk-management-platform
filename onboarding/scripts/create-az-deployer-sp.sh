#!/bin/bash
set -euo pipefail

SUBSCRIPTION_ID="6c27b627-042a-40d3-9d98-f2456e3e534b"
RESOURCE_GROUP="${1:-riskmgmt-dev}"
SP_NAME="${2:-sp-cli-workshop}"

az account set --subscription "$SUBSCRIPTION_ID"

RG_SCOPE="/subscriptions/${SUBSCRIPTION_ID}/resourceGroups/${RESOURCE_GROUP}"

SP_JSON=$(az ad sp create-for-rbac \
  --name "$SP_NAME" \
  --role "Contributor" \
  --scopes "$RG_SCOPE" \
  --output json)

APP_ID=$(echo "$SP_JSON" | jq -r '.appId')

SP_OBJECT_ID=""
for i in $(seq 1 12); do
  SP_OBJECT_ID=$(az ad sp show --id "$APP_ID" --query id -o tsv 2>/dev/null || true)
  [ -n "$SP_OBJECT_ID" ] && break
  sleep 5
done

if [ -z "$SP_OBJECT_ID" ]; then
  echo "ERROR: Service principal ${APP_ID} not found after 60s." >&2
  exit 1
fi

az role assignment create \
  --assignee-object-id "$SP_OBJECT_ID" \
  --assignee-principal-type ServicePrincipal \
  --role "AcrPush" \
  --scope "$RG_SCOPE" \
  --output none

echo "$SP_JSON" | jq '{appId, password, tenant}'
echo "Roles on ${RG_SCOPE}: Contributor, AcrPush"
