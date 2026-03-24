# Capability: CQRS Dispatcher

## Purpose
Lightweight dispatcher pattern for routing commands, queries, and domain events to their respective handlers via DI, with assembly-scanning auto-registration.

## Requirements

### Requirement: Marker interface for commands
All command records SHALL implement `ICommand<TResult>` where `TResult` matches the handler's result type. The interface SHALL be empty (marker only) and defined in `RiskManagement.Application.Common`.

#### Scenario: Command implements marker interface
- **WHEN** `CreateApplicationCommand` is defined
- **THEN** it implements `ICommand<CreateApplicationResult>`

#### Scenario: Marker interface enables type inference
- **WHEN** `dispatcher.SendAsync(new CreateApplicationCommand(...))` is called
- **THEN** the compiler infers `TResult` as `CreateApplicationResult` without explicit type parameter

### Requirement: Marker interface for queries
All query records SHALL implement `IQuery<TResult>` where `TResult` matches the handler's result type. The interface SHALL be empty (marker only) and defined in `RiskManagement.Application.Common`.

#### Scenario: Query implements marker interface
- **WHEN** `GetApplicationQuery` is defined
- **THEN** it implements `IQuery<ApplicationResponse>`

### Requirement: IDispatcher with SendAsync for commands
`IDispatcher` SHALL provide a `SendAsync<TResult>(ICommand<TResult> command, CancellationToken ct)` method that resolves the matching `ICommandHandler<TCommand, TResult>` from DI and calls `HandleAsync`.

#### Scenario: Dispatching a command resolves the correct handler
- **WHEN** `dispatcher.SendAsync(new CreateApplicationCommand(...))` is called
- **THEN** the dispatcher resolves `ICommandHandler<CreateApplicationCommand, CreateApplicationResult>` from DI and invokes its `HandleAsync`

#### Scenario: No handler registered for command
- **WHEN** `SendAsync` is called with a command that has no registered handler
- **THEN** the dispatcher throws `InvalidOperationException` with a message identifying the missing handler type

### Requirement: IDispatcher with QueryAsync for queries
`IDispatcher` SHALL provide a `QueryAsync<TResult>(IQuery<TResult> query, CancellationToken ct)` method that resolves the matching `IQueryHandler<TQuery, TResult>` from DI and calls `HandleAsync`.

#### Scenario: Dispatching a query resolves the correct handler
- **WHEN** `dispatcher.QueryAsync(new GetApplicationQuery(...))` is called
- **THEN** the dispatcher resolves `IQueryHandler<GetApplicationQuery, ApplicationResponse>` from DI and invokes its `HandleAsync`

#### Scenario: No handler registered for query
- **WHEN** `QueryAsync` is called with a query that has no registered handler
- **THEN** the dispatcher throws `InvalidOperationException` with a message identifying the missing handler type

### Requirement: IDispatcher with PublishAsync for domain events
`IDispatcher` SHALL provide a `PublishAsync(IDomainEvent domainEvent, CancellationToken ct)` method that resolves all `IDomainEventHandler<TEvent>` from DI and invokes each sequentially.

#### Scenario: Publishing an event with one handler
- **WHEN** `dispatcher.PublishAsync(new ApplicationSubmittedEvent(1))` is called and one `IDomainEventHandler<ApplicationSubmittedEvent>` is registered
- **THEN** that handler's `HandleAsync` is invoked

#### Scenario: Publishing an event with no handlers
- **WHEN** `dispatcher.PublishAsync(new ApplicationDeletedEvent(1))` is called and no handlers are registered for that event type
- **THEN** the method completes without error

#### Scenario: Publishing an event with multiple handlers
- **WHEN** `dispatcher.PublishAsync(event)` is called and 3 handlers are registered
- **THEN** all 3 handlers are invoked sequentially in registration order

### Requirement: IDomainEventHandler interface
The system SHALL provide `IDomainEventHandler<in TEvent>` with a single `HandleAsync(TEvent, CancellationToken)` method returning `Task`. The interface SHALL be defined in `RiskManagement.Application.Common`.

#### Scenario: Event handler interface contract
- **WHEN** a class implements `IDomainEventHandler<ApplicationSubmittedEvent>`
- **THEN** it MUST implement `Task HandleAsync(ApplicationSubmittedEvent domainEvent, CancellationToken ct)`

### Requirement: Assembly-scanning auto-registration
All implementations of `ICommandHandler<,>`, `IQueryHandler<,>`, and `IDomainEventHandler<>` SHALL be auto-registered as scoped services by scanning the `RiskManagement.Application` assembly at startup. Manual handler registration lines SHALL be removed from `DependencyInjection.cs`.

#### Scenario: New handler is discovered without manual registration
- **WHEN** a new class `FooHandler : ICommandHandler<FooCommand, FooResult>` is added to the Application project
- **THEN** it is available via DI without any change to `DependencyInjection.cs`

#### Scenario: Existing handlers remain functional after migration
- **WHEN** manual registration lines are removed and replaced with auto-scanning
- **THEN** all existing command and query handlers resolve correctly from DI

### Requirement: Explicit after-save event dispatching
Command handlers SHALL dispatch domain events explicitly after `SaveChangesAsync()`. The pattern SHALL be: business logic → save → publish events → clear events. Domain events SHALL NOT be auto-dispatched by DbContext or repository.

#### Scenario: Events dispatched after successful save
- **WHEN** a command handler calls `application.Submit()` which raises `ApplicationSubmittedEvent`, then saves
- **THEN** the handler dispatches the event via `dispatcher.PublishDomainEventsAsync(application)` after `SaveChangesAsync()` succeeds

#### Scenario: Events not dispatched on save failure
- **WHEN** `SaveChangesAsync()` throws an exception
- **THEN** no domain events are published
