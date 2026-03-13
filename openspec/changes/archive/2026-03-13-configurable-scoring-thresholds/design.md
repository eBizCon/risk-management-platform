## Context

The `ScoringService` in `RiskManagement.Domain.Services` calculates a risk score (0–100) and maps it to a TrafficLight (Green/Yellow/Red). All ~14 threshold values and penalty amounts are hardcoded. The system has two roles (`applicant`, `processor`) — no configuration role exists. The `Application` aggregate calls `ApplyScoring()` during create, update, submit, and resubmit, delegating to `IScoringService.CalculateScore()`.

The scoring parameters need to be externalized to a database-backed configuration that a Risk Manager can edit via UI, with full version history for audit compliance.

## Goals / Non-Goals

**Goals:**
- All scoring thresholds and penalties configurable via Admin UI without code deployment
- New `risk_manager` role with dedicated authorization
- Config versioning: every change creates a new version, old versions are immutable
- Audit trail: each scored Application records which config version was used
- Explicit batch rescoring of open applications after config change
- Default config v1 seeded with current hardcoded values
- Validation constraints on config parameters

**Non-Goals:**
- A/B testing or multiple active configs simultaneously
- Automatic rescoring on config change (always explicit)
- Rescoring of already-decided applications (approved/rejected)
- Config import/export (CSV, JSON)
- Config change approval workflow (Risk Manager acts autonomously)
- Scoring algorithm changes (only parameter tuning)

## Decisions

### 1. ScoringConfig as Domain Entity with Versioned Rows

**Decision**: `ScoringConfigVersion` is a Domain Entity stored in a `scoring_config_versions` table. Each save creates a new row (new version). No updates to existing rows.

**Alternatives considered**:
- *AppSettings/JSON file*: No UI, requires deployment — rejected
- *Single row with update*: Loses history — rejected
- *Separate Aggregate with event sourcing*: Over-engineered for ~14 scalar values — rejected

**Rationale**: Immutable versions give natural audit trail. Simple queries (`ORDER BY version DESC LIMIT 1`) for current config. FK from Application to version provides point-in-time traceability.

### 2. Config Passed as Parameter to ScoringService

**Decision**: `IScoringService.CalculateScore()` receives a `ScoringConfig` value object as additional parameter. The handler loads the current config and passes it in.

**Alternatives considered**:
- *Inject config via constructor (DI)*: Makes ScoringService stateful per-request, complicates testing — rejected
- *ScoringService loads config itself via repository*: Domain service with infrastructure dependency, violates Clean Architecture — rejected

**Rationale**: Keeps `ScoringService` as a pure domain service with no dependencies. Easy to test (pass config directly). Handler owns the orchestration (load config → call service → save).

### 3. New `risk_manager` Role

**Decision**: Dedicated `risk_manager` role in Keycloak, not reusing `processor`.

**Alternatives considered**:
- *Extend processor role*: Mixes operational and configuration responsibilities — rejected
- *Generic admin role*: Too vague, doesn't match domain language — rejected

**Rationale**: In banking, scoring parameters are configured by Risk Management (Kreditrisikosteuerung), not by loan processors. Clean separation of concerns. Role name matches the domain.

### 4. Selective Rescoring (Open Applications Only)

**Decision**: Rescore command targets only applications with status `submitted`, `resubmitted`, or `needs_information`. Approved/rejected applications keep their original scoring.

**Alternatives considered**:
- *Rescore everything*: Alters historical decisions — rejected for compliance reasons
- *Per-application rescore*: More UI complexity, low value — rejected

**Rationale**: A decided application was correctly scored at decision time. Only pending applications benefit from updated parameters. Risk Manager triggers rescoring explicitly via button after config change.

### 5. Application.ScoringConfigVersionId as Nullable FK

**Decision**: Add nullable `ScoringConfigVersionId` column to `applications` table. Null means "scored with hardcoded legacy values" (pre-migration applications).

**Alternatives considered**:
- *Non-nullable with backfill*: Would require creating a synthetic v0 for all existing rows — more migration complexity
- *Store config snapshot as JSON on application*: Duplicates data, harder to query — rejected

**Rationale**: Nullable FK is the simplest migration path. Null has clear semantic meaning. New scorings always set the version ID.

### 6. Navigation Structure

**Decision**: New top-level route `/risk-manager/scoring-config` visible only to `risk_manager` role. Separate nav section in layout (alongside applicant/processor sections).

**Rationale**: Follows existing pattern where each role sees its own nav items.

## Risks / Trade-offs

- **[Risk] Batch rescoring performance with many open applications** → Mitigation: Process in-memory loop (not individual DB calls per app). For the expected scale (< 1000 open applications), this is fine. If scale grows, convert to background job.
- **[Risk] Config validation edge cases (e.g., all penalties = 0)** → Mitigation: Allow it — technically valid. The system computes scores correctly even with all zeros. Business validity is the Risk Manager's responsibility.
- **[Risk] Existing applications with NULL ScoringConfigVersionId** → Mitigation: UI displays "Legacy-Bewertung" for null. No functional impact.
- **[Risk] Keycloak realm re-import needed for existing dev environments** → Mitigation: Document in README. Keycloak import is idempotent for new roles/users.
- **[Trade-off] Config is global (not per-product/per-segment)** → Accepted for current scope. Single config covers all applications. Multi-config can be added later if needed.
