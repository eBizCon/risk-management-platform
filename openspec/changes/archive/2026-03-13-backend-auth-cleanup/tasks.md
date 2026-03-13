## 1. Role Constants & Typed Extensions

- [x] 1.1 Add `AppRoles` static class to `Models/Enums.cs` with `const string Applicant = "applicant"`, `Processor = "processor"`, and `string[] All`
- [x] 1.2 Add `AuthPolicies` static class to `Models/Enums.cs` with `const string Applicant`, `Processor`, `ApplicantOrProcessor` (using `nameof()`)
- [x] 1.3 Update `ClaimsPrincipalExtensions.cs`: add `IsApplicant()` and `IsProcessor()` methods using `AppRoles` constants
- [x] 1.4 Update `RoleClaimHelper.cs`: replace local `AllowedRoles` array with `AppRoles.All`

## 2. OidcOptions & Authentication Extension

- [x] 2.1 Create `Extensions/OidcOptions.cs` with properties for Issuer, ClientId, ClientSecret, Scope, RolesClaimPath, PostLogoutRedirectUri, CallbackPath
- [x] 2.2 Create `Extensions/AuthenticationExtensions.cs` with `AddOidcAuthentication(this IServiceCollection, IConfiguration, IWebHostEnvironment)` method that encapsulates cookie config, OIDC config, policy registration, and result handler registration
- [x] 2.3 Create `Extensions/JsonAuthorizationResultHandler.cs` implementing `IAuthorizationMiddlewareResultHandler` — return JSON 401/403 for `/api/*` routes, delegate to default handler otherwise
- [x] 2.4 Refactor `Program.cs`: replace inline auth configuration (~140 lines) with `builder.Services.AddOidcAuthentication(builder.Configuration, builder.Environment)`

## 3. Policy-Based Authorization on Controllers

- [x] 3.1 Refactor `ApplicationsController`: set `[Authorize(Policy = AuthPolicies.Applicant)]` on class, override `GetApplication` with `AuthPolicies.ApplicantOrProcessor`, remove all manual role checks
- [x] 3.2 Refactor `ProcessorController`: set `[Authorize(Policy = AuthPolicies.Processor)]` on class, remove all manual role checks
- [x] 3.3 Refactor `InquiryController`: set `[Authorize]` on class, add per-method policies (`ApplicantOrProcessor` for GetInquiries, `Processor` for CreateInquiry, `Applicant` for RespondToInquiry/AnswerInquiry), remove all manual role checks
- [x] 3.4 Refactor `AuthController`: keep `GetCurrentUser` and `GetDashboardStats` without policy (they handle unauthenticated gracefully), use `AppRoles` constants for role string comparisons
- [x] 3.5 Update `TestSessionController`: use `AppRoles` constants for role validation and default emails

## 4. Tests & Verification

- [x] 4.1 Update `RoleClaimHelperTests`: use `AppRoles.All` instead of local array
- [x] 4.2 Verify `dotnet build` succeeds with zero errors
- [x] 4.3 Verify `dotnet test` — all existing tests pass
