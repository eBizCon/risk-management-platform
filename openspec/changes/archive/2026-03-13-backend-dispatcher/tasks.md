## 1. Marker Interfaces

- [x] 1.1 Create `ICommand<TResult>` marker interface in `RiskManagement.Application/Common/ICommand.cs`
- [x] 1.2 Create `IQuery<TResult>` marker interface in `RiskManagement.Application/Common/IQuery.cs`
- [x] 1.3 Add `ICommand<TResult>` to all existing command records: `CreateApplicationCommand`, `CreateAndSubmitApplicationCommand`, `UpdateApplicationCommand`, `UpdateAndSubmitApplicationCommand`, `DeleteApplicationCommand`, `SubmitApplicationCommand`, `ApproveApplicationCommand`, `RejectApplicationCommand`, `CreateInquiryCommand`, `AnswerInquiryCommand`
- [x] 1.4 Add `IQuery<TResult>` to all existing query records: `GetApplicationQuery`, `GetApplicationsByUserQuery`, `GetProcessorApplicationsQuery`, `GetDashboardStatsQuery`, `GetInquiriesQuery`

## 2. Dispatcher Interfaces

- [x] 2.1 Create `IDomainEventHandler<TEvent>` interface in `RiskManagement.Application/Common/IDomainEventHandler.cs`
- [x] 2.2 Create `IDispatcher` interface in `RiskManagement.Application/Common/IDispatcher.cs` with `SendAsync`, `QueryAsync`, `PublishAsync`

## 3. Dispatcher Implementation

- [x] 3.1 Create `Dispatcher` class in `RiskManagement.Infrastructure/Dispatching/Dispatcher.cs` implementing `IDispatcher`
- [x] 3.2 Implement `SendAsync` — resolve `ICommandHandler<TCommand, TResult>` from `IServiceProvider`, throw `InvalidOperationException` if not found
- [x] 3.3 Implement `QueryAsync` — resolve `IQueryHandler<TQuery, TResult>` from `IServiceProvider`, throw `InvalidOperationException` if not found
- [x] 3.4 Implement `PublishAsync` — resolve all `IDomainEventHandler<TEvent>` from `IServiceProvider`, invoke sequentially, tolerate zero handlers

## 4. Auto-Registration

- [x] 4.1 Add assembly-scanning method in `DependencyInjection.cs` that registers all `ICommandHandler<,>`, `IQueryHandler<,>`, and `IDomainEventHandler<>` implementations as scoped
- [x] 4.2 Register `IDispatcher` → `Dispatcher` as scoped
- [x] 4.3 Remove all manual handler registration lines from `AddApplicationServices()`
- [x] 4.4 Keep non-handler registrations (validators, `ScoringService`) unchanged

## 5. Controller Refactoring

- [x] 5.1 Refactor `ApplicationsController` — replace 8 handler fields with single `IDispatcher`, update all action methods to use `SendAsync`/`QueryAsync`
- [x] 5.2 Refactor `ProcessorController` — replace 4 handler fields with single `IDispatcher`
- [x] 5.3 Refactor `InquiryController` — replace 3 handler fields with single `IDispatcher`
- [x] 5.4 Refactor `AuthController` — replace `IQueryHandler` field with `IDispatcher`

## 6. Domain Event Dispatching in Handlers

- [x] 6.1 Inject `IDispatcher` into command handlers that operate on aggregates raising events: `SubmitApplicationHandler`, `CreateAndSubmitApplicationHandler`, `UpdateAndSubmitApplicationHandler`, `ApproveApplicationHandler`, `RejectApplicationHandler`, `DeleteApplicationHandler`, `CreateInquiryHandler`
- [x] 6.2 Add after-save event dispatching pattern: `foreach event → dispatcher.PublishAsync(event)` then `aggregate.ClearDomainEvents()`
- [x] 6.3 Handlers that don't raise events (`CreateApplicationHandler`, `UpdateApplicationHandler`, `AnswerInquiryHandler`) — no changes needed beyond marker interface on their commands

## 7. Tests & Verification

- [x] 7.1 Add unit tests for `Dispatcher`: verify `SendAsync` resolves correct handler, verify `QueryAsync` resolves correct handler, verify `PublishAsync` fans out to multiple handlers, verify `PublishAsync` tolerates zero handlers, verify `SendAsync` throws on missing handler
- [x] 7.2 Verify solution builds without errors (`dotnet build`)
- [x] 7.3 Verify all existing unit tests pass (`dotnet test`)
- [ ] 7.4 Verify E2E tests pass (`npm run test:e2e:ci`) — blocked: backend API not running (pre-existing infra issue, not related to dispatcher changes)
