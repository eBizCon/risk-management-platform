## 1. SharedKernel Extraction

- [x] 1.1 Create `SharedKernel/SharedKernel.csproj` class library project targeting net8.0
- [x] 1.2 Move DDD base types from `RiskManagement.Domain/Common/` to `SharedKernel/Common/` (AggregateRoot, Entity, Enumeration, IDomainEvent, IHasDomainEvents, ValueObject)
- [x] 1.3 Move `DomainException` from `RiskManagement.Domain/Exceptions/` to `SharedKernel/Exceptions/`
- [x] 1.4 Move `EmailAddress` value object from `RiskManagement.Domain/ValueObjects/` to `SharedKernel/ValueObjects/`
- [x] 1.5 Move CQRS interfaces from `RiskManagement.Application/Common/` to `SharedKernel/Dispatching/` (ICommand, IQuery, IDispatcher, ICommandHandler, IQueryHandler, IDomainEventHandler)
- [x] 1.6 Move `Result<T>` and `ValidationHelper` from `RiskManagement.Application/Common/` to `SharedKernel/Results/`
- [x] 1.7 Move `Dispatcher` implementation from `RiskManagement.Infrastructure/Dispatching/` to `SharedKernel/Dispatching/`
- [x] 1.8 Add `SharedKernel` project reference to `RiskManagement.Domain`, `RiskManagement.Application`, and `RiskManagement.Infrastructure`
- [x] 1.9 Update all `using` statements in RiskManagement projects to reference SharedKernel namespaces
- [x] 1.10 Verify RiskManagement.sln compiles and all existing tests pass

## 2. SharedKernel: API Key Middleware

- [x] 2.1 Create `SharedKernel/Middleware/ApiKeyAuthMiddleware.cs` that validates `X-Api-Key` header on `/api/internal/*` routes against `SERVICE_API_KEY` config
- [x] 2.2 Create `SharedKernel/DependencyInjection.cs` with `AddSharedKernel` extension for Dispatcher + handler registration
- [ ] 2.3 Write unit tests for ApiKeyAuthMiddleware (valid key, invalid key, missing key, non-internal path passes through)

## 3. Customer Service: Solution Setup

- [x] 3.1 Create `CustomerManagement.sln` with projects: `CustomerManagement.Api`, `CustomerManagement.Application`, `CustomerManagement.Domain`, `CustomerManagement.Infrastructure`
- [x] 3.2 Add SharedKernel project reference to Domain, Application, and Infrastructure projects
- [x] 3.3 Configure `CustomerManagement.Api/Program.cs` with Keycloak OIDC auth, CORS, API key middleware, and PostgreSQL connection
- [x] 3.4 Add NuGet dependencies (EF Core, Npgsql, FluentValidation, etc.) matching RiskManagement versions

## 4. Customer Service: Domain Layer

- [x] 4.1 Create `CustomerId` strongly typed ID in `CustomerManagement.Domain/Aggregates/CustomerAggregate/`
- [x] 4.2 Create `Address` value object (Street, City, ZipCode, Country) in `CustomerManagement.Domain/ValueObjects/`
- [x] 4.3 Create `PhoneNumber` value object with validation in `CustomerManagement.Domain/ValueObjects/`
- [x] 4.4 Create `CustomerStatus` enumeration (Active, Archived) in `CustomerManagement.Domain/Aggregates/CustomerAggregate/`
- [x] 4.5 Create `Customer` aggregate with Create, Update, Archive, Activate, Delete methods and guard clauses
- [x] 4.6 Create `ICustomerRepository` interface in `CustomerManagement.Domain/Aggregates/CustomerAggregate/`
- [ ] 4.7 Write unit tests for Customer aggregate (create, update, archive, activate, guard clauses)
- [ ] 4.8 Write unit tests for Address and PhoneNumber value objects

## 5. Customer Service: Application Layer

- [x] 5.1 Create `CreateCustomerCommand` + handler
- [x] 5.2 Create `UpdateCustomerCommand` + handler
- [x] 5.3 Create `DeleteCustomerCommand` + handler (with ACL check via ApplicationServiceClient)
- [x] 5.4 Create `ArchiveCustomerCommand` + handler
- [x] 5.5 Create `ActivateCustomerCommand` + handler
- [x] 5.6 Create `GetCustomerQuery` + handler
- [x] 5.7 Create `GetCustomersByUserQuery`, `GetActiveCustomersByUserQuery`, `GetCustomerInternalQuery` + handlers
- [x] 5.8 Create `CustomerCreateDto`, `CustomerUpdateDto`, `CustomerResponse`, `CustomerInternalResponse`, `CustomerMapper`
- [x] 5.9 Create `CustomerCreateValidator` and `CustomerUpdateValidator` with FluentValidation
- [x] 5.10 ACL implemented via `ApplicationServiceClient` typed HttpClient in Infrastructure

## 6. Customer Service: Infrastructure Layer

- [x] 6.1 Create `CustomerDbContext` with `HasDefaultSchema("customer")` and `DbSet<Customer>` (snake_case column naming)
- [x] 6.2 Entity config inline in DbContext with value object conversions for Address, PhoneNumber, EmailAddress, CustomerStatus, CustomerId
- [x] 6.3 Create `CustomerRepository` implementing `ICustomerRepository`
- [x] 6.4 Create `ApplicationServiceClient` typed HttpClient calling Application Service internal endpoint with API key
- [x] 6.5 Create `DependencyInjection.cs` with service registration (DbContext, repository, dispatcher, handlers, validators, HttpClient)
- [x] 6.6 Register ApiKeyAuthMiddleware in Program.cs for internal endpoints

## 7. Customer Service: API Layer

- [x] 7.1 Create `CustomersController` with CRUD + archive/activate endpoints (authorized for `applicant` role)
- [x] 7.2 Create `InternalCustomersController` for `/api/internal/customers/{id}` (API key auth)
- [x] 7.3 Add auth policies, role configuration, OIDC auth extensions, ClaimsPrincipal extensions, ResultExtensions
- [ ] 7.4 Write integration tests for Customer API endpoints

## 8. Application Service: Internal Endpoint

- [x] 8.1 Create `InternalApplicationsController` with `GET /api/internal/applications/exists?customerId={id}` endpoint
- [x] 8.2 Add `CheckApplicationsExistQuery` + handler in RiskManagement.Application
- [x] 8.3 Add `ExistsForCustomerAsync` method to `IApplicationRepository` and `ApplicationRepository`
- [x] 8.4 Register ApiKeyAuthMiddleware in RiskManagement.Api Program.cs
- [ ] 8.5 Write tests for internal endpoint

## 9. Application Service: Customer Link Refactoring

- [x] 9.1 Replace `Name` property with `CustomerId` (int) on Application aggregate
- [x] 9.2 Update `Application.Create(...)` and `Application.UpdateDetails(...)` signatures to use `int customerId`
- [ ] 9.3 Create `ICustomerValidationService` interface and `CustomerValidationService` (HTTP client calling Customer Service)
- [x] 9.4 Update `CreateApplicationCommand` handler to pass `CustomerId`
- [x] 9.5 Update `CreateAndSubmitApplicationCommand` handler with `CustomerId`
- [x] 9.6 Update `UpdateApplicationCommand` and `UpdateAndSubmitApplicationCommand` handlers with `CustomerId`
- [x] 9.7 Update `ApplicationCreateDto` and `ApplicationUpdateDto`: replace `name` with `customerId`
- [x] 9.8 Update `ApplicationResponse` to include `customerId` and `customerName`
- [x] 9.9 Update `ApplicationMapper` to map `customerId`
- [ ] 9.10 Update `GetApplicationQuery` and `GetApplicationsByUserQuery` handlers to resolve customer name via HTTP
- [x] 9.11 Update `ApplicationValidator` and `ApplicationUpdateValidator`: replace name rules with customerId rules
- [x] 9.12 Update `ApplicationConfiguration` EF config: replace `name` column with `customer_id` column
- [ ] 9.13 Register `ICustomerValidationService` and HttpClient in DependencyInjection.cs
- [x] 9.14 Update existing application tests to use CustomerId instead of Name
- [x] 9.15 Create EF Core migration `RenameNameToCustomerId`
- [x] 9.16 Create EF Core migration `InitialCreate` for CustomerManagement
- [x] 9.17 Update DatabaseSeeder to use `customerId` instead of string name

## 10. Frontend: Customer UI

- [ ] 10.1 Add Customer TypeScript types to `types.ts` (Customer interface, CustomerStatus, etc.)
- [ ] 10.2 Create `/customers/+page.ts` loader fetching customers from Customer Service API
- [ ] 10.3 Create `/customers/+page.svelte` with customer list (table/card toggle, showing name, email, phone, city)
- [ ] 10.4 Create `CustomerCard.svelte` component for mobile card view
- [ ] 10.5 Create `CustomerTable.svelte` component for table view
- [ ] 10.6 Create `/customers/new/+page.svelte` with customer creation form (all fields including address)
- [ ] 10.7 Create `/customers/new/+page.ts` loader
- [ ] 10.8 Create `CustomerForm.svelte` shared form component (used by create and edit pages)
- [ ] 10.9 Create `/customers/[id]/+page.svelte` with customer detail view and action buttons
- [ ] 10.10 Create `/customers/[id]/+page.ts` loader
- [ ] 10.11 Create `/customers/[id]/edit/+page.svelte` with pre-filled edit form
- [ ] 10.12 Create `/customers/[id]/edit/+page.ts` loader
- [ ] 10.13 Add `/customers` link to navigation menu (visible for applicant role)

## 11. Frontend: Application Form Refactoring

- [ ] 11.1 Update `ApplicationForm.svelte`: replace name text input with customer select dropdown
- [ ] 11.2 Fetch active customers from Customer Service API for the dropdown
- [ ] 11.3 Format dropdown options as "LastName, FirstName"
- [ ] 11.4 Add "Neuen Kunden anlegen" link when no customers exist
- [ ] 11.5 Update `Application` TypeScript interface: replace `name` with `customerId` and `customerName`
- [ ] 11.6 Update application list/card/table components to display `customerName` instead of `name`
- [ ] 11.7 Update application detail page to show customer name with link to customer detail

## 12. Infrastructure & Configuration

- [ ] 12.1 Add Customer Service to `dev/docker-compose.yml` (new container, port 5001)
- [ ] 12.2 Add `SERVICE_API_KEY` environment variable to both services in docker-compose
- [ ] 12.3 Add `CUSTOMER_SERVICE_URL` env var to Application Service config
- [ ] 12.4 Add `APPLICATION_SERVICE_URL` env var to Customer Service config
- [ ] 12.5 Add Customer Service API base URL to frontend `.env` configuration
- [ ] 12.6 Configure SvelteKit proxy for Customer Service API routes
- [ ] 12.7 Update database seed script to create `customer` schema

## 13. E2E Tests

- [ ] 13.1 Write E2E tests for customer list page (view, table/card toggle)
- [ ] 13.2 Write E2E tests for customer creation (valid, validation errors)
- [ ] 13.3 Write E2E tests for customer detail page (view, archive, activate, delete)
- [ ] 13.4 Write E2E tests for customer edit page
- [ ] 13.5 Write E2E tests for application form with customer selection
- [ ] 13.6 Verify existing application E2E tests pass after refactoring
