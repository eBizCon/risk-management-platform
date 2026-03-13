## Why

The ScoringService contains ~14 hardcoded threshold values (TrafficLight boundaries, income ratio tiers, rate affordability tiers, employment penalties, payment default penalty). Any change to scoring parameters requires a code deployment. A Risk Manager role should be able to adjust these values via a UI without developer involvement — enabling "Business Configuration" as a first-class capability.

## What Changes

- New `risk_manager` role added to Keycloak and backend authorization
- New `ScoringConfigVersion` entity storing all scoring parameters with full version history
- `ScoringService` refactored from hardcoded values to config-parametrized calculation
- `Application` entity extended with `ScoringConfigVersionId` for audit trail (which config was used for each scoring)
- New Admin UI at `/risk-manager/scoring-config` for viewing, editing, and versioning scoring parameters
- Explicit "Rescore open applications" action that re-evaluates only open applications (submitted, resubmitted, needs_information) with the new config
- Default config v1 seeded with current hardcoded values — existing applications remain unaffected unless explicitly rescored
- Keycloak realm import extended with `risk_manager` role and test user

## Capabilities

### New Capabilities
- `scoring-config`: Scoring configuration management — CRUD for scoring parameters, config versioning, audit trail, and batch rescoring of open applications
- `risk-manager-authorization`: Authorization for the Risk Manager role — Keycloak role, backend policy, frontend route guard and navigation

### Modified Capabilities
<!-- No existing spec-level behavior changes. The scoring algorithm stays the same, it just reads parameters from config instead of hardcoded values. -->

## Impact

- **Backend**: New aggregate/entity (`ScoringConfigVersion`), new repository, new DB migration (scoring_config_versions table + FK on applications), new controller + commands/queries, modified `IScoringService` signature, modified `Application.ApplyScoring()`, modified all handlers that call scoring
- **Frontend**: New route `/risk-manager/scoring-config`, extended navigation (layout + mobile menu), extended `UserRole` type, new `isRiskManager` derived
- **Infrastructure**: Keycloak realm import (`risk_manager` role + test user), DB seed (default config v1)
- **Tests**: Existing `ScoringServiceTests` need update (config parameter), new tests for config CRUD, rescoring, authorization
