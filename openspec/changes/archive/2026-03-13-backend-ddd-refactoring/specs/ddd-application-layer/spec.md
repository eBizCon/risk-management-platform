## ADDED Requirements

### Requirement: ICommandHandler and IQueryHandler interfaces
The Application layer SHALL define generic interfaces `ICommandHandler<TCommand, TResult>` and `IQueryHandler<TQuery, TResult>` with a single `HandleAsync` method. All use case handlers SHALL implement one of these interfaces.

#### Scenario: Command handler interface contract
- **WHEN** a command handler is resolved via DI
- **THEN** it SHALL implement `ICommandHandler<TCommand, TResult>` with `Task<Result<TResult>> HandleAsync(TCommand command, CancellationToken ct)`

#### Scenario: Query handler interface contract
- **WHEN** a query handler is resolved via DI
- **THEN** it SHALL implement `IQueryHandler<TQuery, TResult>` with `Task<Result<TResult>> HandleAsync(TQuery query, CancellationToken ct)`

### Requirement: Result wrapper for handler responses
The Application layer SHALL define a `Result<T>` type that represents either a success with a value or a failure with an error. Handlers SHALL return `Result<T>` instead of throwing exceptions for expected failures (not found, forbidden, validation).

#### Scenario: Successful result
- **WHEN** a handler completes successfully
- **THEN** it SHALL return `Result<T>.Success(value)` where `IsSuccess == true` and `Value` contains the response

#### Scenario: Failure result
- **WHEN** a handler encounters an expected failure (e.g., entity not found)
- **THEN** it SHALL return `Result<T>.Failure(error)` where `IsSuccess == false` and `Error` contains the error details

#### Scenario: Failure result with HTTP status hint
- **WHEN** a handler returns a failure
- **THEN** the error SHALL include a `StatusCode` (e.g., 404, 403, 400) so the controller can map it to the correct HTTP status

### Requirement: CreateApplicationCommand handler
The handler SHALL validate input via FluentValidation, create an `Application` aggregate with scoring, optionally submit it if `Action == "submit"`, persist via repository, and return the application data.

#### Scenario: Create as draft
- **WHEN** CreateApplicationCommand is handled with valid data and Action = null
- **THEN** a new Application SHALL be persisted with Status = Draft and the response SHALL include the application and a redirect path

#### Scenario: Create and submit
- **WHEN** CreateApplicationCommand is handled with valid data and Action = "submit"
- **THEN** the Application SHALL be created and then submitted, the response SHALL include the submitted application and redirect with `?submitted=true`

#### Scenario: Validation failure
- **WHEN** CreateApplicationCommand has invalid input (e.g., empty name, negative income)
- **THEN** the handler SHALL return a failure Result with validation errors in the same format as the current API

### Requirement: SubmitApplicationCommand handler
The handler SHALL load the Application aggregate, verify ownership, call `Submit()`, persist, and return the updated application.

#### Scenario: Successful submission
- **WHEN** SubmitApplicationCommand is handled for a draft application owned by the requesting user
- **THEN** the application SHALL be submitted and the updated application returned

#### Scenario: Not owner
- **WHEN** SubmitApplicationCommand is handled but the application's CreatedBy does not match the user email
- **THEN** the handler SHALL return a failure Result with 403 status

### Requirement: UpdateApplicationCommand handler
The handler SHALL load the Application aggregate, verify ownership, validate input, call `UpdateDetails()`, optionally submit, persist, and return the updated application.

#### Scenario: Successful update
- **WHEN** UpdateApplicationCommand is handled for a draft application owned by the requesting user
- **THEN** the application details SHALL be updated and ScoringResult recalculated

#### Scenario: Update and submit
- **WHEN** UpdateApplicationCommand has Action = "submit"
- **THEN** the application SHALL be updated and then submitted in a single operation

### Requirement: DeleteApplicationCommand handler
The handler SHALL load the Application aggregate, verify ownership, call `MarkAsDeleted()`, remove via repository, and confirm success.

#### Scenario: Successful deletion
- **WHEN** DeleteApplicationCommand is handled for a draft application owned by the requesting user
- **THEN** the application SHALL be removed from persistence

#### Scenario: Cannot delete non-draft
- **WHEN** DeleteApplicationCommand is handled for a non-draft application
- **THEN** the handler SHALL return a failure Result with appropriate error message

### Requirement: ProcessDecisionCommand handler
The handler SHALL validate the decision input, load the Application aggregate, call `Approve()` or `Reject()` based on the decision, persist, and return the updated application.

#### Scenario: Approve application
- **WHEN** ProcessDecisionCommand is handled with decision = "approved" for a submitted application
- **THEN** the application SHALL be approved with optional comment and redirect path returned

#### Scenario: Reject application
- **WHEN** ProcessDecisionCommand is handled with decision = "rejected" for a submitted application
- **THEN** the application SHALL be rejected with optional comment and redirect path returned

### Requirement: CreateInquiryCommand handler
The handler SHALL load the Application aggregate, call `RequestInformation()`, persist, and return the created inquiry.

#### Scenario: Successful inquiry creation
- **WHEN** CreateInquiryCommand is handled for a submitted application with no open inquiry
- **THEN** an inquiry SHALL be created and the application status SHALL change to NeedsInformation

#### Scenario: Empty inquiry text
- **WHEN** CreateInquiryCommand is handled with empty inquiry text
- **THEN** the handler SHALL return a failure Result with validation error

### Requirement: AnswerInquiryCommand handler
The handler SHALL load the Application aggregate, verify ownership, call `AnswerInquiry()`, persist, and return the updated inquiry.

#### Scenario: Successful answer
- **WHEN** AnswerInquiryCommand is handled for an application with an open inquiry, by the application owner
- **THEN** the inquiry SHALL be answered and the application status SHALL change to Resubmitted

### Requirement: GetApplicationQuery handler
The handler SHALL load a single application by ID. For applicants, it SHALL verify ownership.

#### Scenario: Applicant retrieves own application
- **WHEN** GetApplicationQuery is handled by an applicant for their own application
- **THEN** the application data SHALL be returned

#### Scenario: Applicant retrieves someone else's application
- **WHEN** GetApplicationQuery is handled by an applicant for another user's application
- **THEN** the handler SHALL return a failure Result with 403 status

#### Scenario: Processor retrieves any application
- **WHEN** GetApplicationQuery is handled by a processor
- **THEN** the application data SHALL be returned regardless of owner

### Requirement: GetApplicationsByUserQuery handler
The handler SHALL retrieve all applications for a given user email, optionally filtered by status.

#### Scenario: List user's applications
- **WHEN** GetApplicationsByUserQuery is handled with a user email
- **THEN** all applications created by that user SHALL be returned

#### Scenario: Filter by status
- **WHEN** GetApplicationsByUserQuery includes a status filter
- **THEN** only applications matching that status SHALL be returned

### Requirement: GetProcessorApplicationsQuery handler
The handler SHALL retrieve paginated applications for the processor view with statistics.

#### Scenario: Paginated list with stats
- **WHEN** GetProcessorApplicationsQuery is handled with optional status filter and page number
- **THEN** the response SHALL include paginated applications, pagination info, status filter, and processor stats (total, submitted, approved, rejected)

### Requirement: GetDashboardStatsQuery handler
The handler SHALL return application counts by status category, scoped to a user (for applicants) or global (for processors).

#### Scenario: Applicant dashboard
- **WHEN** GetDashboardStatsQuery is handled for an applicant
- **THEN** stats SHALL be scoped to that user's applications only

#### Scenario: Processor dashboard
- **WHEN** GetDashboardStatsQuery is handled for a processor
- **THEN** stats SHALL include all applications

### Requirement: Application layer DTOs
The Application layer SHALL define response DTOs that map from domain objects. Controllers SHALL return these DTOs, never domain entities directly.

#### Scenario: Application response mapping
- **WHEN** an Application aggregate is mapped to a response DTO
- **THEN** it SHALL include all fields currently returned by the API (id, name, income, fixedCosts, desiredRate, employmentStatus, hasPaymentDefault, status, score, trafficLight, scoringReasons, processorComment, createdAt, submittedAt, processedAt, createdBy)

### Requirement: FluentValidation in Application layer
`ApplicationValidator`, `ApplicationUpdateValidator`, and `ProcessorDecisionValidator` SHALL live in the Application layer. They SHALL validate command DTOs before the handler processes them. Validation rules SHALL remain identical to the current implementation.

#### Scenario: Validation rules unchanged
- **WHEN** an invalid ApplicationCreateDto is validated
- **THEN** the same validation errors SHALL be returned as in the current system (same field names, same messages)
