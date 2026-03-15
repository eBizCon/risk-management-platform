## ADDED Requirements

### Requirement: ServiceDefaults project provides shared infrastructure
The system SHALL provide a `ServiceDefaults` project at `src/backend/ServiceDefaults/` that both API projects reference. It SHALL provide a single `AddServiceDefaults()` extension method on `IHostApplicationBuilder` and a `MapDefaultEndpoints()` extension method on `WebApplication`.

#### Scenario: API projects reference ServiceDefaults
- **WHEN** a developer builds RiskManagement.Api or CustomerManagement.Api
- **THEN** the build SHALL succeed with a ProjectReference to ServiceDefaults

#### Scenario: ServiceDefaults is activated in Program.cs
- **WHEN** an API calls `builder.AddServiceDefaults()` and `app.MapDefaultEndpoints()`
- **THEN** OpenTelemetry, health checks, service discovery, and HTTP resilience SHALL be configured

### Requirement: OpenTelemetry integration
ServiceDefaults SHALL configure OpenTelemetry with OTLP exporter for traces, metrics, and structured logs. When running under Aspire, telemetry data SHALL be sent to the Aspire Dashboard.

#### Scenario: Traces appear in Aspire Dashboard
- **WHEN** an API handles an HTTP request while running under Aspire
- **THEN** a distributed trace SHALL be visible in the Aspire Dashboard
- **AND** the trace SHALL include ASP.NET Core and HTTP client instrumentation

#### Scenario: Graceful degradation without Aspire
- **WHEN** an API runs without Aspire (docker-compose mode)
- **THEN** OpenTelemetry SHALL initialize without errors
- **AND** no telemetry data SHALL be lost or cause exceptions (no-op exporter behavior)

### Requirement: Health check endpoints
ServiceDefaults SHALL register health check endpoints at `/health` (readiness) and `/alive` (liveness) on both APIs.

#### Scenario: Readiness endpoint responds
- **WHEN** a client sends GET to `/health`
- **THEN** the API SHALL respond with HTTP 200 and a health status

#### Scenario: Liveness endpoint responds
- **WHEN** a client sends GET to `/alive`
- **THEN** the API SHALL respond with HTTP 200 indicating the process is alive

### Requirement: HTTP client resilience
ServiceDefaults SHALL configure default resilience policies (retry with exponential backoff, circuit breaker) for all `HttpClient` instances registered via `IHttpClientFactory`.

#### Scenario: Transient failure triggers retry
- **WHEN** an HTTP client call to another service fails with a transient error (e.g., 503)
- **THEN** the resilience policy SHALL retry the request before propagating the failure

### Requirement: Service discovery support
ServiceDefaults SHALL configure `Microsoft.Extensions.ServiceDiscovery` so that Aspire-injected service references resolve to correct URLs at runtime.

#### Scenario: Service reference resolves under Aspire
- **WHEN** an HttpClient is configured with a service name (e.g., `http://customer-api`)
- **THEN** service discovery SHALL resolve it to the actual endpoint URL assigned by Aspire

#### Scenario: Service discovery is inactive without Aspire
- **WHEN** an API runs without Aspire
- **THEN** HttpClient base addresses SHALL fall back to explicitly configured URLs from appsettings
