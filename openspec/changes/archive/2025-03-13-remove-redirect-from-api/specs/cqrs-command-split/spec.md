## MODIFIED Requirements

### Requirement: Single-intent create-and-submit command
The system SHALL provide a dedicated `CreateAndSubmitApplicationCommand` implementing `ICommand<CreateAndSubmitApplicationResult>` with its own handler that creates an application and immediately submits it in a single transaction. The handler SHALL be triggered by a `submit=true` query parameter. The handler SHALL dispatch domain events via `IDispatcher.PublishDomainEventsAsync()` after save. The result SHALL contain only the `ApplicationResponse` object, without a redirect path.

#### Scenario: Applicant creates and submits in one step
- **WHEN** applicant sends `POST /api/applications?submit=true` with valid application data
- **THEN** the application is created, scored, submitted, and the response contains only the application object

#### Scenario: Applicant creates a draft without submit parameter
- **WHEN** applicant sends `POST /api/applications` with valid application data (no `submit` query parameter)
- **THEN** the application is created as a draft and the response contains only the application object

### Requirement: Single-intent update-and-submit command
The system SHALL provide a dedicated `UpdateAndSubmitApplicationCommand` implementing `ICommand<UpdateAndSubmitApplicationResult>` with its own handler that updates an application and immediately submits it in a single transaction. The handler SHALL be triggered by a `submit=true` query parameter. The handler SHALL dispatch domain events via `IDispatcher.PublishDomainEventsAsync()` after save. The result SHALL contain only the `ApplicationResponse` object, without a redirect path.

#### Scenario: Applicant updates and submits in one step
- **WHEN** applicant sends `PUT /api/applications/{id}?submit=true` with valid application data
- **THEN** the application is updated, scored, submitted, and the response contains only the application object

#### Scenario: Applicant updates a draft without submit parameter
- **WHEN** applicant sends `PUT /api/applications/{id}` with valid application data (no `submit` query parameter)
- **THEN** the application is updated as a draft and the response contains only the application object

### Requirement: Single-intent approval command
The system SHALL provide a dedicated `ApproveApplicationCommand` implementing `ICommand<ApproveApplicationResult>` with its own handler that calls `Application.Approve()` on the aggregate. The handler SHALL accept an optional comment. The handler SHALL dispatch domain events via `IDispatcher.PublishDomainEventsAsync()` after save. The result SHALL contain only the `ApplicationResponse` object, without a redirect path.

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
The system SHALL provide a dedicated `RejectApplicationCommand` implementing `ICommand<RejectApplicationResult>` with its own handler that calls `Application.Reject()` on the aggregate. The handler SHALL require a comment. The handler SHALL dispatch domain events via `IDispatcher.PublishDomainEventsAsync()` after save. The result SHALL contain only the `ApplicationResponse` object, without a redirect path.

#### Scenario: Processor rejects an application with comment
- **WHEN** processor sends `POST /api/processor/{id}/reject` with `{ "comment": "Einkommen zu niedrig" }`
- **THEN** the application status changes to `rejected`, `ProcessorComment` is set, and `ProcessedAt` is set

#### Scenario: Processor tries to reject without comment
- **WHEN** processor sends `POST /api/processor/{id}/reject` with empty or missing comment
- **THEN** the system returns 400 with validation error "Bei Ablehnung ist eine Begründung erforderlich"

## ADDED Requirements

### Requirement: Command results contain only domain data
All command result records (`CreateApplicationResult`, `CreateAndSubmitApplicationResult`, `UpdateApplicationResult`, `UpdateAndSubmitApplicationResult`, `ApproveApplicationResult`, `RejectApplicationResult`) SHALL contain only the `ApplicationResponse` property. Result records SHALL NOT contain navigation hints, redirect paths, or any UI-specific data.

#### Scenario: Create command result has no redirect
- **WHEN** `CreateApplicationHandler` returns a successful result
- **THEN** the result contains `ApplicationResponse` and no `Redirect` property

#### Scenario: Controller returns only application data
- **WHEN** any command endpoint returns a successful response
- **THEN** the JSON response body contains the application object without a `redirect` field

### Requirement: Frontend owns navigation after mutations
The frontend SHALL determine navigation targets client-side after successful API mutations, using the `application.id` from the response. The frontend SHALL NOT rely on server-provided redirect paths.

#### Scenario: ApplicationForm navigates after create
- **WHEN** the applicant successfully creates or saves an application
- **THEN** the frontend navigates to `/applications/{id}` using the id from the response

#### Scenario: ApplicationForm navigates after submit
- **WHEN** the applicant successfully creates-and-submits or updates-and-submits an application
- **THEN** the frontend navigates to `/applications/{id}` using the id from the response
