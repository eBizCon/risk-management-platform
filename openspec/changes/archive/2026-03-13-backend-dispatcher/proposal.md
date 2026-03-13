## Why

Controllers inject every command/query handler individually, leading to bloated constructors (up to 8 dependencies in `ApplicationsController`). Adding a new handler requires changes in three places: the handler class, the DI registration in `DependencyInjection.cs`, and the controller constructor. Domain events are raised in aggregates but never dispatched — no consumer exists.

## What Changes

- Introduce marker interfaces `ICommand<TResult>` and `IQuery<TResult>` on all existing commands and queries
- Create `IDispatcher` with `SendAsync`, `QueryAsync`, and `PublishAsync` methods that resolve handlers from DI at runtime
- Create `IDomainEventHandler<TEvent>` interface for domain event consumers
- Replace manual handler registration with assembly-scanning auto-registration for all handler types (command, query, event)
- Refactor all controllers to inject a single `IDispatcher` instead of N individual handlers
- Add explicit after-save domain event dispatching in command handlers

## Capabilities

### New Capabilities
- `cqrs-dispatcher`: Lightweight dispatcher that resolves ICommandHandler, IQueryHandler, and IDomainEventHandler from DI. Includes marker interfaces, auto-registration, and PublishAsync for domain events.

### Modified Capabilities
- `cqrs-command-split`: Controllers change from injecting individual handlers to using IDispatcher. Handler interfaces remain but commands/queries gain marker interfaces.

## Impact

- **RiskManagement.Application/Common/**: New marker interfaces, IDispatcher interface, IDomainEventHandler interface
- **RiskManagement.Infrastructure/**: Dispatcher implementation, auto-registration replaces manual DI lines
- **RiskManagement.Api/Controllers/**: All 4 controllers refactored (ApplicationsController, ProcessorController, InquiryController, AuthController)
- **RiskManagement.Application/Commands/**: All command records gain `ICommand<TResult>`
- **RiskManagement.Application/Queries/**: All query records gain `IQuery<TResult>`
- **No external dependencies added** — no MediatR, no Scrutor
- **No breaking API changes** — HTTP contracts remain identical
