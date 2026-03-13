## Context

Credit applications currently store customers as a plain `Name` string on the Application aggregate. There is no customer entity, no structured customer data, and no reuse across applications. The system needs a proper Customer bounded context that can serve multiple products (credit applications now, potentially bank accounts later).

Current state:
- `Application.Name` is a `string` — free text, no validation beyond length
- No `Customer` entity exists anywhere in the codebase
- Single deployment: `RiskManagement.sln` with Api, Application, Domain, Infrastructure layers
- PostgreSQL with `public` schema, EF Core, custom CQRS dispatcher
- SvelteKit frontend consuming Application Service API
- Keycloak for auth with roles `applicant` and `processor`

## Goals / Non-Goals

**Goals:**
- Introduce Customer as a first-class aggregate in its own bounded context and deployment
- Extract shared building blocks (SharedKernel) so both services reuse the same DDD foundations
- Replace `Application.Name` with a `CustomerId` reference
- Provide full customer CRUD + archive/activate UI for the `applicant` role
- Establish service-to-service communication pattern (synchronous HTTP + API key)

**Non-Goals:**
- Event-driven communication between services (synchronous HTTP is sufficient for now)
- Customer search/autocomplete (simple dropdown/select is enough for MVP)
- Customer data import/export
- Multi-tenancy or customer sharing across users
- Separate frontend for Customer Service (existing SvelteKit app serves both)

## Decisions

### D1: Separate Solution + Deployment for Customer Service

**Decision**: Customer Service lives in its own `CustomerManagement.sln` with the same 4-layer structure (Api, Application, Domain, Infrastructure).

**Rationale**: Customer is a separate bounded context that will serve multiple products. An independent deployment allows independent scaling and release cycles. Both services share the same PostgreSQL instance but use different schemas (`customer` vs `public`).

**Alternatives considered**:
- Same solution, separate namespace → too coupled, cannot deploy independently
- Separate DB entirely → unnecessary overhead, same DB with schema separation is sufficient

### D2: SharedKernel as a Shared Project Reference

**Decision**: Create a `SharedKernel` class library project referenced by both solutions via relative project reference (not a NuGet package).

**Rationale**: Both services are in the same repository. A project reference is simpler than publishing/consuming NuGet packages. If the repo is split later, SharedKernel becomes a NuGet package.

**Contents to extract from RiskManagement**:
- `Domain/Common/*` (AggregateRoot, Entity, Enumeration, IDomainEvent, IHasDomainEvents, ValueObject)
- `Application/Common/*` (ICommand, IQuery, IDispatcher, ICommandHandler, IQueryHandler, IDomainEventHandler, Result, ValidationHelper)
- `Infrastructure/Dispatching/Dispatcher.cs`
- `Domain/ValueObjects/EmailAddress.cs`
- `Domain/Exceptions/DomainException.cs`

### D3: Synchronous HTTP for Service-to-Service Communication

**Decision**: Services communicate via synchronous HTTP calls authenticated with a shared API key (`X-Api-Key` header).

**Rationale**: Simple, debuggable, no extra infrastructure. Acceptable for the current scale. Two integration points:
1. Application Service → Customer Service: GET `/api/internal/customers/{id}` (validate customer exists and is active when creating/updating application)
2. Customer Service → Application Service: GET `/api/internal/applications/exists?customerId={id}` (check before hard-deleting customer)

**API Key approach**: Both services share an API key via environment variable (`SERVICE_API_KEY`). A middleware in SharedKernel validates the `X-Api-Key` header on `/api/internal/*` routes. Frontend-facing endpoints continue using Keycloak JWT auth.

**Alternatives considered**:
- Keycloak service account tokens → more complex setup, overkill for two services in same network
- Event-driven (RabbitMQ) → eventual consistency issues for delete guard, extra infra

### D4: Customer Aggregate Design

**Decision**: Customer is an AggregateRoot with strongly-typed `CustomerId`, value objects for `Address` and `PhoneNumber`, and a status lifecycle (Active ↔ Archived).

**Fields**:
| Field | Type | Required |
|---|---|---|
| Id | CustomerId (int, auto-generated) | yes |
| FirstName | string (2-50 chars) | yes |
| LastName | string (2-50 chars) | yes |
| Email | EmailAddress (shared VO) | no |
| Phone | PhoneNumber (new VO) | yes |
| DateOfBirth | DateOnly | yes |
| Address | Address (new VO) | yes |
| Status | CustomerStatus (Active/Archived) | yes |
| CreatedBy | EmailAddress | yes |
| CreatedAt | DateTime | yes |
| UpdatedAt | DateTime? | no |

**Lifecycle**:
- `Create(...)` → status = Active
- `Update(...)` → guard: must be Active
- `Archive()` → guard: must be Active → status = Archived
- `Activate()` → guard: must be Archived → status = Active
- `Delete()` → guard: checked externally (no applications exist), called only after ACL check

### D5: Application ↔ Customer Link

**Decision**: Replace `Application.Name` (string) with `Application.CustomerId` (CustomerId). The Application aggregate stores only the ID reference — no navigation property.

**Query-time resolution**: When returning `ApplicationResponse`, the query handler (or a denormalized read model) resolves customer name. For now: the frontend fetches customer details separately via Customer Service API, or the Application query handler calls Customer Service internally.

**Simpler approach chosen**: `ApplicationResponse` includes `customerId` + `customerName`. The `customerName` is resolved in the query handler via HTTP call to Customer Service. This avoids denormalization complexity.

### D6: Database Schema Separation

**Decision**: Customer Service uses PostgreSQL schema `customer`. Table: `customer.customers`.

**EF Core configuration**: `CustomerDbContext` sets `HasDefaultSchema("customer")` in `OnModelCreating`. Both services connect to the same PostgreSQL instance with the same connection string. Schema isolation is logical, not physical.

### D7: Frontend Customer UI

**Decision**: Customer management pages live in the existing SvelteKit frontend under `/customers`. The frontend calls both backend APIs (different base URLs configured via environment variables).

**Routes**:
- `/customers` — list with table/card toggle (consistent with application list pattern)
- `/customers/new` — create form
- `/customers/[id]` — detail view (with linked applications info)
- `/customers/[id]/edit` — edit form

**Application form change**: Replace name text input with a customer select dropdown. Dropdown shows active customers only, formatted as "LastName, FirstName". Option to navigate to `/customers/new` if no suitable customer exists.

## Risks / Trade-offs

- **Runtime coupling via HTTP**: If Customer Service is down, Application Service cannot create applications (customer validation fails). → Mitigation: health checks, circuit breaker pattern can be added later if needed.
- **SharedKernel coupling**: Changes to SharedKernel affect both services. → Mitigation: SharedKernel should be stable (DDD building blocks rarely change). Semantic versioning discipline.
- **Cross-schema query temptation**: Having both schemas in the same DB makes it tempting to bypass the API and query directly. → Mitigation: Discipline. ACL services are the only allowed cross-boundary integration point.
- **API key security**: Shared API key is simpler but less secure than per-service keys or mTLS. → Mitigation: Acceptable for current scale. Internal endpoints are not exposed externally. Can upgrade to mTLS later.
- **Customer name denormalization**: Query handler calls Customer Service for every application read. → Mitigation: Acceptable for current scale. Can cache or denormalize later if performance becomes an issue.
