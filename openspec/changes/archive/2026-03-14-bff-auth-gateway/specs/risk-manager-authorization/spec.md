## MODIFIED Requirements

### Requirement: Backend authorization policy for risk_manager
The backend SHALL define an `AppRoles.RiskManager` constant with value `"risk_manager"` and an `AuthPolicies.RiskManager` authorization policy requiring this role. The `InternalAuthMiddleware` SHALL map the `X-User-Role` header value `"risk_manager"` to a `ClaimTypes.Role` claim, which the authorization policy evaluates.

#### Scenario: Risk Manager role is recognized from internal headers
- **WHEN** a request arrives with `X-User-Role: risk_manager` header (set by the BFF)
- **THEN** the backend SHALL map the role to a `ClaimTypes.Role` claim via `InternalAuthMiddleware`

#### Scenario: Risk Manager can access protected endpoints
- **WHEN** an authenticated Risk Manager calls an endpoint requiring `AuthPolicies.RiskManager`
- **THEN** the request SHALL be authorized

#### Scenario: Non-Risk-Manager is rejected from Risk Manager endpoints
- **WHEN** a user without `risk_manager` role calls an endpoint requiring `AuthPolicies.RiskManager`
- **THEN** the request SHALL return HTTP 403
