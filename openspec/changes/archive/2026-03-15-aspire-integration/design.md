## Context

The risk-management-platform consists of 5 independently started components:

1. **PostgreSQL** (container, port 5432) — hosts `risk_management` and `customer_management` databases
2. **Keycloak** (container, port 8081) — OIDC identity provider with realm import
3. **RiskManagement.Api** (.NET 10, port 5227) — credit application processing
4. **CustomerManagement.Api** (.NET 10, port 5000) — customer CRUD + credit reports
5. **SvelteKit Frontend** (Node.js, port 5173) — BFF pattern, proxies to both APIs

Currently started via `docker-compose up` (PostgreSQL + Keycloak) plus 2x `dotnet run` plus `npm run dev`. Service URLs and connection strings are manually maintained in `appsettings.Development.json` and `.env` files. No unified observability across services.

Both APIs use Clean Architecture (Domain/Application/Infrastructure/Api layers), a custom CQRS Dispatcher, EF Core with Npgsql, and JWT Bearer authentication via Keycloak.

## Goals / Non-Goals

**Goals:**
- Single `dotnet run` in AppHost starts PostgreSQL, Keycloak, and both .NET APIs
- Aspire Dashboard provides aggregated logs, distributed traces, and metrics for all services
- Connection strings and service URLs are automatically injected by Aspire, eliminating manual config drift
- Health check endpoints (`/health`, `/alive`) on both APIs
- HTTP client resilience (retry, circuit-breaker) for service-to-service calls
- docker-compose continues to work as an independent fallback

**Non-Goals:**
- Frontend orchestration via Aspire (SvelteKit stays separate)
- Production deployment via Aspire (deployment remains via existing Bicep/Azure infra)
- Replacing docker-compose for CI pipelines
- Custom Aspire dashboard extensions or resource commands

## Decisions

### D1: Aspire 13 with simplified SDK declaration

Use the Aspire 13 `Aspire.AppHost.Sdk/13.0.0` as the project SDK directly, not the older two-SDK pattern from Aspire 9.x.

```xml
<Project Sdk="Aspire.AppHost.Sdk/13.0.0">
```

**Rationale**: Aspire 13 is the current stable release targeting net10.0. The simplified SDK removes the need for an explicit `Aspire.Hosting.AppHost` package reference. The project already targets net10.0, so compatibility is guaranteed.

**Alternative considered**: Aspire 9.5.x (supports net9.0/net10.0) — rejected because Aspire 13 is the actively supported version and 9.x will reach end-of-support.

### D2: Keycloak via Aspire.Hosting.Keycloak with realm import

Use the official `Aspire.Hosting.Keycloak` hosting package with `WithRealmImport()` to mount the existing realm export directory.

```csharp
var keycloak = builder.AddKeycloak("keycloak", 8081)
    .WithDataVolume()
    .WithRealmImport("../../dev/keycloak/import");
```

**Rationale**: The package provides first-class Keycloak support including health checks and service discovery. The realm import path points to the existing `dev/keycloak/import/` directory, reusing the current realm configuration without duplication.

**Alternative considered**: Generic `AddContainer()` with manual port/volume config — rejected because the dedicated package handles health probes and Keycloak-specific configuration.

### D3: Two databases on a single PostgreSQL instance

```csharp
var postgres = builder.AddPostgres("pg").WithDataVolume();
var riskDb = postgres.AddDatabase("risk-management");
var customerDb = postgres.AddDatabase("customer-management");
```

**Rationale**: Matches the current docker-compose setup (single PG instance, two DBs). Aspire's `AddDatabase()` creates the databases automatically, replacing the `init-db.sql` script for the Aspire flow. The init script remains for docker-compose.

**Alternative considered**: Two separate PostgreSQL instances — rejected because it increases resource usage without benefit for local dev.

### D4: Connection string naming strategy

Name Aspire database resources to match the existing `GetConnectionString("DefaultConnection")` calls in both APIs. This is achieved by using `WithEnvironment()` to map Aspire's generated connection strings to `ConnectionStrings__DefaultConnection`.

```csharp
var customerApi = builder.AddProject<Projects.CustomerManagement_Api>("customer-api")
    .WithReference(customerDb, "DefaultConnection");
```

The second parameter of `WithReference` sets the connection string name, so `GetConnectionString("DefaultConnection")` resolves correctly.

**Rationale**: Zero changes needed in the existing `Program.cs` connection string resolution. The appsettings fallback values continue to work for docker-compose mode.

**Alternative considered**: Rename `GetConnectionString()` calls to use semantic names like `risk-management` — rejected because it's unnecessary churn and doesn't improve the Aspire integration.

### D5: Service-to-service discovery via WithReference

RiskManagement.Api calls CustomerManagement.Api via `HttpClient`. With Aspire:

```csharp
builder.AddProject<Projects.RiskManagement_Api>("risk-api")
    .WithReference(customerApi);
```

This injects the customer API's URL as a service reference. The `DependencyInjection.cs` code that configures `HttpClient` base addresses needs to resolve service URLs from configuration, falling back to hardcoded values when not running under Aspire.

**Rationale**: Aspire's service discovery eliminates hardcoded URLs and handles dynamic port assignment. The existing `CUSTOMER_SERVICE_URL` / `APPLICATION_SERVICE_URL` configuration keys continue to work as fallback.

### D6: ServiceDefaults project structure

A shared `ServiceDefaults` project provides:

1. **OpenTelemetry**: OTLP exporter for traces, metrics, and logs → Aspire Dashboard
2. **Health checks**: Default `/health` (readiness) and `/alive` (liveness) endpoints
3. **HTTP resilience**: Standard retry and circuit-breaker policies via `Microsoft.Extensions.Http.Resilience`
4. **Service discovery**: `Microsoft.Extensions.ServiceDiscovery` for resolving Aspire-injected service references

Both API projects reference ServiceDefaults and call `builder.AddServiceDefaults()` + `app.MapDefaultEndpoints()`.

**Rationale**: Centralizes cross-cutting infrastructure concerns. When not running under Aspire, OpenTelemetry gracefully degrades (no exporter endpoint → no-op), and health checks still work.

### D7: Dual-mode operation (Aspire vs docker-compose)

Both modes coexist without conflict:

| Concern | Aspire mode | docker-compose mode |
|---|---|---|
| PostgreSQL | Aspire container | docker-compose container |
| Keycloak | Aspire container | docker-compose container |
| Connection strings | Injected by Aspire | `appsettings.Development.json` |
| Service URLs | Service discovery | `appsettings.Development.json` |
| Observability | Aspire Dashboard | None (or manual setup) |
| Start command | `dotnet run --project AppHost` | `docker compose up` + 2x `dotnet run` |

No conditional code needed — Aspire overrides environment variables when active, appsettings provide defaults when not.

## Risks / Trade-offs

- **Port conflicts**: If docker-compose containers are already running on ports 5432/8081, Aspire's containers will fail to bind. → Mitigation: Document that developers should use either Aspire OR docker-compose, not both simultaneously.
- **Aspire CLI dependency**: Developers need the Aspire CLI installed. → Mitigation: Document installation in README. The CLI is a one-time install (`curl -sSL https://aspire.dev/install.sh | bash`).
- **Keycloak startup time**: Keycloak takes 10-20s to start with realm import. `WaitFor(keycloak)` ensures APIs don't start before Keycloak is ready, but initial startup is slower than docker-compose (which starts everything in parallel without dependency ordering). → Acceptable trade-off for guaranteed readiness.
- **OpenTelemetry overhead**: Minimal in dev mode. Traces/metrics add negligible latency. → No mitigation needed.
- **ServiceDefaults couples APIs to Aspire packages**: The ServiceDefaults project references OpenTelemetry and Aspire-related packages. → Mitigation: These are lightweight, well-maintained Microsoft packages. When not running under Aspire, they degrade gracefully.
