## Context

Application creation in RiskManagement follows a synchronous chain: validate → HTTP fetch customer profile → credit check → scoring → save. This is implemented in `CreateApplicationHandler` and `CreateAndSubmitApplicationHandler`, both injecting `ICustomerProfileService` (HTTP to CustomerManagement) and `ICreditCheckService` (SCHUFA mock). The system runs on .NET 10 with Aspire orchestration, PostgreSQL, and Keycloak. There is no message broker in the current infrastructure.

The existing `IDispatcher` handles in-process CQRS (commands, queries, domain events). Domain events are published after save within the same process. There is no cross-service event infrastructure.

## Goals / Non-Goals

**Goals:**
- Decouple application creation from synchronous availability of CustomerManagement
- Demonstrate Saga/Process Manager pattern as a DDD teaching example for distributed process orchestration
- Introduce MassTransit + RabbitMQ as the messaging backbone for future cross-context communication
- Keep the existing synchronous flows (Update, Approve, Reject, Delete, Inquiries) unchanged
- Persist saga state in PostgreSQL alongside domain data for operational visibility

**Non-Goals:**
- Replace all HTTP communication with messaging (ICustomerNameService, ICustomerProfileService for Update flows remain HTTP)
- Introduce event-driven architecture for CustomerManagement → RiskManagement data sync (no CustomerUpdatedEvent consumption)
- Replace the existing IDispatcher with MassTransit for in-process domain events
- Implement SignalR/WebSocket for real-time frontend updates (polling is sufficient)
- Make UpdateApplicationCommand or UpdateAndSubmitApplicationCommand asynchronous

## Decisions

### D1: MassTransit as Saga Framework

**Decision:** Use MassTransit's built-in `MassTransitStateMachine<T>` for saga orchestration.

**Alternatives considered:**
- **Wolverine**: Lighter weight, good saga support, but smaller ecosystem and less battle-tested EF Core integration
- **Custom IHostedService + Outbox**: Minimal dependencies, but requires building state machine, retry, and persistence from scratch
- **NServiceBus**: Enterprise-grade but commercial license

**Rationale:** MassTransit is the de-facto open-source standard for .NET messaging. It has first-class EF Core saga persistence, built-in retry policies, and excellent Aspire integration via `Aspire.Hosting.RabbitMQ`. The State Machine DSL makes the process flow explicit and testable.

### D2: RabbitMQ as Transport

**Decision:** Use RabbitMQ as message transport, managed via Aspire.

**Alternatives considered:**
- **InMemory transport**: Zero infrastructure, but no durability — messages lost on restart, not suitable for demonstrating production patterns
- **Azure Service Bus**: Cloud-native, but requires Azure subscription and doesn't work offline
- **Redis Streams**: Lightweight, but MassTransit Redis transport is less mature

**Rationale:** RabbitMQ is the standard local-dev transport for MassTransit. Aspire has a first-class `AddRabbitMQ()` hosting integration with data volumes. It demonstrates a realistic production setup while running fully local.

### D3: Saga State in Separate EF Core Table

**Decision:** Saga state is persisted in a dedicated `saga_application_creation_state` table in the RiskManagement database, managed by MassTransit's EF Core repository integration using the existing `ApplicationDbContext`.

**Alternatives considered:**
- **InMemory saga repository**: Simple, but state lost on restart — incomplete sagas become orphans
- **Separate DbContext for saga**: Clean separation, but unnecessary complexity given shared database
- **MongoDB saga repository**: Good for document-style state, but adds a new database dependency

**Rationale:** Reusing the existing `ApplicationDbContext` keeps the migration pipeline unified. MassTransit's `EntityFrameworkRepository` with `ExistingDbContext<T>()` is designed for exactly this pattern. The saga table is infrastructure, not domain — it lives alongside but separate from the `applications` table.

### D4: Choreography via Publish, Not Direct Send

**Decision:** The saga publishes command messages to consumers via `Publish()`. Consumers respond with result events via `Publish()`. The saga correlates responses by `CorrelationId`.

**Alternatives considered:**
- **Request/Response pattern**: Saga sends a request, consumer responds directly. Tighter coupling, harder to add observers.
- **Direct Send to queues**: Requires knowing queue names in the saga. Less flexible.

**Rationale:** Publish/subscribe with CorrelationId is the idiomatic MassTransit saga pattern. Each consumer is independently deployable and testable. The saga orchestrates without knowing consumer implementation details.

### D5: Fat Messages for FinalizeApplication

**Decision:** The `FinalizeApplication` message carries all accumulated data (customer profile fields, credit check results, application input data) so the consumer can finalize the application without reading saga state.

**Alternatives considered:**
- **Thin message + saga state DB read**: Consumer reads saga state table directly. Creates coupling between consumer and saga persistence.
- **Thin message + re-fetch from services**: Consumer re-fetches customer profile and re-runs credit check. Defeats the purpose of the saga collecting data.

**Rationale:** Fat messages make consumers self-contained and independently testable. The data size is small (a few hundred bytes). No coupling to saga persistence internals.

### D6: Processing and Failed as New ApplicationStatus Values

**Decision:** Add `ApplicationStatus.Processing` and `ApplicationStatus.Failed` to the existing `Enumeration<ApplicationStatus>`. The Application aggregate gets a `CreateProcessing()` factory method (creates a minimal application in Processing state) and a `Finalize()` method (transitions from Processing to Draft with all data).

**Alternatives considered:**
- **No new status — use nullable fields**: Application created with null Score/CreditReport, frontend infers "processing". Breaks existing invariants and makes the state implicit.
- **Separate ProcessingApplication entity**: Two aggregate types for the same concept. Unnecessary complexity.

**Rationale:** Explicit status values make the state machine visible in the domain. `Processing` clearly communicates "saga in progress". `Failed` enables error recovery UX. The aggregate guards valid transitions: `Processing → Draft` (on success), `Processing → Failed` (on error).

### D7: Only Create Flows Become Async

**Decision:** Only `CreateApplicationCommand` and `CreateAndSubmitApplicationCommand` use the saga. `UpdateApplicationCommand` and `UpdateAndSubmitApplicationCommand` remain synchronous.

**Rationale:** Update commands operate on an existing, loaded application. The user has the form open, expects immediate validation feedback, and the application already has a CustomerId (so customer data can be re-fetched). The latency risk is acceptable for updates. Making updates async would require complex state merging (what if the saga overwrites user edits?).

### D8: Frontend Uses Polling

**Decision:** After receiving `202 Accepted`, the frontend polls `GET /api/applications/{id}` every 2 seconds until `status !== "processing"`.

**Alternatives considered:**
- **SignalR**: Real-time push, but requires new infrastructure (SignalR hub, connection management)
- **Server-Sent Events**: Simpler than SignalR, but still requires backend streaming endpoint
- **No polling — redirect to list**: User navigates to list and sees the application when ready. Poor UX.

**Rationale:** Polling is the simplest approach. The saga completes in < 5 seconds (mock credit check). A 2-second poll interval means at most one extra request. No new infrastructure needed. Can be upgraded to SignalR later.

## Risks / Trade-offs

- **Increased infrastructure complexity** → RabbitMQ container must run alongside PostgreSQL and Keycloak. Mitigated by Aspire orchestration managing the dependency automatically.
- **Eventually consistent application state** → User sees "Processing" for 1–5 seconds instead of immediate result. Mitigated by polling with clear UI feedback.
- **Saga failure leaves orphaned Processing applications** → If saga fails permanently after max retries, application stays in "Failed" state. Mitigated by `Failed` status with clear error message; user can retry or delete.
- **MassTransit learning curve** → New framework for the project. Mitigated by using the standard patterns (StateMachine, EF Core repository, RabbitMQ) with no custom extensions.
- **Dual flow complexity** → Create is async, Update is sync. Different code paths for similar operations. Mitigated by keeping the async consumers thin and reusing existing domain services (IScoringService, ICreditCheckService).
- **Docker-compose compatibility** → RabbitMQ must also be added to `dev/docker-compose.yml` for non-Aspire development. Mitigated by adding a RabbitMQ service to docker-compose alongside the existing PostgreSQL and Keycloak services.
- **Message ordering** → RabbitMQ does not guarantee strict ordering across queues. Not a risk here since each saga instance processes messages sequentially via CorrelationId partitioning.
