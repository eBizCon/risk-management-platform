## Why

The platform currently requires 4 separate terminal sessions to start development (docker-compose for PostgreSQL + Keycloak, dotnet run for RiskManagement.Api, dotnet run for CustomerManagement.Api, npm for frontend). Service URLs and connection strings are hardcoded across multiple `appsettings.Development.json` and `.env` files. There is no unified observability (logs, traces, metrics) across services. Aspire 13 — released alongside .NET 10 — provides a single orchestration entry point, automatic service discovery, and built-in OpenTelemetry with a dashboard, making the dev experience significantly better for a multi-service DDD platform.

## What Changes

- **New `AppHost` project**: Aspire 13 orchestrator that starts PostgreSQL (with both databases), Keycloak (with realm import), RiskManagement.Api, and CustomerManagement.Api with a single `dotnet run`.
- **New `ServiceDefaults` project**: Shared configuration for OpenTelemetry (traces, metrics, structured logs), health check endpoints (`/health`, `/alive`), and HTTP resilience (retry/circuit-breaker via Polly).
- **Both API `Program.cs` updated**: Add `builder.AddServiceDefaults()` and `app.MapDefaultEndpoints()` to wire up observability and health checks.
- **Both API `.csproj` updated**: Add `ProjectReference` to `ServiceDefaults`.
- **Solution file updated**: `RiskManagementPlatform.slnx` includes AppHost and ServiceDefaults projects.
- **Connection strings via Aspire**: Aspire injects connection strings automatically via environment variables. Existing `appsettings.Development.json` values remain as fallback for non-Aspire (docker-compose) mode.
- **Service-to-service URLs via Aspire**: `WithReference(customerApi)` replaces hardcoded `CUSTOMER_SERVICE_URL`. Fallback values in appsettings remain for docker-compose mode.
- **docker-compose preserved**: The existing `dev/docker-compose.yml` continues to work as a fallback for CI or non-Aspire workflows. No breaking changes to the current flow.
- **Frontend excluded**: SvelteKit frontend stays separate (`npm run dev`), not managed by Aspire.

## Capabilities

### New Capabilities
- `aspire-orchestration`: AppHost project that orchestrates all backend infrastructure (PostgreSQL, Keycloak) and .NET services with dependency ordering, automatic connection string injection, and service discovery.
- `service-defaults`: Shared ServiceDefaults project providing OpenTelemetry exporters, health check endpoints, and HTTP client resilience policies for all .NET services.

### Modified Capabilities

## Impact

- **New projects**: `src/backend/AppHost/`, `src/backend/ServiceDefaults/`
- **Modified files**: `RiskManagement.Api/Program.cs`, `CustomerManagement.Api/Program.cs`, both `.csproj` files, `RiskManagementPlatform.slnx`
- **New dependencies**: `Aspire.AppHost.Sdk/13.0.0`, `Aspire.Hosting.PostgreSQL`, `Aspire.Hosting.Keycloak`, OpenTelemetry packages, `Microsoft.Extensions.Http.Resilience`, `Microsoft.Extensions.ServiceDiscovery`
- **Prerequisites**: .NET 10 SDK (already in use), Docker (already required for docker-compose), Aspire CLI (`curl -sSL https://aspire.dev/install.sh | bash`)
- **No breaking changes**: docker-compose workflow continues to work unchanged
