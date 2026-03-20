## Context

The Application aggregate currently owns `EmploymentStatus` (Enumeration VO) and `HasPaymentDefault` (bool) — both are properties of the customer, not of a credit application. The Customer aggregate (`CustomerManagement.Domain`) has only personal data (name, address, phone, DOB, status). Two bounded contexts exist: `CustomerManagement` (separate solution, `customer` DB schema) and `RiskManagement` (`public` schema). They communicate via synchronous HTTP with API key auth on `/api/internal/*` endpoints.

Current scoring flow: `Application.Create()` receives `EmploymentStatus` and `HasPaymentDefault` as direct parameters → `ScoringService.CalculateScore()` uses them → result stored on Application.

## Goals / Non-Goals

**Goals:**
- Move `EmploymentStatus` to the Customer aggregate as a managed profile attribute
- Replace `HasPaymentDefault` with a `CreditReport` Value Object on Customer, populated via a domain service (ICreditReportProvider) with a mock SCHUFA implementation
- Provide an explicit "Request Credit Check" action for the Sachbearbeiter
- Store a denormalized scoring snapshot on Application for audit trail
- Demonstrate DDD patterns: Anti-Corruption Layer, Ports & Adapters, Domain Services, Value Objects

**Non-Goals:**
- Real SCHUFA/Creditreform API integration (mock is sufficient)
- Credit report history (only the latest report is stored on Customer)
- Automated/scheduled credit checks (manual trigger only)
- Changing the scoring algorithm itself (only the data source changes)

## Decisions

### D1: EmploymentStatus as Customer property

**Decision**: Add `EmploymentStatus` as a property on the Customer aggregate. The `EmploymentStatus` Enumeration VO stays in SharedKernel (both contexts need it). Customer create/update flows accept employment status. The field is required.

**Rationale**: Employment status describes the person, not the application. Multiple applications from the same customer should share a single source of truth for this attribute. Moving it to Customer eliminates potential inconsistencies.

**Alternatives considered**:
- Keep on Application, read-only from Customer → still duplicates ownership, muddy domain model
- Optional on Customer → employment status is always relevant for credit decisions, should be required

### D2: CreditReport as Value Object on Customer

**Decision**: Introduce `CreditReport` as an immutable Value Object on Customer with fields:

| Field | Type | Description |
|---|---|---|
| HasPaymentDefault | bool | Whether the customer has payment defaults |
| CreditScore | int? | External credit score (e.g., 100-600 SCHUFA range) |
| CheckedAt | DateTime | When the check was performed |
| Provider | string | Source identifier (e.g., "schufa_mock") |

The property is **nullable** on Customer — a customer without a credit check has `CreditReport = null`.

**Rationale**: A Value Object captures the full result of a credit check as a cohesive concept. `CheckedAt` and `Provider` provide audit trail. Nullable because a newly created customer hasn't been checked yet.

**Alternatives considered**:
- Simple `bool HasPaymentDefault` on Customer → loses audit context (when? by whom?), not realistic for a DDD demo
- Separate `CreditReport` entity with history → overengineering for current scope, can be added later

### D3: ICreditReportProvider as Domain Port (Anti-Corruption Layer)

**Decision**: Define `ICreditReportProvider` interface in `CustomerManagement.Domain` as a Port. The `MockSchufaProvider` implementation lives in `CustomerManagement.Infrastructure`.

```
Domain (Port):
  ICreditReportProvider.CheckAsync(string firstName, string lastName, 
    DateOnly dateOfBirth, Address address) → CreditReport

Infrastructure (Adapter):
  MockSchufaProvider : ICreditReportProvider
```

**Mock strategy**: Deterministic results based on customer data:
- LastName contains "Verzug" or "Default" → `HasPaymentDefault = true`, CreditScore = 250
- Age > 65 (retired typical) → CreditScore = 520 (very good)
- Age < 25 → CreditScore = 350 (limited history)
- Default case → `HasPaymentDefault = false`, CreditScore = 420

**Rationale**: Demonstrates the Ports & Adapters pattern. The domain defines what it needs (the Port), infrastructure provides the implementation (the Adapter). The mock provides predictable demo behavior. A real SCHUFA adapter could be swapped in without touching the domain.

**Alternatives considered**:
- No interface, just direct mock → misses the DDD teaching point of ACL/Ports
- Random results → unpredictable demo behavior, harder to test

### D4: Explicit "Request Credit Check" command

**Decision**: New `RequestCreditReportCommand(CustomerId)` triggers the credit check flow:

1. Handler loads Customer
2. Calls `ICreditReportProvider.CheckAsync(...)` with customer data
3. Calls `Customer.UpdateCreditReport(creditReport)`
4. Saves → publishes `CreditReportReceivedEvent`

The Sachbearbeiter triggers this via a "Bonität prüfen" button on the Customer detail page.

**Rationale**: Credit checks are an explicit business action with cost implications in the real world. They should not happen implicitly (e.g., on customer creation). An explicit command makes this visible in the domain.

### D5: Scoring snapshot on Application

**Decision**: When an Application is created or scored, store snapshot values from the Customer:

| Field | Type | Purpose |
|---|---|---|
| ScoringEmploymentStatus | string | Employment status at scoring time |
| ScoringHasPaymentDefault | bool | Payment default flag at scoring time |
| ScoringCreditScore | int? | Credit score at scoring time |

These are **read-only audit fields** — they document what data was used for scoring. The Application no longer exposes `EmploymentStatus` or `HasPaymentDefault` as editable fields.

**Rationale**: Scores must be reproducible. If a customer's employment status changes after scoring, the historical score should still be explainable. Denormalized snapshots solve this without complex event sourcing.

**Alternatives considered**:
- No snapshot, just reference ScoringReasons JSON → implicit, not queryable
- Full CreditReport snapshot as VO on Application → too heavy, the essential fields suffice

### D6: Extended internal Customer API

**Decision**: Extend the existing `GET /api/internal/customers/{id}` response to include `employmentStatus` and `creditReport` (nullable object with `hasPaymentDefault`, `creditScore`, `checkedAt`, `provider`).

The Application Service reads this extended response when creating/scoring applications. If the customer has no credit report (`creditReport = null`), the application cannot be created — a credit check must be performed first.

**Rationale**: Reuses the existing service-to-service communication pattern. No new endpoint needed, just an extended response.

### D7: Credit check required before application creation

**Decision**: An application cannot be created for a customer who has no CreditReport. The Application Service checks for `creditReport != null` in the customer validation step and returns a clear error if missing.

**Rationale**: In reality, you would never issue a credit decision without a credit check. This enforces realistic business rules and gives the credit check action a clear purpose in the workflow.

## Risks / Trade-offs

- **Breaking change for existing applications**: Existing Application records have `EmploymentStatus` and `HasPaymentDefault` directly. → Mitigation: Migrate existing values to snapshot fields. Since this is a demo project (not production), a database reset is also acceptable.
- **Coupling between Application creation and Customer state**: Application creation now depends on Customer having a CreditReport. → Mitigation: Clear error message guides the user to perform a credit check first. This is intentional domain behavior, not accidental coupling.
- **Mock provider leaking into domain tests**: Tests that involve credit checks need to be aware of mock behavior. → Mitigation: Use interface mocking in unit tests; mock strategy only matters for integration/E2E tests.
- **EmploymentStatus in SharedKernel**: Moving the VO to SharedKernel creates a dependency both contexts share. → Mitigation: EmploymentStatus is a stable, well-defined enumeration. Low change risk.
