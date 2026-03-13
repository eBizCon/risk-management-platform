## Context

The `RiskManagement.Api` backend is a single ASP.NET Core 8 project with a flat structure: Controllers, Models (anemic), Data (god-repository), Services, Validation, Extensions. All business logic — status transitions, scoring, invariant checks, ownership validation — is spread across `ApplicationRepository` (~270 lines) and controller actions. The domain consists of one core aggregate (`Application`) with a child entity (`ApplicationInquiry`) and a stateless domain service (`ScoringService`).

Current project structure:
```
RiskManagement.sln
├── RiskManagement.Api/          (everything in one project)
│   ├── Controllers/             (fat — contain business rules)
│   ├── Models/                  (anemic — pure data containers)
│   ├── Data/                    (god-repository + DbContext)
│   ├── Services/                (ScoringService)
│   ├── Validation/              (FluentValidation)
│   ├── Extensions/              (Auth, Claims)
│   └── Migrations/
└── RiskManagement.Api.Tests/
```

Constraints:
- Database schema (PostgreSQL) must not change — same tables, columns, types
- HTTP API contract (endpoints, request/response shapes) must remain identical
- OIDC authentication flow and cookie-based session must remain in the Api project
- EF Core 8 with Npgsql is the ORM — no change
- FluentValidation remains for input validation

## Goals / Non-Goals

**Goals:**
- Centralize all business rules (status transitions, invariants, scoring) in a rich `Application` aggregate root
- Establish strict dependency direction: Domain ← Application ← Infrastructure/Api
- Domain project has zero external NuGet dependencies
- Every status transition is enforced by the aggregate — impossible to create invalid state from outside
- Repository becomes a thin persistence adapter (load, save, query)
- Controllers become thin HTTP adapters (deserialize, dispatch, respond)
- Application layer orchestrates use cases via command/query handlers
- Enable isolated unit testing of domain logic without database or HTTP

**Non-Goals:**
- No CQRS with separate read/write databases
- No event sourcing — domain events are in-memory only (no event store)
- No MediatR or other mediator library — plain interfaces with DI
- No database schema migration — this is a pure code refactoring
- No API contract changes — external behavior stays identical
- No frontend changes
- No new features — purely structural

## Decisions

### 1. Four-project solution structure

**Decision**: Split into `RiskManagement.Domain`, `RiskManagement.Application`, `RiskManagement.Infrastructure`, `RiskManagement.Api`.

**Rationale**: Clean separation of concerns with enforceable dependency rules via project references. Domain can never accidentally depend on EF Core or ASP.NET.

**Alternative considered**: Folder-based separation within one project — rejected because it doesn't enforce dependency direction at compile time.

**Dependency graph:**
```
Api → Application → Domain
Api → Infrastructure → Domain
```

### 2. Lightweight command/query handlers without MediatR

**Decision**: Define `ICommandHandler<TCommand, TResult>` and `IQueryHandler<TQuery, TResult>` interfaces. Each use case gets its own handler class. Register all handlers via DI in `Program.cs`.

**Rationale**: The application has ~12 use cases total. MediatR adds pipeline complexity (behaviors, notifications) that isn't needed. Plain DI keeps it simple, debuggable, and explicit.

**Alternative considered**: MediatR — rejected as over-engineering for this scope. Can be introduced later if cross-cutting concerns (logging, validation pipelines) justify it.

### 3. Result<T> pattern instead of exceptions for expected failures

**Decision**: Command handlers return `Result<T>` (success/failure wrapper) for expected business failures (e.g., "application not found", "invalid status transition"). Domain exceptions (`DomainException`, `InvalidStatusTransitionException`) are reserved for programming errors / invariant violations that should never happen if the application layer is correct. `ExceptionHandlingMiddleware` catches any uncaught domain exceptions and maps them to 400/500.

**Rationale**: Expected failures (not found, unauthorized, validation) are control flow, not exceptional. Using Result makes the happy/unhappy paths explicit in handler signatures.

### 4. Aggregate loads its child entities

**Decision**: `IApplicationRepository.GetByIdAsync()` always loads the `Application` with its `Inquiries` collection (eager loading). The aggregate owns and manages its inquiries.

**Rationale**: `ApplicationInquiry` is part of the `Application` aggregate boundary. Loading them together ensures the aggregate can enforce invariants like "only one open inquiry at a time".

**Alternative considered**: Lazy loading — rejected because it hides DB calls and makes the aggregate boundary ambiguous.

### 5. Value objects as single-value wrappers mapped via EF conversions

**Decision**: `ApplicationStatus`, `EmploymentStatus`, and `TrafficLight` are C# `record` types (or enums) with implicit/explicit conversion. EF maps them via `HasConversion<>()` to their string database columns. `ScoringResult` is a value object holding `Score`, `TrafficLight`, and serialized `Reasons`.

**Rationale**: No schema changes. EF value conversions handle the mapping transparently. The domain works with type-safe values.

### 6. Domain events as in-memory collection on AggregateRoot

**Decision**: `AggregateRoot` base class has a `List<IDomainEvent>` that aggregate methods populate. The application layer (or a save-changes interceptor) dispatches events after persistence. Initially, events are for extensibility — no handlers wired up in this refactoring.

**Rationale**: Establishes the pattern for future use (notifications, audit trail) without adding complexity now.

### 7. Validation stays in Application layer with FluentValidation

**Decision**: `ApplicationValidator` and `ProcessorDecisionValidator` move to the Application layer. They validate DTOs/commands before the handler touches the domain.

**Rationale**: Input validation (field lengths, required fields, format) is an application concern. Domain invariants (status transitions, business rules) are enforced by the aggregate itself. Two distinct validation layers.

### 8. Auth/OIDC stays in Api project

**Decision**: `AuthenticationExtensions`, `ClaimsPrincipalExtensions`, `RoleClaimHelper`, `OidcOptions`, `AuthController`, `TestSessionController`, and `JsonAuthorizationResultHandler` remain in `RiskManagement.Api`.

**Rationale**: These are purely HTTP/ASP.NET concerns. The application layer receives the user identity as a simple string (email) parameter on commands — no dependency on `ClaimsPrincipal`.

## Risks / Trade-offs

- **Big-bang risk** → All changes land at once. Mitigation: Ensure all existing API tests pass after refactoring. Add domain unit tests for aggregate behavior before/during the refactoring.
- **EF Core value object mapping complexity** → `HasConversion` can have quirks with queries. Mitigation: Keep value objects simple (single underlying type). Test that existing queries still generate correct SQL.
- **Increased file count** → ~40+ new files across 3 new projects. Trade-off accepted for compile-time enforced boundaries and testability.
- **Learning curve** → Team needs to understand aggregate pattern and command/handler flow. Mitigation: Clear naming conventions, documented examples in this design.
