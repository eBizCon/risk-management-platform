## ADDED Requirements

### Requirement: API key authentication for internal endpoints
The system SHALL authenticate service-to-service HTTP calls using a shared API key passed in the `X-Api-Key` request header. The API key SHALL be configured via the `SERVICE_API_KEY` environment variable.

#### Scenario: Valid API key on internal endpoint
- **WHEN** a service calls an internal endpoint (path starting with `/api/internal/`) with a valid `X-Api-Key` header
- **THEN** the system SHALL allow the request

#### Scenario: Missing API key on internal endpoint
- **WHEN** a request to an internal endpoint lacks the `X-Api-Key` header
- **THEN** the system SHALL return HTTP 401

#### Scenario: Invalid API key on internal endpoint
- **WHEN** a request to an internal endpoint has an incorrect `X-Api-Key` value
- **THEN** the system SHALL return HTTP 401

#### Scenario: Public endpoints unaffected
- **WHEN** a request targets a non-internal endpoint (e.g., `/api/customers`)
- **THEN** the system SHALL NOT require the `X-Api-Key` header (standard Keycloak JWT auth applies)

### Requirement: API key middleware in SharedKernel
The SharedKernel SHALL provide an `ApiKeyAuthMiddleware` that both services can register. It SHALL only apply to routes matching `/api/internal/*`.

#### Scenario: Middleware registration
- **WHEN** a service registers `ApiKeyAuthMiddleware`
- **THEN** it SHALL intercept only requests to `/api/internal/*` paths

### Requirement: Customer Service internal endpoint for customer validation
The Customer Service SHALL expose `GET /api/internal/customers/{id}` returning the customer's essential data (id, firstName, lastName, status) for service-to-service validation.

#### Scenario: Customer exists and is active
- **WHEN** Application Service calls `GET /api/internal/customers/{id}` for an active customer
- **THEN** the Customer Service SHALL return HTTP 200 with customer data including `status: "active"`

#### Scenario: Customer exists but is archived
- **WHEN** Application Service calls `GET /api/internal/customers/{id}` for an archived customer
- **THEN** the Customer Service SHALL return HTTP 200 with customer data including `status: "archived"`

#### Scenario: Customer does not exist
- **WHEN** Application Service calls `GET /api/internal/customers/{id}` for a non-existent customer
- **THEN** the Customer Service SHALL return HTTP 404

### Requirement: Application Service internal endpoint for application existence check
The Application Service SHALL expose `GET /api/internal/applications/exists?customerId={id}` returning whether any applications exist for a given customer.

#### Scenario: Customer has applications
- **WHEN** Customer Service calls `GET /api/internal/applications/exists?customerId={id}` for a customer with one or more applications
- **THEN** the Application Service SHALL return HTTP 200 with `{ "exists": true }`

#### Scenario: Customer has no applications
- **WHEN** Customer Service calls `GET /api/internal/applications/exists?customerId={id}` for a customer with zero applications
- **THEN** the Application Service SHALL return HTTP 200 with `{ "exists": false }`

### Requirement: Service HTTP client configuration
Each service SHALL configure a typed `HttpClient` for calling the other service, with the API key header automatically attached and the base URL configured via environment variable.

#### Scenario: Customer Service calls Application Service
- **WHEN** Customer Service needs to check application existence
- **THEN** it SHALL use a typed HttpClient configured with `APPLICATION_SERVICE_URL` base URL and `SERVICE_API_KEY` header

#### Scenario: Application Service calls Customer Service
- **WHEN** Application Service needs to validate a customer
- **THEN** it SHALL use a typed HttpClient configured with `CUSTOMER_SERVICE_URL` base URL and `SERVICE_API_KEY` header
