## ADDED Requirements

### Requirement: risk_manager role in Keycloak
The Keycloak realm import SHALL include a `risk_manager` realm role. A test user `riskmanager` (email: `riskmanager@example.com`, password: `riskmanager`) SHALL be seeded with the `risk_manager` role.

#### Scenario: Risk Manager user can log in
- **WHEN** a user logs in with credentials `riskmanager` / `riskmanager`
- **THEN** the OIDC token SHALL contain the `risk_manager` role

#### Scenario: Keycloak realm import includes new role
- **WHEN** the Keycloak realm is imported from `dev/keycloak/import/risk-management-realm.json`
- **THEN** the realm SHALL contain the roles `applicant`, `processor`, and `risk_manager`

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

### Requirement: Frontend role support for risk_manager
The frontend `UserRole` type SHALL include `'risk_manager'`. The layout SHALL show a navigation item "Scoring-Konfiguration" linking to `/risk-manager/scoring-config` when the user has the `risk_manager` role. The mobile menu SHALL also include this navigation item for Risk Managers.

#### Scenario: Risk Manager sees dedicated navigation
- **WHEN** a Risk Manager is logged in
- **THEN** the navigation SHALL display "Scoring-Konfiguration" link
- **THEN** the link SHALL point to `/risk-manager/scoring-config`

#### Scenario: Non-Risk-Manager does not see config navigation
- **WHEN** an applicant or processor is logged in
- **THEN** the navigation SHALL NOT display "Scoring-Konfiguration"

### Requirement: Risk Manager role label in UI
The layout SHALL display the role label "Risikomanager" for users with the `risk_manager` role, following the existing pattern where `applicant` shows "Antragsteller" and `processor` shows "Antragsbearbeiter".

#### Scenario: Role label displayed correctly
- **WHEN** a Risk Manager is logged in
- **THEN** the user info area SHALL display "Risikomanager" as the role label
