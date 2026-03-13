#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BACKEND_DIR="$SCRIPT_DIR/../src/backend"

if [ -z "$1" ]; then
  echo "Usage: ./dev/add-migration.sh <MigrationName>"
  echo "Example: ./dev/add-migration.sh AddUserTable"
  exit 1
fi

echo "Creating migration '$1'..."
dotnet ef migrations add "$1" --project "$BACKEND_DIR/RiskManagement.Api"
echo "Done. Migration '$1' created."
