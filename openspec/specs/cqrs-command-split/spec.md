# Capability: CQRS Command Split

## Purpose
Dedicated single-intent command/query handlers following CQRS principles, replacing multi-purpose endpoints with focused commands per business operation.

## Requirements

### Requirement: Single-intent approval command
The system SHALL provide a dedicated `ApproveApplicationCommand` implementing `ICommand<ApproveApplicationResult>` with its own handler that calls `Application.Approve()` on the aggregate. The handler SHALL accept an optional comment. The handler SHALL dispatch domain events via `IDispatcher.PublishDomainEventsAsync()` after save.

#### Scenario: Processor approves an application without comment
- **WHEN** processor sends `POST /api/processor/{id}/approve` with empty body
- **THEN** the application status changes to `approved` and `ProcessedAt` is set

#### Scenario: Processor approves an application with comment
- **WHEN** processor sends `POST /api/processor/{id}/approve` with `{ "comment": "Alles in Ordnung" }`
- **THEN** the application status changes to `approved`, `ProcessorComment` is set, and `ProcessedAt` is set

#### Scenario: Processor tries to approve a draft application
- **WHEN** processor sends `POST /api/processor/{id}/approve` for an application in `draft` status
- **THEN** the system returns 400 with an invalid status transition error

### Requirement: Single-intent rejection command
The system SHALL provide a dedicated `RejectApplicationCommand` implementing `ICommand<RejectApplicationResult>` with its own handler that calls `Application.Reject()` on the aggregate. The handler SHALL require a comment. The handler SHALL dispatch domain events via `IDispatcher.PublishDomainEventsAsync()` after save.

#### Scenario: Processor rejects an application with comment
- **WHEN** processor sends `POST /api/processor/{id}/reject` with `{ "comment": "Einkommen zu niedrig" }`
- **THEN** the application status changes to `rejected`, `ProcessorComment` is set, and `ProcessedAt` is set

#### Scenario: Processor tries to reject without comment
- **WHEN** processor sends `POST /api/processor/{id}/reject` with empty or missing comment
- **THEN** the system returns 400 with validation error "Bei Ablehnung ist eine Begründung erforderlich"

### Requirement: Single-intent create-and-submit command
The system SHALL provide a dedicated `CreateAndSubmitApplicationCommand` implementing `ICommand<CreateAndSubmitApplicationResult>` with its own handler that creates an application and immediately submits it in a single transaction. The handler SHALL be triggered by a `submit=true` query parameter. The handler SHALL dispatch domain events via `IDispatcher.PublishDomainEventsAsync()` after save.

#### Scenario: Applicant creates and submits in one step
- **WHEN** applicant sends `POST /api/applications?submit=true` with valid application data
- **THEN** the application is created, scored, submitted, and the response includes a redirect to the application detail page with `?submitted=true`

#### Scenario: Applicant creates a draft without submit parameter
- **WHEN** applicant sends `POST /api/applications` with valid application data (no `submit` query parameter)
- **THEN** the application is created as a draft and the response includes a redirect to the application detail page

### Requirement: Single-intent update-and-submit command
The system SHALL provide a dedicated `UpdateAndSubmitApplicationCommand` implementing `ICommand<UpdateAndSubmitApplicationResult>` with its own handler that updates an application and immediately submits it in a single transaction. The handler SHALL be triggered by a `submit=true` query parameter. The handler SHALL dispatch domain events via `IDispatcher.PublishDomainEventsAsync()` after save.

#### Scenario: Applicant updates and submits in one step
- **WHEN** applicant sends `PUT /api/applications/{id}?submit=true` with valid application data
- **THEN** the application is updated, scored, submitted, and the response includes a redirect with `?submitted=true`

#### Scenario: Applicant updates a draft without submit parameter
- **WHEN** applicant sends `PUT /api/applications/{id}` with valid application data (no `submit` query parameter)
- **THEN** the application is updated as a draft

### Requirement: Delete domain operation with event
The `Application` aggregate SHALL expose a `Delete()` method that guards against deletion of non-draft applications and raises an `ApplicationDeletedEvent`. `DeleteApplicationCommand` SHALL implement `ICommand<bool>`. The handler SHALL dispatch domain events via `IDispatcher.PublishDomainEventsAsync()` after save.

#### Scenario: Deleting a draft application
- **WHEN** `Delete()` is called on an application with status `draft`
- **THEN** an `ApplicationDeletedEvent` is raised and the handler proceeds with physical removal

#### Scenario: Deleting a non-draft application
- **WHEN** `Delete()` is called on an application with status other than `draft`
- **THEN** the system throws a `DomainException` with message "Nur Entwürfe können gelöscht werden"

### Requirement: No Action property in DTOs
`ApplicationCreateDto` and `ApplicationUpdateDto` SHALL NOT contain an `Action` property. Intent SHALL be expressed via the HTTP query parameter, not via DTO properties.

#### Scenario: DTO does not accept action field
- **WHEN** a request body contains `{ "action": "submit", ...data }` without the `submit=true` query parameter
- **THEN** the `action` field is ignored and the application is saved as a draft

### Requirement: No Decision property in DTOs
`ProcessorDecisionDto` SHALL be removed and replaced by `ApproveApplicationDto` and `RejectApplicationDto`.

#### Scenario: Old decide endpoint is removed
- **WHEN** processor sends `POST /api/processor/{id}/decide`
- **THEN** the system returns 404

### Requirement: Extracted ToCamelCase utility
The `ToCamelCase` helper method SHALL be extracted to a shared utility class and reused across all handlers that format validation errors.

#### Scenario: Validation errors use camelCase keys
- **WHEN** a validation error occurs for property `EmploymentStatus`
- **THEN** the error key in the response is `employmentStatus`

### Requirement: Controllers use IDispatcher
All controllers SHALL inject `IDispatcher` as their single handler dependency instead of individual `ICommandHandler<,>` and `IQueryHandler<,>` instances. Controllers SHALL call `dispatcher.SendAsync()` for commands and `dispatcher.QueryAsync()` for queries.

#### Scenario: ApplicationsController uses dispatcher
- **WHEN** `ApplicationsController` handles a POST request
- **THEN** it calls `dispatcher.SendAsync(new CreateApplicationCommand(...))` instead of `_createHandler.HandleAsync(...)`

#### Scenario: Controller constructor has single dependency
- **WHEN** `ApplicationsController` is constructed
- **THEN** it receives exactly one `IDispatcher` parameter (plus any non-handler dependencies)
