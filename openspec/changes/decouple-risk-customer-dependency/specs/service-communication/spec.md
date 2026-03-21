## MODIFIED Requirements

### Requirement: Service HTTP client configuration
Each service SHALL configure a typed `HttpClient` for calling the other service, with the API key header automatically attached and the base URL configured via environment variable. The Risk Service SHALL only use typed HTTP clients within saga consumers and the bootstrap sync service, NOT in command handlers.

#### Scenario: Customer Service calls Application Service
- **WHEN** Customer Service needs to check application existence
- **THEN** it SHALL use a typed HttpClient configured with `APPLICATION_SERVICE_URL` base URL and `SERVICE_API_KEY` header

#### Scenario: Application Service calls Customer Service from saga consumer
- **WHEN** the `FetchCustomerProfileConsumer` needs to fetch customer data
- **THEN** it SHALL use a typed HttpClient configured with `CUSTOMER_SERVICE_URL` base URL and `SERVICE_API_KEY` header

#### Scenario: Application Service command handlers do not call Customer Service
- **WHEN** a command handler processes an Update, UpdateAndSubmit, or Submit request
- **THEN** the handler SHALL NOT make any HTTP calls to the Customer Service
- **THEN** the handler SHALL delegate external data fetching to the saga pipeline

## REMOVED Requirements

### Requirement: ICustomerNameService HTTP client registration
**Reason**: Replaced entirely by `ICustomerReadModelRepository.GetCustomerNamesAsync()` which reads from the local Customer Read Model table (populated via domain events). The `ICustomerNameService` interface and its `AddHttpClient` registration are dead code with zero consumers.
**Migration**: All query handlers already use `ICustomerReadModelRepository`. No code changes needed beyond deletion of `ICustomerNameService`, removal of corresponding methods from `CustomerServiceClient`, and removal of the `AddHttpClient<ICustomerNameService>` DI registration.
