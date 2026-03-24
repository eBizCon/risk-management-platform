## Context

The SCHUFA credit check currently lives in CustomerManagement: `ICreditReportProvider` (domain service), `MockSchufaProvider` (infrastructure), `CreditReport` (value object on `Customer` aggregate). When creating a credit application, RiskManagement fetches the customer profile via HTTP (`ICustomerProfileService` → `GET /api/internal/customers/{id}`) and copies `HasPaymentDefault` + `CreditScore` from the customer's `CreditReport` into the `Application` aggregate. If no credit report exists, the application cannot be created.

This design violates bounded context boundaries: credit worthiness assessment belongs to risk evaluation, not customer master data. Additionally, one `CreditReport` per customer is fachlich incorrect — each application needs its own up-to-date check.

## Goals / Non-Goals

**Goals:**
- Move SCHUFA credit check responsibility from CustomerManagement to RiskManagement
- Each `Application` owns its own `CreditReport` (1:1 relationship, not shared across applications)
- RiskManagement performs the credit check during application creation using customer master data fetched via existing HTTP ACL
- Remove all credit-report-related code from CustomerManagement
- Update the internal customer API to no longer expose credit report data

**Non-Goals:**
- Introducing async messaging (RabbitMQ/MassTransit) — separate change
- Replacing the `MockSchufaProvider` with a real SCHUFA integration
- Changing the scoring algorithm itself
- Modifying the `Customer` database table migration history (new migration only)

## Decisions

### Decision 1: CreditReport as Value Object on Application aggregate

The `Application` aggregate gets a `CreditReport` property (value object) instead of separate `HasPaymentDefault`/`CreditScore` fields. This bundles credit check metadata (provider, checkedAt) with the result.

**Alternative considered:** Keep flat fields `HasPaymentDefault`, `CreditScore` on Application and add `CreditCheckedAt`, `CreditProvider`. Rejected because a value object is more expressive and consistent with DDD patterns used elsewhere.

### Decision 2: ICreditCheckService as RiskManagement domain service

A new `ICreditCheckService` interface in `RiskManagement.Domain.Services` replaces `ICreditReportProvider` from CustomerManagement. The interface accepts customer master data (firstName, lastName, dateOfBirth, address) and returns a `CreditCheckResult` value object.

**Alternative considered:** Reuse `ICreditReportProvider` from SharedKernel. Rejected because credit checking is domain-specific to RiskManagement, not a shared concern.

### Decision 3: Extend CustomerProfile ACL with address and date of birth

`ICustomerProfileService.CustomerProfile` needs `DateOfBirth` and `Address` fields so RiskManagement can pass them to the credit check service. The internal API endpoint `GET /api/internal/customers/{id}` is extended to include these fields. The `CreditReport` field is removed from the internal response.

**Alternative considered:** Separate endpoint for credit-check-relevant data. Rejected — unnecessary complexity, the existing endpoint just needs field adjustments.

### Decision 4: Credit check during Application.Create, not on Submit

The credit check runs synchronously during `Application.Create` (and `CreateAndSubmit`). This keeps the current UX where the applicant sees the score immediately. The `Application.Submit` method no longer needs to re-check credit.

**Alternative considered:** Lazy credit check on Submit only. Rejected because the score is needed for the draft view and the current UX expects immediate feedback.

### Decision 5: MockSchufaProvider moves to RiskManagement.Infrastructure

The `MockSchufaProvider` implementation moves to `RiskManagement.Infrastructure.ExternalServices`, implementing the new `ICreditCheckService` interface. The mock logic stays unchanged.

### Decision 6: Address as shared value object or local record

RiskManagement needs an `Address` type for the credit check. Rather than referencing CustomerManagement's `Address` value object, create a simple `CreditCheckRequest` record in the Application layer that contains the needed fields (street, city, zipCode, country) as strings. This avoids coupling between bounded contexts.

**Alternative considered:** Move `Address` to SharedKernel. Rejected because Address semantics may differ between contexts. RiskManagement only needs address strings for the credit check, not the full validation logic.

## Risks / Trade-offs

- **Breaking change for frontend**: The credit report request flow is removed. Frontend must be updated to no longer require a pre-existing credit report. → Mitigation: Frontend changes are part of this change scope.
- **Synchronous credit check adds latency to application creation**: The SCHUFA call (even mocked) adds time to the create flow. → Mitigation: Mock is instant; real integration would need async pattern (separate change).
- **Two HTTP calls during application creation** (customer profile + credit check): Currently only customer profile is fetched. → Mitigation: Credit check is local (no HTTP), only customer data fetch is remote.
- **CustomerManagement DB migration removes columns**: `CreditReport` fields are dropped from Customer table. → Mitigation: New migration only, no history rewrite. Data loss is acceptable since mock data has no production value.
- **Existing tests reference credit report on Customer**: Tests in `RiskManagement.Api.Tests` (`MockSchufaProviderTests`, `RequestCreditReportHandlerTests`) must be moved/rewritten. → Mitigation: Part of task scope.
