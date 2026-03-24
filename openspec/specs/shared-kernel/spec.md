## ADDED Requirements

### Requirement: SharedKernel project with DDD building blocks
The system SHALL provide a `SharedKernel` class library project containing common DDD building blocks used by both Customer Service and Application Service.

#### Scenario: Both services reference SharedKernel
- **WHEN** `RiskManagement.Domain` or `CustomerManagement.Domain` needs DDD base types
- **THEN** they SHALL reference `SharedKernel.csproj` via project reference

### Requirement: SharedKernel contains common base types
The SharedKernel SHALL include the following types extracted from `RiskManagement.Domain/Common`: `AggregateRoot<TId>`, `Entity<TId>`, `Enumeration`, `IDomainEvent`, `IHasDomainEvents`, `ValueObject`.

#### Scenario: AggregateRoot available in SharedKernel
- **WHEN** a new aggregate is defined in any service
- **THEN** it SHALL inherit from `SharedKernel.Common.AggregateRoot<TId>`

#### Scenario: RiskManagement uses SharedKernel base types
- **WHEN** `RiskManagement.Domain` is compiled after the extraction
- **THEN** all aggregates and entities SHALL use base types from SharedKernel instead of local copies

### Requirement: SharedKernel contains CQRS dispatcher
The SharedKernel SHALL include the full dispatcher pattern: `ICommand<TResult>`, `IQuery<TResult>`, `IDispatcher`, `ICommandHandler<TCommand, TResult>`, `IQueryHandler<TQuery, TResult>`, `IDomainEventHandler<TEvent>`, and `Dispatcher` implementation.

#### Scenario: Dispatcher available for Customer Service
- **WHEN** the Customer Service registers its handlers
- **THEN** it SHALL use `IDispatcher` and handler interfaces from SharedKernel

#### Scenario: Auto-registration works with SharedKernel interfaces
- **WHEN** a service scans its assemblies for handlers
- **THEN** it SHALL find handlers implementing SharedKernel interfaces

### Requirement: SharedKernel contains Result pattern
The SharedKernel SHALL include the `Result<T>` type with factory methods (Success, Failure, NotFound, Forbidden, ValidationFailure) and `ValidationHelper`.

#### Scenario: Result pattern used in Customer Service handlers
- **WHEN** a Customer Service command handler returns a result
- **THEN** it SHALL use `SharedKernel.Results.Result<T>`

### Requirement: SharedKernel contains shared value objects
The SharedKernel SHALL include `EmailAddress` value object (extracted from `RiskManagement.Domain.ValueObjects`).

#### Scenario: EmailAddress reused in Customer aggregate
- **WHEN** the Customer aggregate stores CreatedBy or Email fields
- **THEN** it SHALL use `SharedKernel.ValueObjects.EmailAddress`

### Requirement: SharedKernel contains domain exceptions
The SharedKernel SHALL include `DomainException` (extracted from `RiskManagement.Domain.Exceptions`).

#### Scenario: DomainException used in Customer domain
- **WHEN** a Customer aggregate guard clause fails
- **THEN** it SHALL throw `SharedKernel.Exceptions.DomainException`

### Requirement: RiskManagement refactored to use SharedKernel
After extraction, `RiskManagement.Domain`, `RiskManagement.Application`, and `RiskManagement.Infrastructure` SHALL reference SharedKernel and remove their local copies of the extracted types.

#### Scenario: No duplicate types after extraction
- **WHEN** both solutions are compiled
- **THEN** there SHALL be exactly one definition of each shared type (in SharedKernel), with no local copies remaining in RiskManagement
