## Context

The backend OIDC flow was recently refactored to use ASP.NET Core's built-in authentication middleware. The result works correctly but has three structural issues:

1. **Magic strings**: Role identifiers `"applicant"` and `"processor"` are scattered as string literals across ~20 locations in 5 controllers and 2 extension classes
2. **Manual auth checks**: Every controller method manually checks `User.GetRole()` and returns `StatusCode(403, ...)` — identical boilerplate repeated ~12 times
3. **Monolithic Program.cs**: ~140 lines of inline OIDC/cookie/authorization configuration make Program.cs hard to read

The codebase already uses `static class` + `const string` for similar domain values (`ApplicationStatuses`, `EmploymentStatuses` in `Models/Enums.cs`).

## Goals / Non-Goals

**Goals:**
- Eliminate all role magic strings via `AppRoles` constants
- Remove manual role-check boilerplate from controller methods using ASP.NET Core authorization policies and `[Authorize(Policy)]` attributes
- Centralize 401/403 JSON error responses in a single `IAuthorizationMiddlewareResultHandler`
- Extract OIDC configuration into `OidcOptions` + `AddOidcAuthentication()` extension method
- Keep all existing HTTP status codes and response shapes unchanged

**Non-Goals:**
- Changing the OIDC provider (Keycloak) or its configuration
- Adding new roles or permissions
- Modifying frontend code
- Introducing a full RBAC/permission system beyond the two existing roles
- Resource-ownership checks (e.g. `application.CreatedBy != User.GetEmail()`) — these remain as business logic in controllers

## Decisions

### D1: Roles as `static class AppRoles` with `const string`

**Choice**: Static constants (like existing `ApplicationStatuses` pattern)
**Over**: Enum with mapping extensions

**Rationale**: Claims are strings, the DB stores strings, Keycloak delivers strings. An enum would require a mapping layer at every boundary (claim read, claim write, test setup) with no real benefit. `const string` works in attributes, switch expressions, and is compile-time.

### D2: Policy names as `static class AuthPolicies` with `const string`

**Choice**: Separate `AuthPolicies` class
**Over**: Nested class inside `AppRoles`

**Rationale**: Policies may eventually include non-role-based policies (e.g. resource ownership). Keeping them separate follows SRP. `const string` + `nameof()` gives rename safety and works in `[Authorize(Policy = ...)]` attributes.

### D3: `[Authorize(Policy)]` on controller/method level

**Choice**: Declarative attributes per controller (default) with per-method overrides
**Over**: Convention-based middleware, base controller with `RequireRole()` helper

**Rationale**: Framework-native, zero boilerplate in method bodies, visible at the declaration site. The `InquiryController` (mixed roles per method) works naturally with per-method `[Authorize(Policy)]` overrides.

### D4: `IAuthorizationMiddlewareResultHandler` for unified error responses

**Choice**: Custom `JsonAuthorizationResultHandler` registered as singleton
**Over**: Cookie event handlers (`OnRedirectToLogin`, `OnRedirectToAccessDenied`)

**Rationale**: Single place for all 401/403 formatting. Replaces the current cookie event handlers that duplicate the same logic. Non-API routes fall through to the default handler.

### D5: `OidcOptions` + `AddOidcAuthentication()` extension method

**Choice**: Options class bound from environment variables + extension method in `Extensions/AuthenticationExtensions.cs`
**Over**: Inline configuration in Program.cs

**Rationale**: Program.cs becomes ~2 lines for auth setup. The extension method encapsulates cookie config, OIDC config, authorization policies, and the result handler registration. `OidcOptions` makes configuration injectable and testable.

## Risks / Trade-offs

- **[Authorize] hides auth logic from method body** → Acceptable because role requirements are visible at declaration. Business-logic checks (ownership) remain explicit in method body.
- **Policy name typo in attribute** → Mitigated by `AuthPolicies` constants. A typo would be a compile error.
- **Removing cookie 401/403 event handlers** → The `JsonAuthorizationResultHandler` must cover the same cases. Verify with existing E2E tests.
