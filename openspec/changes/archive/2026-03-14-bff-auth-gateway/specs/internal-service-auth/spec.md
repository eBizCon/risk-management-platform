## ADDED Requirements

### Requirement: InternalAuthMiddleware in SharedKernel
The SharedKernel SHALL provide an `InternalAuthMiddleware` that replaces the existing `ApiKeyAuthMiddleware`. This middleware SHALL apply to ALL `/api/*` routes (not just `/api/internal/*`). It SHALL validate the `X-Api-Key` header against the configured `SERVICE_API_KEY` and construct a `ClaimsPrincipal` from `X-User-Id`, `X-User-Email`, `X-User-Name`, `X-User-Role` headers.

#### Scenario: Valid API key and user headers
- **WHEN** a request arrives at `/api/applications` with a valid `X-Api-Key` and `X-User-Id`, `X-User-Email`, `X-User-Name`, `X-User-Role` headers
- **THEN** the middleware SHALL create a `ClaimsPrincipal` with `ClaimTypes.NameIdentifier`, `ClaimTypes.Email`, `ClaimTypes.Name`, `ClaimTypes.Role` claims and set `HttpContext.User`

#### Scenario: Missing API key returns 401
- **WHEN** a request arrives at `/api/applications` without an `X-Api-Key` header
- **THEN** the middleware SHALL return HTTP 401 with `{"error": "Unauthorized"}`

#### Scenario: Invalid API key returns 401
- **WHEN** a request arrives at `/api/applications` with an incorrect `X-Api-Key`
- **THEN** the middleware SHALL return HTTP 401 with `{"error": "Unauthorized"}`

#### Scenario: Missing user headers still authenticated with API key only
- **WHEN** a request arrives at `/api/internal/customers/5` with a valid `X-Api-Key` but without `X-User-*` headers
- **THEN** the middleware SHALL allow the request through without setting user claims (for service-to-service calls that don't act on behalf of a user)

#### Scenario: Health endpoint bypasses middleware
- **WHEN** a request arrives at `/health`
- **THEN** the middleware SHALL pass the request through without any authentication checks

### Requirement: Remove OIDC authentication from RiskManagement.Api
The RiskManagement.Api SHALL NOT configure OIDC authentication, cookie authentication, or DataProtection shared keys. The `AuthController` (login/logout/user endpoints), `AuthenticationExtensions`, `OidcOptions`, and related OIDC NuGet dependencies SHALL be removed. Authorization policies (`AuthPolicies.Applicant`, `AuthPolicies.Processor`, `AuthPolicies.RiskManager`, `AuthPolicies.ApplicantOrProcessor`) SHALL remain but use the identity provided by `InternalAuthMiddleware`.

#### Scenario: No OIDC middleware in pipeline
- **WHEN** `RiskManagement.Api` starts
- **THEN** the middleware pipeline SHALL NOT include `UseAuthentication()` with OIDC scheme or cookie scheme from ASP.NET

#### Scenario: Authorization policies still enforce roles
- **WHEN** a request arrives with `X-User-Role: applicant` and targets an endpoint requiring `AuthPolicies.Processor`
- **THEN** the request SHALL be rejected with HTTP 403

#### Scenario: Existing controller [Authorize] attributes unchanged
- **WHEN** `ApplicationsController` is inspected
- **THEN** it SHALL still have `[Authorize(Policy = AuthPolicies.Applicant)]` attribute

### Requirement: Remove OIDC authentication from CustomerManagement.Api
The CustomerManagement.Api SHALL NOT configure OIDC authentication, cookie authentication, or DataProtection shared keys. The `AuthenticationExtensions`, `OidcOptions`, and related files SHALL be removed. Authorization policies SHALL remain and use the identity from `InternalAuthMiddleware`.

#### Scenario: No OIDC middleware in CustomerManagement.Api
- **WHEN** `CustomerManagement.Api` starts
- **THEN** the middleware pipeline SHALL NOT include OIDC or cookie authentication

#### Scenario: Customer endpoints still enforce authorization
- **WHEN** a request arrives with `X-User-Role: processor` and targets `/api/customers` requiring `AuthPolicies.Applicant`
- **THEN** the request SHALL be rejected with HTTP 403

### Requirement: Retain TestSessionController for backward compatibility
The `TestSessionController` in RiskManagement.Api SHALL remain functional but adapt to the new auth model. In development/test mode, it SHALL create a `ClaimsPrincipal` directly (without cookie authentication) for integration tests that call the API directly.

#### Scenario: Direct API integration test
- **WHEN** an integration test POSTs to `/api/test/session` on the RiskManagement.Api directly
- **THEN** a session SHALL be created that works for subsequent requests in the same test context

### Requirement: Remove DataProtection shared-keys
Both C# services SHALL NOT configure `AddDataProtection().PersistKeysToFileSystem()` with a shared keys path. The `shared-keys/` directory is no longer needed.

#### Scenario: No shared-keys directory dependency
- **WHEN** either service starts
- **THEN** it SHALL NOT attempt to read from or write to a `shared-keys/` directory
