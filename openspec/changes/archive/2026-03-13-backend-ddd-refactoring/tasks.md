## 1. Solution Structure & Project Scaffolding

- [x] 1.1 Create `RiskManagement.Domain` class library project (net8.0, no NuGet dependencies)
- [x] 1.2 Create `RiskManagement.Application` class library project (net8.0, reference Domain)
- [x] 1.3 Create `RiskManagement.Infrastructure` class library project (net8.0, reference Domain, add EF Core + Npgsql packages)
- [x] 1.4 Update `RiskManagement.Api.csproj` to reference Application and Infrastructure projects, remove direct EF Core model/repository references
- [x] 1.5 Update `RiskManagement.Api.Tests.csproj` to reference all necessary projects
- [x] 1.6 Update `RiskManagement.sln` to include all new projects

## 2. Domain Layer — Base Classes & Common

- [x] 2.1 Create `Entity` base class with `Id` property in `Domain/Common/`
- [x] 2.2 Create `AggregateRoot` base class extending Entity with `DomainEvents` list and `AddDomainEvent()` / `ClearDomainEvents()` methods
- [x] 2.3 Create `ValueObject` base class with structural equality in `Domain/Common/`
- [x] 2.4 Create `IDomainEvent` marker interface in `Domain/Common/`

## 3. Domain Layer — Value Objects

- [x] 3.1 Create `ApplicationStatus` value object (Draft, Submitted, NeedsInformation, Resubmitted, Approved, Rejected) with `From(string)` factory and `Value` property mapping to existing snake_case strings
- [x] 3.2 Create `EmploymentStatus` value object (Employed, SelfEmployed, Unemployed, Retired) with string conversion (employed, self_employed, unemployed, retired)
- [x] 3.3 Create `TrafficLight` value object (Red, Yellow, Green) with string conversion
- [x] 3.4 Create `ScoringResult` value object with Score (int), TrafficLight, and Reasons (IReadOnlyList<string>)

## 4. Domain Layer — Exceptions

- [x] 4.1 Create `DomainException` base exception class
- [x] 4.2 Create `InvalidStatusTransitionException` extending DomainException with fromStatus/toStatus in message

## 5. Domain Layer — Domain Events

- [x] 5.1 Create `ApplicationSubmittedEvent` implementing IDomainEvent (contains ApplicationId)
- [x] 5.2 Create `ApplicationDecidedEvent` implementing IDomainEvent (contains ApplicationId, Decision)
- [x] 5.3 Create `InquiryCreatedEvent` implementing IDomainEvent (contains ApplicationId, InquiryId)

## 6. Domain Layer — ScoringService

- [x] 6.1 Move `ScoringService` to `Domain/Services/` — change it to accept `EmploymentStatus` value object instead of string, return `ScoringResult` value object. Keep algorithm identical.

## 7. Domain Layer — Aggregate Root & Child Entity

- [x] 7.1 Create `ApplicationInquiry` child entity in `Domain/Aggregates/Application/` with private setters and `Answer(responseText)` method
- [x] 7.2 Create `Application` aggregate root in `Domain/Aggregates/Application/` with private setters, private `_inquiries` list, and `IReadOnlyList<ApplicationInquiry> Inquiries` property
- [x] 7.3 Implement `Application` factory constructor (or static `Create()` method) that sets initial state to Draft, calculates scoring, sets CreatedAt
- [x] 7.4 Implement `Submit(ScoringService)` method — validates Draft status, recalculates scoring, sets Submitted status + SubmittedAt, adds ApplicationSubmittedEvent
- [x] 7.5 Implement `Approve(comment?)` method — validates Submitted/Resubmitted status, sets Approved + ProcessorComment + ProcessedAt, adds ApplicationDecidedEvent
- [x] 7.6 Implement `Reject(comment?)` method — validates Submitted/Resubmitted status, sets Rejected + ProcessorComment + ProcessedAt, adds ApplicationDecidedEvent
- [x] 7.7 Implement `UpdateDetails(name, income, fixedCosts, desiredRate, employmentStatus, hasPaymentDefault, ScoringService)` — validates Draft status, updates fields, recalculates scoring
- [x] 7.8 Implement `MarkAsDeleted()` — validates Draft status
- [x] 7.9 Implement `RequestInformation(inquiryText, processorEmail)` — validates Submitted/Resubmitted status, no open inquiry, creates child entity, sets NeedsInformation status, adds InquiryCreatedEvent
- [x] 7.10 Implement `AnswerInquiry(responseText)` — validates NeedsInformation status, finds open inquiry, calls Answer(), sets Resubmitted status

## 8. Domain Layer — Repository Interface

- [x] 8.1 Create `IApplicationRepository` interface in `Domain/Aggregates/Application/` with methods: GetByIdAsync, GetByUserAsync, GetAllPaginatedAsync, AddAsync, RemoveAsync, SaveChangesAsync, GetProcessorStatsAsync, GetDashboardStatsAsync, GetApplicationsForExportAsync, GetInquiriesAsync

## 9. Application Layer — Common

- [x] 9.1 Create `ICommandHandler<TCommand, TResult>` interface with `Task<Result<TResult>> HandleAsync(TCommand, CancellationToken)`
- [x] 9.2 Create `IQueryHandler<TQuery, TResult>` interface with `Task<Result<TResult>> HandleAsync(TQuery, CancellationToken)`
- [x] 9.3 Create `Result<T>` type with `IsSuccess`, `Value`, `Error` (including StatusCode) and static factory methods `Success(T)`, `Failure(string, int statusCode)`

## 10. Application Layer — DTOs

- [x] 10.1 Move/recreate response DTOs in `Application/DTOs/`: ApplicationResponse, ProcessorApplicationsResponse, ProcessorStats, PaginationInfo, DashboardStats, ValidationErrorResponse, UserDto
- [x] 10.2 Create mapping logic from Application aggregate to ApplicationResponse DTO (extension method or static mapper)

## 11. Application Layer — Validation

- [x] 11.1 Move `ApplicationValidator` (for create commands) to `Application/Validation/`
- [x] 11.2 Move `ApplicationUpdateValidator` to `Application/Validation/`
- [x] 11.3 Move `ProcessorDecisionValidator` to `Application/Validation/`
- [x] 11.4 Update validators to reference Application layer DTO/command types

## 12. Application Layer — Command Handlers

- [x] 12.1 Create `CreateApplicationCommand` and `CreateApplicationHandler` — validate input, create aggregate via factory, optionally submit, persist, return response
- [x] 12.2 Create `SubmitApplicationCommand` and `SubmitApplicationHandler` — load aggregate, verify ownership, call Submit(), persist
- [x] 12.3 Create `UpdateApplicationCommand` and `UpdateApplicationHandler` — load aggregate, verify ownership, validate input, call UpdateDetails(), optionally submit, persist
- [x] 12.4 Create `DeleteApplicationCommand` and `DeleteApplicationHandler` — load aggregate, verify ownership, call MarkAsDeleted(), remove, persist
- [x] 12.5 Create `ProcessDecisionCommand` and `ProcessDecisionHandler` — validate input, load aggregate, call Approve()/Reject(), persist
- [x] 12.6 Create `CreateInquiryCommand` and `CreateInquiryHandler` — validate input, load aggregate, call RequestInformation(), persist
- [x] 12.7 Create `AnswerInquiryCommand` and `AnswerInquiryHandler` — load aggregate, verify ownership, call AnswerInquiry(), persist

## 13. Application Layer — Query Handlers

- [x] 13.1 Create `GetApplicationQuery` and `GetApplicationHandler` — load application, check ownership for applicants, return mapped DTO
- [x] 13.2 Create `GetApplicationsByUserQuery` and handler — query by user email with optional status filter
- [x] 13.3 Create `GetProcessorApplicationsQuery` and handler — paginated query with stats
- [x] 13.4 Create `GetDashboardStatsQuery` and handler — stats scoped by role
- [x] 13.5 Create `GetInquiriesQuery` and handler — load inquiries for an application with ownership check

## 14. Infrastructure Layer — Persistence

- [x] 14.1 Move `ApplicationDbContext` to `Infrastructure/Persistence/`, update namespace, use `ApplyConfigurationsFromAssembly`
- [x] 14.2 Create `ApplicationConfiguration : IEntityTypeConfiguration<Application>` with value object conversions (ApplicationStatus, EmploymentStatus, TrafficLight) and same table/column mappings
- [x] 14.3 Create `ApplicationInquiryConfiguration : IEntityTypeConfiguration<ApplicationInquiry>` with same table/column mappings
- [x] 14.4 Implement `ApplicationRepository : IApplicationRepository` — all methods as thin EF Core queries, no business logic. Eager-load Inquiries on GetByIdAsync.
- [x] 14.5 Move `DatabaseSeeder` to `Infrastructure/Seeding/`, update namespace and references
- [x] 14.6 Move EF migrations to `Infrastructure/Persistence/Migrations/`

## 15. Infrastructure Layer — DI Registration

- [x] 15.1 Create `DependencyInjection.AddInfrastructure(IServiceCollection, IConfiguration)` extension method registering DbContext, Repository, Seeder

## 16. Api Layer — Middleware

- [x] 16.1 Create `ExceptionHandlingMiddleware` that catches `DomainException` → 400, `InvalidStatusTransitionException` → 400, unhandled → 500

## 17. Api Layer — Thin Controllers

- [x] 17.1 Rewrite `ApplicationsController` to dispatch commands/queries via injected handlers, map Result<T> to HTTP responses
- [x] 17.2 Rewrite `ProcessorController` to dispatch commands/queries via injected handlers
- [x] 17.3 Rewrite `InquiryController` to dispatch commands/queries via injected handlers (preserve alternative endpoints)
- [x] 17.4 Update `AuthController.GetDashboardStats` to use `GetDashboardStatsQuery` handler

## 18. Api Layer — Program.cs & DI Wiring

- [x] 18.1 Update `Program.cs`: call `AddInfrastructure()`, register all command/query handlers, register ScoringService (Singleton), register validators, register ExceptionHandlingMiddleware, keep auth/CORS config
- [x] 18.2 Remove old `Models/`, `Data/ApplicationRepository.cs`, `Services/ScoringService.cs`, `Validation/` files from Api project (now in Domain/Application/Infrastructure)
- [x] 18.3 Keep `Models/Enums.cs` constants (AppRoles, AuthPolicies) in Api project or move role/policy constants to a shared location accessible by Api

## 19. Tests

- [ ] 19.1 Create `RiskManagement.Domain.Tests` project referencing Domain
- [ ] 19.2 Write unit tests for Application aggregate: Submit(), Approve(), Reject(), UpdateDetails(), MarkAsDeleted(), RequestInformation(), AnswerInquiry() — both happy and failure paths
- [ ] 19.3 Write unit tests for value objects: ApplicationStatus, EmploymentStatus, TrafficLight round-trip conversion and invalid input
- [ ] 19.4 Write unit tests for ScoringService producing correct ScoringResult
- [x] 19.5 Update `RiskManagement.Api.Tests` to work with new project structure (update references, namespaces)

## 20. Verification

- [x] 20.1 Verify solution builds without errors (`dotnet build`)
- [x] 20.2 Verify all existing API tests pass (`dotnet test`)
- [ ] 20.3 Verify E2E tests pass (`npm run test:e2e:ci` from frontend)
- [ ] 20.4 Verify database migration still works (no schema changes)
