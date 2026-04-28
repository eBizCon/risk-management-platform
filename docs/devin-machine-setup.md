# Devin machine setup

## Startup commands
```bash
cd ~/repos/risk-management-platform && git pull && git submodule update --init --recursive
```


## Install Dependenciees
```bash
1. curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash 2>&1
2. az config set extension.dynamic_install_allow_preview=true
3. az extension add --name log-analytics
```
## Update Dependencies
```bash
1. cd ~/repos/risk-management-platform/src/frontend && npm install
2. dotnet restore ~/repos/risk-management-platform/src/backend/RiskManagementPlatform.slnx
3. dotnet build ~/repos/risk-management-platform/src/backend/RiskManagementPlatform.slnx
```

## Run unit tests
```bash
1. dotnet test ~/repos/risk-management-platform/src/backend/RiskManagement.Api.Tests/RiskManagement.Api.Tests.csproj -c Debug -v minimal
2. cd ~/repos/risk-management-platform/src/frontend && npm test
```

## Testing instructions
```md
Run the app locally via Aspire for E2E testing.

Start commands:

1) Start backend stack with Aspire:
dotnet run --project ~/repos/risk-management-platform/src/backend/AppHost/AppHost.csproj

2) In a second terminal, start frontend:
cd ~/repos/risk-management-platform/src/frontend
cp .env.test .env
npm run dev --  --port 5173

Wait until:
- Keycloak is healthy
- Frontend on http://localhost:5173 is reachable
- APIs are healthy in Aspire dashboard

E2E test command:
cd ~/repos/risk-management-platform/src/frontend
npm run test:e2e:ci

Login for browser-based flows:
- applicant / applicant
- processor / processor

```

# Playbook
## Azure Monitor Alert Remediation
## Overview

Handle incoming Azure Monitor Alerts by following this logic to ensure rapid root cause analysis and resolution.

## Procedure

1. **Payload Parsing**: Parse the provided JSON payload to identify the failing service and the specific log query associated with the alert.

2. **Log Extraction**: Use the Azure CLI to fetch the last 10 relevant exceptions surrounding the `firedDateTime`. Ensure the time window is accurately captured.

3. **Source Mapping**: Cross-reference the extracted stack trace with the local source code in the repository to pinpoint the exact line and file causing the issue.

4. **Root Cause Analysis (RCA)**: Identify the root cause. Distinguish clearly between a code bug, a configuration mismatch, or an infrastructure-related issue.

5. **GitHub Documentation**: Create a GitHub Issue with a clear 'Root Cause Analysis' section and a 'Proposed Fix' section.

6. **Automated Remediation**: If the fix is trivial (e.g., a simple configuration update or a one-line bug fix), offer to create a Pull Request immediately.

## Advice & Pointers

* **Context is Key**: When fetching logs, look for correlated events just before the exception to understand the state leading up to the crash.
* **CLI Efficiency**: Use `az monitor app-insights query` for deep inspection if the standard alert log is insufficient.
* **Local Env**: Check for environment variable mismatches in the local `.env` or config files compared to the identified root cause.

## Forbidden actions

* **No Manual Portal Changes**: Do not apply fixes directly via the Azure Portal. All changes must go through the repository/PR workflow.
* **No Silent Failures**: Do not ignore an alert even if it seems transient; always document the RCA in an issue.
* **No Destructive CLI commands**: Do not run any Azure CLI commands that delete resources or clear production logs.