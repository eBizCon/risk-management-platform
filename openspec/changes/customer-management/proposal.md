## Why

Credit applications currently store the customer as a plain `Name` string — no structured customer data, no reuse across applications, no lifecycle management. This is not business-realistic. Customers need to be first-class entities that can be created, managed, and linked to multiple products (credit applications now, potentially bank accounts later). A separate Customer bounded context also enables independent scaling and deployment.

## What Changes

- **New Customer Service** as a separate deployment (`CustomerManagement.sln`) with its own bounded context, using the `customer` PostgreSQL schema on the shared database
- **SharedKernel library** extracting common building blocks (AggregateRoot, Dispatcher, Result, ValueObject, EmailAddress, DomainException) from `RiskManagement` so both services can reuse them
- **Customer Aggregate** with full CRUD + archive/activate lifecycle: FirstName, LastName, Email (optional), Phone (ValueObject), DateOfBirth, Address (ValueObject, required), Status (Active/Archived)
- **Customer UI** in the existing SvelteKit frontend: `/customers` list, `/customers/new`, `/customers/[id]`, `/customers/[id]/edit`
- **Application refactoring**: Replace `Application.Name` (string) with `Application.CustomerId` (strongly typed ID). Application form gets a customer select instead of text input.
- **Service-to-service communication** via synchronous HTTP calls with shared API key auth:
  - Application Service → Customer Service: validate customer exists and is active when creating/updating an application
  - Customer Service → Application Service: check if customer has applications before allowing hard delete
- **Shared Keycloak auth**: Both services use the same Keycloak realm. Customer management is accessible to the `applicant` role.

## Capabilities

### New Capabilities
- `customer-crud`: Full customer lifecycle management — create, read, update, delete, archive, activate. Includes Customer aggregate, repository, API endpoints, and customer management UI.
- `shared-kernel`: Extract common DDD building blocks into a shared library used by both services (AggregateRoot, Entity, ValueObject, Enumeration, Dispatcher, Result pattern, EmailAddress, DomainException).
- `service-communication`: Synchronous HTTP service-to-service calls with shared API key authentication. Includes internal endpoints, API key middleware, and HttpClient configuration.

### Modified Capabilities
- `application-customer-link`: Refactor Application aggregate to reference Customer by ID instead of storing a plain name string. Update DTOs, validators, form, and API to use customer selection.

## Impact

- **Backend**: New `CustomerManagement.sln` solution (Api, Application, Domain, Infrastructure). New `SharedKernel` project referenced by both solutions. Refactoring of `RiskManagement.Domain` and `RiskManagement.Application` to use SharedKernel and CustomerId.
- **Database**: New `customer` schema with `customers` table. Column change on `applications` table: `name` → `customer_id`. No migration needed (app not in production).
- **Frontend**: New `/customers` routes and components. Modified `ApplicationForm.svelte` (customer select instead of name input). Updated TypeScript types.
- **Infrastructure**: New deployment target for Customer Service. Shared API key environment variables. Docker Compose update for local dev.
- **Auth**: Shared Keycloak realm. Customer management restricted to `applicant` role.
