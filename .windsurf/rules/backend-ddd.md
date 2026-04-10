---
trigger: glob
globs: src/backend/**
---

# Backend DDD & Clean Architecture Rule

Enforce Domain-Driven Design and Clean Architecture principles across the C# backend.

## Layer Architecture

The backend follows strict Clean Architecture with four layers. Dependencies point inward only.

```
Api → Application → Domain
         ↑
   Infrastructure
```

- **Domain**: Aggregates, Entities, Value Objects, Domain Events, Domain Exceptions, Domain Services. Zero external dependencies.
- **Application**: Commands, Queries, Handlers, DTOs, Validators, Common interfaces (IDispatcher, ICommand, IQuery, Result). Depends only on Domain.
- **Infrastructure**: EF Core DbContext, Repository implementations, Dispatcher, DI registration. Implements interfaces from Domain/Application.
- **Api**: Controllers, Middleware, Extensions. Thin transport layer.

## SharedKernel Pattern

Common base classes and interfaces are centralized in the SharedKernel project to avoid duplication across bounded contexts:

- **AggregateRoot**: Base class for all aggregates with domain event management (AddDomainEvent, ClearDomainEvents, DomainEvents)
- **ValueObject**: Abstract base class with equality based on GetEqualityComponents() - NEVER implement IEquatable manually
- **Entity**: Base class for all entities with ID
- **Result**: Record type for operation results (Success, Failure with error message)
- **IDispatcher**: CQRS dispatcher interface with SendAsync, QueryAsync, PublishAsync
- **ICommand<TResult>, IQuery<TResult>, IDomainEventHandler**: Marker interfaces for CQRS pattern
- **IHasDomainEvents**: Interface for aggregates that raise domain events

All bounded contexts (CustomerManagement, RiskManagement) reference SharedKernel and use these common building blocks.

## Multi-Bounded Context Architecture

The system is organized into multiple bounded contexts, each with its own database and ubiquitous language:

- **CustomerManagement**: Manages customer data (Customer aggregate). Has its own DbContext (CustomerDbContext) and database.
- **RiskManagement**: Manages credit risk applications, scoring, and processing (Application, ScoringConfig aggregates). Has its own DbContext (ApplicationDbContext) and database.

Cross-context communication happens through:
- **MassTransit messaging** (RabbitMQ) for integration events
- **Integration Event Publishers** in one context publish events to the message bus
- **Consumers** in other contexts subscribe to integration events and react
- **Internal API calls** with X-Api-Key header for synchronous service-to-service communication

Each bounded context follows the same Clean Architecture pattern (Api → Application → Domain ← Infrastructure) and references SharedKernel for common building blocks.

## Domain Events vs Integration Events

Two types of events exist with different purposes and lifecycles:

### Domain Events (Intra-Context)
- Raised within aggregates via `AddDomainEvent()` in the Domain layer
- Automatically dispatched via `DomainEventDispatchInterceptor` before `SaveChangesAsync()`
- Handled by `IDomainEventHandler<TEvent>` implementations within the same bounded context
- Used for side-effects within the same context (audit logging, notifications, process triggers)
- Stored in-memory on the aggregate until published, then cleared

### Integration Events (Inter-Context)
- Published via MassTransit to RabbitMQ message bus
- Implemented as `IntegrationEventPublisher` classes in Infrastructure
- Consumed by MassTransit consumers in other bounded contexts
- Used for cross-context communication (e.g., CustomerCreated → RiskManagement creates application read model)
- Persisted in MassTransit Outbox table for reliable messaging
- Support saga orchestration for long-running processes across contexts

**Rule**: Never mix domain events and integration events. Domain events stay within the context; integration events cross context boundaries.

## Saga Pattern

Long-running processes that span multiple bounded contexts use the Saga pattern with MassTransit:

- **State Machine**: `ApplicationCreationStateMachine` orchestrates the credit check process across CustomerManagement and RiskManagement
- **Saga State**: Persisted state machine instance tracks progress (Created, CustomerFetched, CreditCheckPerformed, Finalized, Failed)
- **Consumers**: Each saga step has a consumer that processes messages and advances the state
- **Fault Handling**: Fault consumers handle errors and transition to failed state
- **Correlation**: Messages are correlated by saga instance ID

Sagas are defined in Infrastructure/Sagas and registered via MassTransit configuration. They provide eventual consistency across bounded contexts without distributed transactions.

## Domain Layer Rules

### Aggregates

- Aggregates MUST extend `AggregateRoot` and encapsulate all business rules.
- State transitions MUST happen through domain methods on the aggregate (e.g., `application.Approve()`, `application.Submit()`).
- Domain events MUST be raised inside aggregate methods via `AddDomainEvent()`.
- Business rule violations MUST throw `DomainException`.
- Repository interfaces (e.g., `IApplicationRepository`) are defined in the Domain layer.
- Repository interfaces MUST only contain aggregate-level operations (CRUD, query by ID/owner). Report methods (stats, exports, dashboards) belong in dedicated read-model services or separate query repositories.

### Guard Clauses (Self-Validating Aggregates)

- Aggregate factory methods (e.g., `Create(...)`) and mutation methods MUST validate their own parameters with guard clauses.
- Invalid arguments MUST throw `DomainException` or `ArgumentException` — regardless of whether FluentValidation ran in the Application layer.
- The Domain Model is the last line of defense: it MUST never accept an invalid state.
- FluentValidation in the Application layer provides user-friendly error messages; guard clauses in the Domain protect invariants.

### Value Objects

- Value Objects MUST extend the `ValueObject` base class and override `GetEqualityComponents()`.
- NEVER implement `IEquatable<T>` manually on Value Objects — the `ValueObject` base class handles equality.
- Value Objects MUST be immutable (readonly properties, no public setters).
- Enumeration-style Value Objects (e.g., `ApplicationStatus`, `EmploymentStatus`) MUST also extend `ValueObject` or use a dedicated `Enumeration` base class — not standalone classes with manual equality.

### Avoid Primitive Obsession

- Domain concepts MUST be modeled as Value Objects, not as primitive types.
- Use `Money` (or similar) instead of `double` for monetary amounts (income, costs, rates).
- Use `EmailAddress` instead of `string` for email fields.
- Use `DateTime` or a dedicated Value Object instead of `string` for timestamps.
- Use dedicated Value Object types instead of magic strings (e.g., `InquiryStatus` instead of `"open"` / `"answered"`).

### Strongly Typed IDs

- Each Aggregate SHOULD have a strongly typed ID (e.g., `record ApplicationId(int Value)`) to prevent mixing IDs of different aggregates at compile time.
- Child entities SHOULD also use strongly typed IDs (e.g., `InquiryId`).
- EF Core value conversions MUST be configured for strongly typed IDs in `IEntityTypeConfiguration`.

### Domain Events & Handlers

- Domain events MUST be raised inside aggregate methods via `AddDomainEvent()`.
- Every Domain Event SHOULD have at least one `IDomainEventHandler<TEvent>` that reacts to it (e.g., audit logging, notifications, process triggers).
- Do NOT introduce a Domain Event without a concrete handler — events without handlers are dead code.
- Event handlers are auto-registered via assembly scanning (same as command/query handlers).

### Domain Services

- Domain Services encapsulate domain logic that does not naturally belong to a single aggregate.
- Domain Service interfaces are defined in the Domain layer; implementations may live in Domain or Infrastructure.

### Specification Pattern

- Reusable query predicates SHOULD be modeled as Specifications implementing `ISpecification<T>` with a `ToExpression()` method.
- Repositories SHOULD accept specifications instead of ad-hoc filter parameters to avoid duplicated `Where()` logic.
- Specifications are composable (AND, OR, NOT).

### Domain Policies

- A Domain Policy is a transactional reaction to a domain event that runs BEFORE `SaveChangesAsync()` — within the same transaction.
- Policies implement `IDomainPolicy<TEvent>` and are distinct from Domain Event Handlers (which run AFTER save).
- Use policies for aggregate-spanning business rules where "all or nothing" consistency is required.
- Use Domain Event Handlers (after save) for side-effects like notifications, logging, or external calls.

## Application Layer Rules (CQRS)

- Every write operation MUST be a dedicated Command implementing `ICommand<TResult>`.
- Every read operation MUST be a Query implementing `IQuery<TResult>`.
- Each Command/Query has exactly one Handler implementing `ICommandHandler<TCommand, TResult>` or `IQueryHandler<TQuery, TResult>`.
- Handlers MUST return `Result<T>` — never throw exceptions for expected business errors.
- Command results MUST contain only domain data (no redirect paths, no UI hints).
- Input validation MUST use FluentValidation (`IValidator<TDto>`).
- Handlers follow the pattern: validate → load aggregate → call domain method → (policies) → save → publish events.

### Pipeline Behaviors

- Cross-cutting concerns that repeat in every handler (validation, logging, transactions) SHOULD be extracted into `IPipelineBehavior<TRequest, TResult>` decorators.
- The Dispatcher executes behaviors in order before invoking the actual handler.
- Typical behaviors: `ValidationBehavior`, `LoggingBehavior`, `TransactionBehavior`.

### Read Models (CQRS Read-Side)

- Pure read operations (stats, dashboards, lists, exports) SHOULD NOT load full aggregates.
- Use dedicated read-model queries or lightweight DTOs projected directly from the database.
- Read-model repositories or query services are separate from write-model repositories.

## Dispatcher Pattern

- Controllers MUST inject only `IDispatcher` — never individual handlers.
- Use `dispatcher.SendAsync()` for commands, `dispatcher.QueryAsync()` for queries.
- Domain events are automatically dispatched via `DomainEventDispatchInterceptor` before `SaveChangesAsync()`.
- Pattern: business logic → (policies) → save (interceptor publishes events automatically).
- Domain events are NOT manually dispatched by handlers.

## Handler Auto-Registration

- All handlers are auto-registered via assembly scanning in `DependencyInjection.cs`.
- Adding a new handler requires NO manual registration — just implement the interface.
- Scanned interfaces: `ICommandHandler<,>`, `IQueryHandler<,>`, `IDomainEventHandler<>`, `IDomainPolicy<>`.

## Controller Rules

- Controllers are a thin transport layer: deserialize → dispatch → map result to HTTP response.
- Use `result.ToActionResult()` extension for consistent Result-to-HTTP mapping.
- Intent is expressed via HTTP verb + route + query parameters, NOT via DTO properties.
- Navigation/redirect logic belongs in the frontend, not in API responses.

## Strategic Design

### Bounded Contexts

- As the system grows, separate Bounded Contexts SHOULD be introduced (e.g., "Application Management" vs. "Processor Workspace" vs. "Reporting").
- Each Bounded Context has its own aggregates, repositories, and ubiquitous language.
- Cross-context communication happens through Domain Events or explicit integration interfaces — never by sharing aggregates directly.

### Anti-Corruption Layer (ACL)

- External system integrations (e.g., external scoring providers, credit agencies) MUST be wrapped in an Anti-Corruption Layer.
- The ACL adapter lives in Infrastructure and translates external models into Domain Value Objects.
- The Domain layer MUST NOT depend on external system models or DTOs.
