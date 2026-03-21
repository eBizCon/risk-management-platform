## MODIFIED Requirements

### Requirement: Single-intent create-and-submit command
The system SHALL provide a dedicated `CreateAndSubmitApplicationCommand` implementing `ICommand<CreateAndSubmitApplicationResult>` with its own handler that creates a `Processing` application, saves it, and publishes an `ApplicationCreationStarted` event with `AutoSubmit = true` to the message bus. The handler SHALL return HTTP 202 Accepted. The handler SHALL dispatch domain events via `IDispatcher.PublishDomainEventsAsync()` after save. The result SHALL contain only the `ApplicationResponse` object, without a redirect path.

#### Scenario: Applicant creates and submits in one step (async)
- **WHEN** applicant sends `POST /api/applications?submit=true` with valid application data
- **THEN** the application is created with status "processing" and the response is HTTP 202
- **THEN** after saga completion, the application SHALL have status "submitted"

#### Scenario: Applicant creates a draft without submit parameter (async)
- **WHEN** applicant sends `POST /api/applications` with valid application data (no `submit` query parameter)
- **THEN** the application is created with status "processing" and the response is HTTP 202
- **THEN** after saga completion, the application SHALL have status "draft"

### Requirement: Controllers use IDispatcher
All controllers SHALL inject `IDispatcher` as their single handler dependency instead of individual `ICommandHandler<,>` and `IQueryHandler<,>` instances. Controllers SHALL call `dispatcher.SendAsync()` for commands and `dispatcher.QueryAsync()` for queries. For create commands, controllers SHALL return `Accepted()` (HTTP 202) instead of `Ok()` (HTTP 200).

#### Scenario: ApplicationsController returns 202 for create
- **WHEN** `ApplicationsController` handles a POST request for application creation
- **THEN** it SHALL call `dispatcher.SendAsync(new CreateApplicationCommand(...))` and return HTTP 202

#### Scenario: ApplicationsController returns 200 for update
- **WHEN** `ApplicationsController` handles a PUT request for application update
- **THEN** it SHALL call `dispatcher.SendAsync(new UpdateApplicationCommand(...))` and return HTTP 200 (update remains synchronous)

#### Scenario: Controller constructor has single dependency
- **WHEN** `ApplicationsController` is constructed
- **THEN** it receives exactly one `IDispatcher` parameter (plus any non-handler dependencies)
