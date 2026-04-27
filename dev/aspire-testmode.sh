#!/bin/bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

dotnet run --project "$REPO_ROOT/src/backend/AppHost/AppHost.csproj" --launch-profile http-testmode
