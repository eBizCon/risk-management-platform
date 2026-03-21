## Why

Application creation in RiskManagement currently uses synchronous HTTP calls to CustomerManagement for customer profile data and a synchronous credit check (SCHUFA mock). This creates temporal coupling — if CustomerManagement is unavailable, no applications can be created. In a production system with a real SCHUFA API (1–5s latency), users would wait for the entire chain (HTTP + CreditCheck + Scoring) before getting a response. Moving to an asynchronous, saga-orchestrated flow decouples the services, improves resilience through automatic retries, and provides a realistic DDD showcase for distributed process orchestration with MassTransit.

## What Changes

- Introduce MassTransit with RabbitMQ as message transport for asynchronous process orchestration
- Implement a State Machine Saga (`ApplicationCreationSaga`) that orchestrates the multi-step application creation process: fetch customer profile → perform credit check → finalize application with scoring
- Add new `ApplicationStatus` values: `Processing` (creation in progress) and `Failed` (creation failed after retries)
- Split the synchronous `CreateApplicationHandler` into: a thin command handler that creates a `Processing` application and publishes a saga start event, plus three MassTransit consumers that execute each step (FetchCustomerProfile, PerformCreditCheck, FinalizeApplication)
- Add RabbitMQ to the Aspire AppHost orchestration
- Persist saga state in PostgreSQL via EF Core (MassTransit.EntityFrameworkCore)
- **BREAKING**: `POST /api/applications` returns `202 Accepted` with `{ id, status: "processing" }` instead of `200 OK` with the fully scored application. Frontend must poll for completion.
- Apply the same pattern to `CreateAndSubmitApplicationCommand` (create + submit in one async flow)
- `UpdateApplicationCommand` and `UpdateAndSubmitApplicationCommand` remain synchronous (the application already exists, user expects immediate feedback on edits)

## Capabilities

### New Capabilities
- `saga-application-creation`: MassTransit State Machine Saga orchestrating async application creation with RabbitMQ transport, EF Core saga persistence, and three consumer steps (customer fetch, credit check, finalize)
- `rabbitmq-messaging`: RabbitMQ integration via Aspire hosting and MassTransit transport configuration with retry policies

### Modified Capabilities
- `aspire-orchestration`: Add RabbitMQ resource to AppHost, wire connection reference to RiskManagement.Api
- `application-credit-check`: Credit check is now triggered asynchronously by a MassTransit consumer instead of synchronously in the command handler
- `cqrs-command-split`: CreateApplicationCommand and CreateAndSubmitApplicationCommand handlers become thin starters that publish saga events instead of executing the full flow synchronously

## Impact

- **Backend packages**: MassTransit, MassTransit.RabbitMQ, MassTransit.EntityFrameworkCore added to RiskManagement.Infrastructure; Aspire.Hosting.RabbitMQ added to AppHost
- **Database**: New saga state table in RiskManagement DB, new EF Core migration for saga state + new ApplicationStatus values
- **API contract**: `POST /api/applications` and `POST /api/applications?submit=true` change from 200 → 202 (breaking for frontend)
- **Frontend**: Application creation flow needs polling or status refresh to detect when processing completes; new UI states for "Processing" and "Failed"
- **Infrastructure**: RabbitMQ container added to dev environment (Aspire + docker-compose)
- **Existing sync flows unaffected**: Update, Approve, Reject, Delete, Inquiries remain synchronous
- **ICustomerProfileService and ICreditCheckService interfaces unchanged** — consumers reuse existing service abstractions
