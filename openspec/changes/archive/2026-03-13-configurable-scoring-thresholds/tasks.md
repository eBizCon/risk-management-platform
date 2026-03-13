## 1. Keycloak & Authorization Setup

- [x] 1.1 Add `risk_manager` role and test user to Keycloak realm import (`dev/keycloak/import/risk-management-realm.json`)
- [x] 1.2 Add `AppRoles.RiskManager` constant and `AuthPolicies.RiskManager` policy in backend (`Enums.cs`, `AuthenticationExtensions.cs`)
- [x] 1.3 Extend `RoleClaimHelper.AllowedRoles` to include `"risk_manager"`
- [x] 1.4 Add `ClaimsPrincipalExtensions.IsRiskManager()` helper method

## 2. Domain: ScoringConfigVersion Entity

- [x] 2.1 Create `ScoringConfig` value object containing all 18 scoring parameters with validation logic (threshold ordering, range constraints, non-negative penalties)
- [x] 2.2 Create `ScoringConfigVersion` entity with Id, Version, ScoringConfig (value object), CreatedBy, CreatedAt
- [x] 2.3 Create `IScoringConfigRepository` interface with `GetCurrentAsync()`, `SaveAsync()`, `GetByIdAsync()`

## 3. Domain: Refactor ScoringService to Config-Parametrized

- [x] 3.1 Extend `IScoringService.CalculateScore()` signature with `ScoringConfig` parameter
- [x] 3.2 Refactor `ScoringService` to use config parameters instead of hardcoded values
- [x] 3.3 Add nullable `ScoringConfigVersionId` property to `Application` entity
- [x] 3.4 Update `Application.ApplyScoring()` to accept and store config version ID
- [x] 3.5 Update all `Application` methods that call `ApplyScoring()` (Create, Submit, UpdateDetails) to pass config

## 4. Infrastructure: Persistence & Migration

- [x] 4.1 Create EF `ScoringConfigVersionConfiguration` for `scoring_config_versions` table
- [x] 4.2 Update `ApplicationConfiguration` with `ScoringConfigVersionId` nullable FK column
- [x] 4.3 Register `DbSet<ScoringConfigVersion>` in `ApplicationDbContext`
- [x] 4.4 Create `ScoringConfigRepository` implementation
- [x] 4.5 Create EF migration for new table and FK
- [x] 4.6 Extend `DatabaseSeeder` to seed default config v1 with current hardcoded values

## 5. Application Layer: Commands & Queries

- [x] 5.1 Create `GetScoringConfigQuery` and handler (returns current config version)
- [x] 5.2 Create `UpdateScoringConfigCommand` and handler (validates, creates new version)
- [x] 5.3 Create `RescoreOpenApplicationsCommand` and handler (loads open applications, rescores with current config, saves)
- [x] 5.4 Update all existing handlers that call `IScoringService` to load current config and pass it (CreateApplicationHandler, UpdateApplicationHandler, SubmitApplicationHandler, CreateAndSubmitApplicationHandler, UpdateAndSubmitApplicationHandler)

## 6. API: ScoringConfig Controller

- [x] 6.1 Create `ScoringConfigController` with `[Authorize(Policy = AuthPolicies.RiskManager)]`
- [x] 6.2 Implement `GET /api/scoring-config` endpoint
- [x] 6.3 Implement `PUT /api/scoring-config` endpoint
- [x] 6.4 Implement `POST /api/scoring-config/rescore` endpoint

## 7. Frontend: Authorization & Navigation

- [x] 7.1 Extend `UserRole` type with `'risk_manager'` in `types.ts`
- [x] 7.2 Add `isRiskManager` derived in `+layout.svelte` and nav link "Scoring-Konfiguration" with Settings icon
- [x] 7.3 Add Risk Manager nav item in `MobileMenu.svelte`
- [x] 7.4 Add role label "Risikomanager" in layout and mobile menu user info

## 8. Frontend: Scoring Config Page

- [x] 8.1 Create `/risk-manager/scoring-config/+page.ts` load function (fetch current config)
- [x] 8.2 Create `/risk-manager/scoring-config/+page.svelte` with RoleGuard, config form (all 18 parameters grouped by category), version info display
- [x] 8.3 Implement save action (PUT to API, show success/validation errors inline)
- [x] 8.4 Implement rescore action ("Offene Anträge neu bewerten" button with confirmation and result count display)

## 9. Tests

- [x] 9.1 Update existing `ScoringServiceTests` to pass config parameter (verify default config produces identical results)
- [x] 9.2 Add tests for `ScoringService` with custom config values (verify parameter changes affect scoring)
- [x] 9.3 Add tests for `ScoringConfig` validation (valid config, invalid threshold ordering, invalid ratio ordering, negative penalties)
- [x] 9.4 Add tests for `UpdateScoringConfigCommand` handler (happy path, validation failure)
- [x] 9.5 Add tests for `RescoreOpenApplicationsCommand` handler (rescores only open apps, updates config version ID)
- [x] 9.6 Add E2E test: Risk Manager login, view config page, update config, verify save
