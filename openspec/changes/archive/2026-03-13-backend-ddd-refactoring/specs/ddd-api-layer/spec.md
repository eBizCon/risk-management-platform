## ADDED Requirements

### Requirement: Thin ApplicationsController
`ApplicationsController` SHALL delegate all business logic to command/query handlers. Each action SHALL: extract user identity from `ClaimsPrincipal`, construct the appropriate command/query, call the handler, and map the `Result<T>` to an HTTP response. The controller SHALL contain NO business logic, validation orchestration, or direct repository access.

#### Scenario: Create application endpoint
- **WHEN** POST `/api/applications` is received with a valid body
- **THEN** the controller SHALL dispatch `CreateApplicationCommand` and return the handler's result as `200 OK` with `{ application, redirect }` or `400 Bad Request` with validation errors

#### Scenario: Get applications endpoint
- **WHEN** GET `/api/applications?status=X` is received
- **THEN** the controller SHALL dispatch `GetApplicationsByUserQuery` with the user's email and optional status filter

#### Scenario: Get single application endpoint
- **WHEN** GET `/api/applications/{id}` is received
- **THEN** the controller SHALL dispatch `GetApplicationQuery` with the id, user email, and user role

#### Scenario: Update application endpoint
- **WHEN** PUT `/api/applications/{id}` is received
- **THEN** the controller SHALL dispatch `UpdateApplicationCommand` and return the result

#### Scenario: Delete application endpoint
- **WHEN** DELETE `/api/applications/{id}` is received
- **THEN** the controller SHALL dispatch `DeleteApplicationCommand` and return `200 OK` with `{ success: true }` or an error

#### Scenario: Submit application endpoint
- **WHEN** POST `/api/applications/{id}/submit` is received
- **THEN** the controller SHALL dispatch `SubmitApplicationCommand` and return the updated application

### Requirement: Thin ProcessorController
`ProcessorController` SHALL delegate all business logic to command/query handlers. It SHALL contain NO direct repository access.

#### Scenario: Get processor applications endpoint
- **WHEN** GET `/api/processor/applications?status=X&page=N` is received
- **THEN** the controller SHALL dispatch `GetProcessorApplicationsQuery` and return `ProcessorApplicationsResponse`

#### Scenario: Get processor application detail endpoint
- **WHEN** GET `/api/processor/{id}` is received
- **THEN** the controller SHALL dispatch `GetApplicationQuery` with processor role

#### Scenario: Process decision endpoint
- **WHEN** POST `/api/processor/{id}/decide` is received
- **THEN** the controller SHALL dispatch `ProcessDecisionCommand` and return `{ application, redirect }`

### Requirement: Thin InquiryController
`InquiryController` SHALL delegate all inquiry operations to command/query handlers. No direct repository access, no inline business rule checks.

#### Scenario: Get inquiries endpoint
- **WHEN** GET `/api/applications/{id}/inquiries` is received
- **THEN** the controller SHALL dispatch a query and return the inquiries list

#### Scenario: Create inquiry endpoint
- **WHEN** POST `/api/applications/{id}/inquiry` is received by a processor
- **THEN** the controller SHALL dispatch `CreateInquiryCommand`

#### Scenario: Respond to inquiry endpoint
- **WHEN** POST `/api/applications/{id}/inquiry/response` is received by an applicant
- **THEN** the controller SHALL dispatch `AnswerInquiryCommand`

#### Scenario: Alternative inquiry endpoints preserved
- **WHEN** POST `/api/applications/{id}/answer-inquiry` or POST `/api/processor/{id}/inquire` is received
- **THEN** the controller SHALL delegate to the same handlers as the primary endpoints

### Requirement: ExceptionHandlingMiddleware
A global middleware SHALL catch `DomainException` and `InvalidStatusTransitionException` and map them to appropriate HTTP responses (400 Bad Request with error message). Unhandled exceptions SHALL result in 500 Internal Server Error.

#### Scenario: DomainException mapped to 400
- **WHEN** a handler throws `DomainException` with message "Nur Entwürfe können gelöscht werden"
- **THEN** the middleware SHALL return `400 Bad Request` with `{ error: "Nur Entwürfe können gelöscht werden" }`

#### Scenario: InvalidStatusTransitionException mapped to 400
- **WHEN** a handler throws `InvalidStatusTransitionException`
- **THEN** the middleware SHALL return `400 Bad Request` with a descriptive error message

#### Scenario: Unhandled exception mapped to 500
- **WHEN** an unexpected exception occurs
- **THEN** the middleware SHALL return `500 Internal Server Error` with a generic error message (no stack trace in production)

### Requirement: Result-to-HTTP mapping in controllers
Controllers SHALL map `Result<T>` to HTTP responses using a consistent pattern: `Result.IsSuccess` → `Ok(result.Value)`, `Result.IsFailure` → status code from `Result.Error.StatusCode` with error body.

#### Scenario: Success result mapped to 200
- **WHEN** a handler returns `Result.Success(data)`
- **THEN** the controller SHALL return `200 OK` with the data

#### Scenario: NotFound result mapped to 404
- **WHEN** a handler returns `Result.Failure` with StatusCode 404
- **THEN** the controller SHALL return `404 Not Found` with the error message

#### Scenario: Forbidden result mapped to 403
- **WHEN** a handler returns `Result.Failure` with StatusCode 403
- **THEN** the controller SHALL return `403 Forbidden` with the error message

#### Scenario: Validation failure mapped to 400
- **WHEN** a handler returns `Result.Failure` with StatusCode 400 and validation errors
- **THEN** the controller SHALL return `400 Bad Request` with `{ errors, values }` matching the current API format

### Requirement: Updated Program.cs with multi-project DI wiring
`Program.cs` SHALL register services from all layers: `AddInfrastructure()` for persistence, explicit registration for Application layer handlers, and Api-level services (auth, CORS, controllers).

#### Scenario: All handlers registered
- **WHEN** the application starts
- **THEN** all command and query handlers SHALL be resolvable via DI

#### Scenario: ScoringService registered
- **WHEN** the application starts
- **THEN** `ScoringService` (from Domain) SHALL be registered as Singleton

#### Scenario: Migration and seeding still works
- **WHEN** the application starts
- **THEN** database migration and seeding SHALL execute as before

### Requirement: Auth infrastructure unchanged
`AuthController`, `TestSessionController`, `AuthenticationExtensions`, `ClaimsPrincipalExtensions`, `RoleClaimHelper`, `OidcOptions`, and `JsonAuthorizationResultHandler` SHALL remain in the Api project. Their behavior SHALL not change.

#### Scenario: Auth endpoints preserved
- **WHEN** `/login`, `/logout`, `/api/auth/user`, `/api/dashboard/stats` are called
- **THEN** they SHALL behave identically to the current implementation

#### Scenario: Policy-based authorization preserved
- **WHEN** `[Authorize(Policy = "Applicant")]` is applied to a controller
- **THEN** it SHALL enforce the same role-based access as the current system
