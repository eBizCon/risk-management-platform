## Why

The backend authentication layer was recently refactored to use ASP.NET Core's built-in OIDC middleware, but three structural issues remain: role identifiers are scattered as magic strings across controllers, every controller method manually checks roles and returns 403, and the OIDC configuration is 140+ lines of inline code in Program.cs. This cleanup makes the auth layer type-safe, DRY, and maintainable.

## What Changes

- Introduce `AppRoles` static class with `const string` role identifiers (consistent with existing `ApplicationStatuses` pattern) and typed `ClaimsPrincipal` extensions (`User.IsApplicant()`, `User.IsProcessor()`)
- Introduce `AuthPolicies` static class with `const string` policy names and configure ASP.NET Core authorization policies (`Applicant`, `Processor`, `ApplicantOrProcessor`)
- Replace all manual role checks in controllers with `[Authorize(Policy = AuthPolicies.X)]` attributes on controller/method level
- Implement `IAuthorizationMiddlewareResultHandler` for unified JSON error responses (401/403) on API routes
- Extract OIDC configuration into an `OidcOptions` class and `AddOidcAuthentication()` extension method, reducing Program.cs OIDC setup from ~140 lines to ~2 lines
- Remove redundant cookie event handlers for 401/403 (replaced by the centralized result handler)

## Capabilities

### New Capabilities

- `backend-auth-policies`: Centralized role constants, authorization policies, policy-based access control on controllers, and unified authorization error handling

### Modified Capabilities

(none — no existing specs)

## Impact

- **Controllers**: `ApplicationsController`, `ProcessorController`, `InquiryController`, `AuthController` — manual role checks removed, `[Authorize(Policy)]` attributes added
- **Program.cs**: OIDC/auth configuration extracted to extension method, authorization policies registered
- **Models/Enums.cs**: `AppRoles` and `AuthPolicies` added
- **Extensions/**: New `AuthenticationExtensions.cs` with `AddOidcAuthentication()`, new `OidcOptions.cs`, `JsonAuthorizationResultHandler.cs` added
- **Extensions/ClaimsPrincipalExtensions.cs**: Updated to use `AppRoles` constants, add `IsApplicant()`/`IsProcessor()` helpers
- **Extensions/RoleClaimHelper.cs**: Updated to reference `AppRoles.All` instead of local array
- **Tests**: `RoleClaimHelperTests` updated to use `AppRoles` constants
- **No breaking API changes** — all endpoints retain the same routes, request/response shapes, and HTTP status codes
