## Why

The backend (`RiskManagement.Api`) uses an anemic domain model where all business logic is scattered across a god-repository (`ApplicationRepository`, ~270 lines), fat controllers, and a disconnected scoring service. Status transitions, invariant checks, and domain rules are duplicated and spread across multiple layers. This makes the codebase hard to reason about, test in isolation, and extend safely. Restructuring to Domain-Driven Design principles centralizes business logic in rich domain objects, enforces invariants at the aggregate level, and establishes clear architectural boundaries.

## What Changes

- **New solution projects**: Split the monolithic `RiskManagement.Api` into 4 projects — `Domain`, `Application`, `Infrastructure`, `Api` — with strict dependency direction (Domain has zero external dependencies)
- **Rich Aggregate Root**: `Application` becomes an aggregate root with behavior methods (`Submit()`, `Approve()`, `Reject()`, `RequestInformation()`, `AnswerInquiry()`, `UpdateDetails()`, `Delete()`) that enforce all status transition invariants
- **Child Entity**: `ApplicationInquiry` becomes a child entity owned by the `Application` aggregate
- **Value Objects**: Replace stringly-typed fields with `ApplicationStatus`, `EmploymentStatus`, `TrafficLight`, and `ScoringResult` value objects
- **Domain Service**: `ScoringService` moves into the Domain layer, operating on domain types instead of primitives
- **Domain Events**: Introduce event infrastructure (`ApplicationSubmittedEvent`, `ApplicationDecidedEvent`, `InquiryCreatedEvent`) on the aggregate base class
- **Thin Repository**: `IApplicationRepository` interface in Domain, implementation in Infrastructure — pure persistence, no business logic
- **Application Layer**: Command/Query handlers orchestrate use cases (validation, loading aggregates, calling domain methods, persisting)
- **Thin Controllers**: Controllers become pure HTTP adapters — deserialize, dispatch to handler, map result to HTTP response
- **Domain Exceptions**: `DomainException` and `InvalidStatusTransitionException` replace ad-hoc error returns; a global `ExceptionHandlingMiddleware` maps them to HTTP status codes
- **BREAKING**: All business logic moves out of `ApplicationRepository` and controllers — any code depending on repository methods like `SubmitApplication()` or `ProcessApplication()` must use the new command handlers instead

## Capabilities

### New Capabilities
- `ddd-domain-model`: Rich aggregate root (Application), child entity (ApplicationInquiry), value objects (ApplicationStatus, EmploymentStatus, TrafficLight, ScoringResult), domain service (ScoringService), domain events, base classes (Entity, AggregateRoot, ValueObject), domain exceptions
- `ddd-application-layer`: Command/Query handler interfaces, command and query objects, handlers for all use cases (CreateApplication, SubmitApplication, UpdateApplication, DeleteApplication, ProcessDecision, CreateInquiry, AnswerInquiry, GetApplication, GetApplicationsByUser, GetProcessorApplications, GetDashboardStats), Result wrapper, DTO mapping
- `ddd-infrastructure`: EF Core repository implementation, entity type configurations, DbContext adjustments for value object mapping, DI registration extension method
- `ddd-api-layer`: Thin controllers, ExceptionHandlingMiddleware, updated Program.cs with multi-project DI wiring

### Modified Capabilities
_(none — no existing specs to modify)_

## Impact

- **Projects**: New `RiskManagement.Domain`, `RiskManagement.Application`, `RiskManagement.Infrastructure` projects added to the solution; `RiskManagement.Api` retains only HTTP/presentation concerns
- **Code**: Every file in `Models/`, `Data/ApplicationRepository.cs`, `Services/ScoringService.cs`, `Validation/`, and all controllers will be restructured or replaced
- **Database**: No schema changes — EF entity configurations move to Infrastructure but map to the same tables/columns
- **Tests**: `RiskManagement.Api.Tests` needs updates for the new project references; new `RiskManagement.Domain.Tests` project for unit-testing aggregate behavior and value objects
- **Dependencies**: Domain project has zero NuGet dependencies; Application references Domain; Infrastructure references Domain + EF Core; Api references Application + Infrastructure
- **API contract**: No changes to HTTP endpoints, request/response shapes, or status codes — this is a pure internal refactoring
