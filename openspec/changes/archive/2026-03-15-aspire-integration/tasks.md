## 1. ServiceDefaults Project

- [x] 1.1 Create `src/backend/ServiceDefaults/ServiceDefaults.csproj` targeting `net10.0` with packages: `Microsoft.Extensions.Http.Resilience`, `Microsoft.Extensions.ServiceDiscovery`, `OpenTelemetry.Exporter.OpenTelemetryProtocol`, `OpenTelemetry.Extensions.Hosting`, `OpenTelemetry.Instrumentation.AspNetCore`, `OpenTelemetry.Instrumentation.Http`, `OpenTelemetry.Instrumentation.Runtime`
- [x] 1.2 Create `src/backend/ServiceDefaults/Extensions.cs` with `AddServiceDefaults(this IHostApplicationBuilder)` and `MapDefaultEndpoints(this WebApplication)` extension methods. Configure OpenTelemetry (OTLP exporter, ASP.NET Core + HTTP + Runtime instrumentation), health checks (`/health`, `/alive`), service discovery, and HTTP resilience defaults.

## 2. AppHost Project

- [x] 2.1 Create `src/backend/AppHost/AppHost.csproj` with `Sdk="Aspire.AppHost.Sdk/13.0.0"`, targeting `net10.0`, with packages: `Aspire.Hosting.PostgreSQL`, `Aspire.Hosting.Keycloak`. Add ProjectReferences to `RiskManagement.Api` and `CustomerManagement.Api`.
- [x] 2.2 Create `src/backend/AppHost/Program.cs` with: PostgreSQL resource (`pg`) with data volume and two databases (`risk-management`, `customer-management`), Keycloak resource on port 8081 with realm import from `../../dev/keycloak/import`, CustomerManagement.Api project with references to `customer-management` DB (named `DefaultConnection`) and Keycloak, RiskManagement.Api project with references to `risk-management` DB (named `DefaultConnection`), Keycloak, and CustomerManagement.Api. Use `WaitFor()` for dependency ordering.

## 3. Integrate ServiceDefaults into Existing APIs

- [x] 3.1 Add `ProjectReference` to `ServiceDefaults` in `RiskManagement.Api/RiskManagement.Api.csproj`
- [x] 3.2 Add `builder.AddServiceDefaults()` at the start and `app.MapDefaultEndpoints()` before `app.Run()` in `RiskManagement.Api/Program.cs`
- [x] 3.3 Add `ProjectReference` to `ServiceDefaults` in `CustomerManagement.Api/CustomerManagement.Api.csproj`
- [x] 3.4 Add `builder.AddServiceDefaults()` at the start and `app.MapDefaultEndpoints()` before `app.Run()` in `CustomerManagement.Api/Program.cs`

## 4. Solution File Update

- [x] 4.1 Add AppHost and ServiceDefaults projects to `RiskManagementPlatform.slnx` under a new `/Aspire/` solution folder

## 5. Verification

- [x] 5.1 Verify `dotnet build` succeeds for the entire solution
- [x] 5.2 Verify `dotnet run --project src/backend/AppHost` starts the Aspire Dashboard and all services (PostgreSQL, Keycloak, both APIs)
- [x] 5.3 Verify both APIs still start correctly without Aspire (using `dotnet run` directly with docker-compose for infrastructure)
