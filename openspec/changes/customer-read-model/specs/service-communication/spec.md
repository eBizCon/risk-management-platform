## MODIFIED Requirements

### Requirement: Customer Service internal endpoint for customer validation
The Customer Service SHALL expose `GET /api/internal/customers/{id}` returning the customer's essential data (id, firstName, lastName, status) for service-to-service validation. Additionally, the Customer Service SHALL expose `GET /api/internal/customers` returning all customers for initial read model synchronization.

#### Scenario: Customer exists and is active
- **WHEN** Application Service calls `GET /api/internal/customers/{id}` for an active customer
- **THEN** the Customer Service SHALL return HTTP 200 with customer data including `status: "active"`

#### Scenario: Customer exists but is archived
- **WHEN** Application Service calls `GET /api/internal/customers/{id}` for an archived customer
- **THEN** the Customer Service SHALL return HTTP 200 with customer data including `status: "archived"`

#### Scenario: Customer does not exist
- **WHEN** Application Service calls `GET /api/internal/customers/{id}` for a non-existent customer
- **THEN** the Customer Service SHALL return HTTP 404

#### Scenario: List all customers for sync
- **WHEN** Application Service calls `GET /api/internal/customers`
- **THEN** the Customer Service SHALL return HTTP 200 with a JSON array of all customers (active and archived) containing `id`, `firstName`, `lastName`, `status`

## ADDED Requirements

### Requirement: BFF proxy no longer routes customer-active to CustomerManagement
The frontend BFF SHALL route `/api/applications/customers` to the RiskManagement API instead of routing `/api/customers/active` to the CustomerManagement API for the application form customer dropdown. The existing `/api/customers/*` routes for customer CRUD operations SHALL remain unchanged.

#### Scenario: Application form customer dropdown route
- **WHEN** the frontend fetches `/api/applications/customers`
- **THEN** the BFF SHALL proxy to the RiskManagement API (not CustomerManagement)

#### Scenario: Customer CRUD routes unchanged
- **WHEN** the frontend fetches `/api/customers` or `/api/customers/{id}`
- **THEN** the BFF SHALL continue to proxy to the CustomerManagement API
