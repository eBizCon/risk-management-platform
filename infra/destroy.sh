#!/bin/bash
set -euo pipefail

# =============================================================================
# Destroy the entire Risk Management Platform Azure environment
# Deletes the resource group and ALL resources within it
# =============================================================================

RESOURCE_GROUP="${1:?Usage: destroy.sh <resource-group>}"

echo "WARNING: This will permanently delete the resource group '$RESOURCE_GROUP'"
echo "         and ALL resources within it (Container Apps, PostgreSQL, ACR, etc.)."
echo ""
read -p "Are you sure? Type 'yes' to confirm: " CONFIRM

if [ "$CONFIRM" != "yes" ]; then
  echo "Aborted."
  exit 0
fi

echo ""
echo "Deleting resource group '$RESOURCE_GROUP'..."
az group delete --name "$RESOURCE_GROUP" --yes --no-wait

echo ""
echo "Resource group deletion initiated (runs in background)."
echo "It may take a few minutes for all resources to be fully removed."
echo ""
echo "To check status:  az group show --name $RESOURCE_GROUP --query properties.provisioningState -o tsv"
echo "To rebuild:        ./infra/setup.sh $RESOURCE_GROUP <postgres-password> <keycloak-password>"
echo ""
