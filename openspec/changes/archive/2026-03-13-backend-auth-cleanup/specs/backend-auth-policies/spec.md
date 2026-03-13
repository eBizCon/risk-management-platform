## ADDED Requirements

### Requirement: Centralized role constants
The system SHALL define all application role identifiers as compile-time string constants in a single `AppRoles` static class. All code referencing role values MUST use these constants instead of string literals.

#### Scenario: Role constants are used everywhere
- **WHEN** any code in the backend needs to reference a role identifier
- **THEN** it MUST use `AppRoles.Applicant` or `AppRoles.Processor` instead of a string literal

#### Scenario: Role constants match Keycloak claim values
- **WHEN** `AppRoles.Applicant` is referenced
- **THEN** its value SHALL be `"applicant"`
- **WHEN** `AppRoles.Processor` is referenced
- **THEN** its value SHALL be `"processor"`

### Requirement: Typed ClaimsPrincipal role helpers
The system SHALL provide typed extension methods on `ClaimsPrincipal` for role checking. These methods MUST use `AppRoles` constants internally.

#### Scenario: Check if user is applicant
- **WHEN** `User.IsApplicant()` is called on an authenticated user with role `"applicant"`
- **THEN** it SHALL return `true`

#### Scenario: Check if user is processor
- **WHEN** `User.IsProcessor()` is called on an authenticated user with role `"processor"`
- **THEN** it SHALL return `true`

### Requirement: Authorization policies for role-based access
The system SHALL define named authorization policies for each role and role combination. Policy names MUST be defined as compile-time string constants in an `AuthPolicies` static class.

#### Scenario: Applicant policy
- **WHEN** an endpoint is protected with `AuthPolicies.Applicant`
- **THEN** only authenticated users with role `"applicant"` SHALL be granted access

#### Scenario: Processor policy
- **WHEN** an endpoint is protected with `AuthPolicies.Processor`
- **THEN** only authenticated users with role `"processor"` SHALL be granted access

#### Scenario: ApplicantOrProcessor policy
- **WHEN** an endpoint is protected with `AuthPolicies.ApplicantOrProcessor`
- **THEN** authenticated users with role `"applicant"` or `"processor"` SHALL be granted access

### Requirement: Declarative policy enforcement on controllers
Controller endpoints SHALL use `[Authorize(Policy = AuthPolicies.X)]` attributes for role-based access control. Controller methods MUST NOT contain manual role-check boilerplate that returns 401 or 403 for role mismatches.

#### Scenario: ApplicationsController requires applicant role
- **WHEN** an unauthenticated or non-applicant user accesses `POST /api/applications`
- **THEN** the framework SHALL reject the request before the controller method executes

#### Scenario: ProcessorController requires processor role
- **WHEN** an unauthenticated or non-processor user accesses `GET /api/processor/applications`
- **THEN** the framework SHALL reject the request before the controller method executes

#### Scenario: InquiryController supports mixed roles per method
- **WHEN** `GET /api/applications/{id}/inquiries` is accessed
- **THEN** both applicant and processor roles SHALL be allowed
- **WHEN** `POST /api/applications/{id}/inquiry` is accessed
- **THEN** only the processor role SHALL be allowed
- **WHEN** `POST /api/applications/{id}/inquiry/response` is accessed
- **THEN** only the applicant role SHALL be allowed

### Requirement: Unified JSON error responses for authorization failures
The system SHALL return consistent JSON error responses for all authorization failures on API routes (`/api/*`). A single `IAuthorizationMiddlewareResultHandler` MUST handle all cases.

#### Scenario: Unauthenticated API request
- **WHEN** an unauthenticated request hits a protected `/api/*` endpoint
- **THEN** the response SHALL be HTTP 401 with body `{ "error": "Login erforderlich" }`

#### Scenario: Forbidden API request
- **WHEN** an authenticated user without the required role hits a protected `/api/*` endpoint
- **THEN** the response SHALL be HTTP 403 with body `{ "error": "Keine Berechtigung" }`

#### Scenario: Non-API route authorization failure
- **WHEN** a non-API route authorization fails
- **THEN** the default ASP.NET Core behavior SHALL apply (redirect or status code)

### Requirement: OIDC configuration as Options pattern
All OIDC-related environment variables (`OIDC_ISSUER`, `OIDC_CLIENT_ID`, `OIDC_CLIENT_SECRET`, `OIDC_SCOPE`, `OIDC_ROLES_CLAIM_PATH`, `OIDC_POST_LOGOUT_REDIRECT_URI`) SHALL be bound to an `OidcOptions` class. The OpenID Connect and cookie authentication setup SHALL be encapsulated in a single `AddOidcAuthentication()` extension method.

#### Scenario: Program.cs uses extension method
- **WHEN** authentication is configured in Program.cs
- **THEN** it SHALL use `builder.Services.AddOidcAuthentication(builder.Configuration, builder.Environment)` instead of inline configuration

#### Scenario: OidcOptions are injectable
- **WHEN** a service needs OIDC configuration values
- **THEN** it SHALL receive them via `IOptions<OidcOptions>` dependency injection
