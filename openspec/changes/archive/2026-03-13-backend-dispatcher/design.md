## Context

Controllers currently inject every `ICommandHandler<TCmd, TResult>` and `IQueryHandler<TQuery, TResult>` individually. `ApplicationsController` has 8 handler dependencies. Each new handler requires: handler class + DI registration line + controller constructor parameter. Domain events are raised via `AddDomainEvent()` in aggregates but never dispatched — `AggregateRoot` collects them, nobody consumes them.

Existing interfaces:
- `ICommandHandler<TCommand, TResult>` → `Task<Result<TResult>> HandleAsync(TCommand, CancellationToken)`
- `IQueryHandler<TQuery, TResult>` → `Task<Result<TResult>> HandleAsync(TQuery, CancellationToken)`
- `IDomainEvent` → `DateTime OccurredOn`

4 domain events exist: `ApplicationSubmittedEvent`, `ApplicationDecidedEvent`, `ApplicationDeletedEvent`, `InquiryCreatedEvent`. None have consumers yet.

## Goals / Non-Goals

**Goals:**
- Single `IDispatcher` dependency in controllers instead of N individual handlers
- Auto-registration of all handler types via assembly scanning (no manual DI lines)
- Domain event dispatching infrastructure (`PublishAsync` + `IDomainEventHandler<T>`)
- Preserve existing `ICommandHandler` and `IQueryHandler` interfaces
- Zero external dependencies (no MediatR, no Scrutor)

**Non-Goals:**
- Pipeline behaviors / middleware (documented in `backlog/domain-policy.md` for future)
- Domain policies (transactional pre-save logic — also in backlog)
- Creating concrete domain event handlers (infrastructure only, consumers come later)
- Changing HTTP API contracts

## Decisions

### 1. Marker interfaces for type-safe dispatch

Commands implement `ICommand<TResult>`, queries implement `IQuery<TResult>`. The dispatcher uses these to resolve the correct handler type from DI.

```csharp
public interface ICommand<TResult> { }
public interface IQuery<TResult> { }
```

**Rationale:** Without marker interfaces, the dispatcher cannot infer `TResult` from the request type alone. The alternative (passing `TResult` as a generic parameter to `SendAsync`) forces callers to specify the return type at every call site, which is verbose and error-prone.

**Alternative considered:** Convention-based dispatch (resolve by naming convention). Rejected: fragile, no compile-time safety.

### 2. Lightweight custom dispatcher over MediatR

A single `Dispatcher` class resolves handlers from `IServiceProvider` at runtime.

```csharp
public interface IDispatcher
{
    Task<Result<TResult>> SendAsync<TResult>(ICommand<TResult> command, CancellationToken ct = default);
    Task<Result<TResult>> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken ct = default);
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken ct = default);
}
```

**Rationale:** We already have well-defined handler interfaces. MediatR would replace them and add an external dependency for no additional value at our scale.

**Alternative considered:** MediatR. Rejected: external dependency, replaces existing interfaces, overkill for current needs.

### 3. Reflection-based assembly scanning for auto-registration

At startup, scan `RiskManagement.Application` assembly for all types implementing `ICommandHandler<,>`, `IQueryHandler<,>`, and `IDomainEventHandler<>`. Register each as scoped.

**Rationale:** Eliminates the manual registration block in `DependencyInjection.cs` (currently 15+ explicit lines). No external library needed — `Assembly.GetTypes()` + `GetInterfaces()` is sufficient.

**Alternative considered:** Scrutor library. Rejected: external dependency for ~20 lines of code.

### 4. After-save event dispatching in handlers (explicit)

Command handlers dispatch domain events explicitly after `SaveChangesAsync()`. No magic in `DbContext.SaveChanges()`.

```
Handler flow:
1. Business logic (aggregate methods)
2. repository.SaveChangesAsync()
3. foreach event in aggregate.DomainEvents → dispatcher.PublishAsync(event)
4. aggregate.ClearDomainEvents()
```

**Rationale:** Events are past-tense facts (`ApplicationSubmittedEvent`). Dispatching before save risks notifying about uncommitted state. Explicit dispatching in the handler is transparent — no hidden behavior in infrastructure.

**Alternative considered:** DbContext SaveChanges override with auto-dispatch. Rejected: implicit behavior, harder to reason about, couples infrastructure to domain events.

### 5. Fan-out event handlers (0..N per event)

`PublishAsync` resolves all `IDomainEventHandler<TEvent>` from DI and invokes them sequentially. Zero handlers is valid (current state — events are raised but no consumers exist yet).

```csharp
public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken ct = default);
}
```

**Rationale:** Unlike commands (exactly 1 handler), events are notifications that may have 0, 1, or N consumers. This matches the publish-subscribe pattern.

### 6. Files and location

| Component | Location | Layer |
|-----------|----------|-------|
| `ICommand<T>`, `IQuery<T>` | `Application/Common/` | Application |
| `IDispatcher` | `Application/Common/` | Application |
| `IDomainEventHandler<T>` | `Application/Common/` | Application |
| `Dispatcher` (impl) | `Infrastructure/Dispatching/` | Infrastructure |
| Auto-registration | `Infrastructure/DependencyInjection.cs` | Infrastructure |

## Risks / Trade-offs

- **Runtime resolution errors** → If a handler is not registered, `SendAsync` fails at runtime instead of DI composition time. Mitigation: startup validation that scans all `ICommand<T>`/`IQuery<T>` types and verifies a handler exists.
- **Reflection cost at startup** → Assembly scanning uses reflection. Mitigation: runs once at startup, negligible compared to DB migration.
- **Event handler ordering** → `PublishAsync` invokes handlers sequentially in registration order. If ordering matters, this becomes fragile. Mitigation: acceptable for now; if ordering becomes critical, introduce explicit priority or saga patterns.
- **No retry on event handler failure** → If a domain event handler throws, the exception propagates to the command handler. Mitigation: event handlers should be fire-and-forget; wrap in try/catch with logging. Revisit with outbox pattern if durability is needed.
