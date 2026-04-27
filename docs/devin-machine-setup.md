# Devin machine setup

## Startup commands
```bash
cd ~/repos/risk-management-platform && git pull && git submodule update --init --recursive
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