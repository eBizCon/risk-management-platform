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

### 1. Payload Parsing

Parse the provided JSON payload using the [Azure Monitor Common Alert Schema](https://learn.microsoft.com/en-us/azure/azure-monitor/alerts/alerts-common-schema). Extract these fields **before** any log queries:

| Field | Path in payload | Purpose |
|---|---|---|
| Failing resource | `data.essentials.alertTargetIDs` and `data.essentials.configurationItems` | Identifies the exact App Insights / resource |
| Alert window start | `data.alertContext.condition.windowStartTime` | **Exact** start of the evaluation window |
| Alert window end | `data.alertContext.condition.windowEndTime` | **Exact** end of the evaluation window |
| Search query | `data.alertContext.condition.allOf[*].searchQuery` | The KQL query that matched |
| Link to results | `data.alertContext.condition.allOf[*].linkToFilteredSearchResultsUI` | Direct portal link to filtered results |
| Link to search results | `data.alertContext.condition.allOf[*].linkToSearchResultsUI` | Direct portal link to search results |
| Fired timestamp | `data.essentials.firedDateTime` | When the alert fired |
| Severity | `data.essentials.severity` | Alert severity (Sev0–Sev4) |
| Custom properties | `data.customProperties` | Any user-defined context attached to the alert rule |

> **Critical**: If the payload contains `windowStartTime` / `windowEndTime`, those define the **only** time window you are allowed to query in the next step. Do not widen the window or use `firedDateTime` as a rough center point.

### 2. Log Extraction

Use the Azure CLI to fetch exceptions. **Always use the exact alert window and query from the payload.**

```bash
# Preferred: replay the alert's own KQL query against the exact window
az monitor app-insights query \
  --app <resource-name-from-configurationItems> \
  --analytics-query "<searchQuery from payload>" \
  --start-time <windowStartTime> \
  --end-time <windowEndTime>
```

If the payload does not contain an explicit `searchQuery`, fall back to:

```bash
az monitor app-insights query \
  --app <resource-name> \
  --analytics-query "exceptions | where timestamp between(datetime('<windowStartTime>') .. datetime('<windowEndTime>')) | order by timestamp desc | take 20" \
  --start-time <windowStartTime> \
  --end-time <windowEndTime>
```

**Validation step**: After fetching results, verify that the exceptions you found actually match the alert's condition (e.g. same exception type, same `customDimensions` filter, same count threshold). If the results are empty or don't match, re-check your time window and query parameters — do **not** silently widen the search.

### 3. Source Mapping

Cross-reference the extracted stack trace with the local source code in the repository to pinpoint the exact line and file causing the issue.

- Map namespaces/class names in the stack trace to files in the repo.
- If the stack trace points to framework/library code, look for the application code frame that called into it.

### 4. Root Cause Analysis (RCA)

Identify the root cause. Distinguish clearly between:
- **Code bug** — logic error, null reference, unhandled edge case
- **Configuration mismatch** — wrong connection string, missing env var, feature flag
- **Infrastructure issue** — resource unavailable, throttling, network partition
- **Transient / external** — downstream service timeout, rate limiting (still document)

Always include the **exact exception message and type** in your analysis.

### 5. GitHub Documentation

Create a GitHub Issue with these sections:
- **Alert Details**: severity, fired time, resource, alert rule name
- **Root Cause Analysis**: what happened and why
- **Impact**: which users/services were affected
- **Proposed Fix**: concrete steps or code changes
- **Evidence**: relevant log excerpts, timestamps, query results

### 6. Automated Remediation

If the fix is trivial (e.g., a simple configuration update or a one-line bug fix), offer to create a Pull Request immediately. For non-trivial fixes, document the proposed approach in the issue and let the user decide.

## Advice & Pointers

* **Strict time window**: The single most common mistake is querying a wider time window than the alert specifies, which returns unrelated exceptions. Always use `windowStartTime` / `windowEndTime` from the payload.
* **Use the alert's own query**: The `searchQuery` in the payload is the exact KQL that triggered the alert. Re-run it first; only write custom queries if that field is missing.
* **Context is Key**: When fetching logs, also look for correlated events (requests, dependencies, traces) just before the exception to understand the state leading up to the crash.
* **CLI Efficiency**: Use `az monitor app-insights query` for deep inspection if the standard alert log is insufficient.
* **Portal links**: The payload often includes `linkToFilteredSearchResultsUI` — include this link in the GitHub Issue for easy verification.

## Forbidden Actions

* **No Manual Portal Changes**: Do not apply fixes directly via the Azure Portal. All changes must go through the repository/PR workflow.
* **No Silent Failures**: Do not ignore an alert even if it seems transient; always document the RCA in an issue.
* **No Destructive CLI Commands**: Do not run any Azure CLI commands that delete resources or clear production logs.
* **No Window Widening**: Do not expand the query time window beyond what the alert payload specifies without explicitly telling the user and getting confirmation.