## Why

The SCHUFA credit check currently lives in the CustomerManagement bounded context (`ICreditReportProvider`, `MockSchufaProvider`, `CreditReport` as Value Object on `Customer`). This is a bounded context violation: credit worthiness assessment is a risk evaluation concern, not a customer master data concern. Additionally, the current design stores one `CreditReport` per customer, but in the real credit industry each application requires its own up-to-date credit check (§18 KWG, MaRisk). Two applications for the same customer must result in two independent SCHUFA checks with potentially different scores.

## What Changes

- **Move `ICreditReportProvider` and `MockSchufaProvider`** from CustomerManagement to RiskManagement context
- **Create `CreditReport` Value Object** in RiskManagement domain (separate from the CustomerManagement one)
- **Application aggregate owns its CreditReport**: Each `Application` stores its own credit check result instead of copying values from the customer profile
- **SCHUFA check triggered during application creation**: RiskManagement fetches customer master data (name, DOB, address) via existing HTTP ACL, then runs its own credit check
- **Remove `CreditReport` from `Customer` aggregate** and the `RequestCreditReportCommand` from CustomerManagement
- **Remove `CreditReportReceivedEvent`** from CustomerManagement domain events
- **BREAKING**: The `GET /api/internal/customers/{id}` response no longer includes `CreditReport` data
- **BREAKING**: The `POST /api/customers/{id}/credit-report` endpoint is removed
- **BREAKING**: Frontend no longer needs a separate "Request Credit Report" step before creating an application

## Capabilities

### New Capabilities
- `application-credit-check`: SCHUFA credit check as part of the RiskManagement context, triggered per application. Includes `ICreditReportProvider` domain service, `CreditReport` value object on `Application`, and automatic check during application creation/submission.

### Modified Capabilities
- `scoring-config`: The scoring flow changes because `HasPaymentDefault` and `CreditScore` are no longer read from the customer profile but from the application's own `CreditReport`. The `Application.Create` signature changes to accept an `ICreditReportProvider` instead of pre-fetched credit data.

## Impact

- **Backend (RiskManagement)**: New domain service, value object, updated `Application` aggregate, updated command handlers (`CreateApplicationCommand`, `CreateAndSubmitApplicationCommand`)
- **Backend (CustomerManagement)**: Remove `ICreditReportProvider`, `MockSchufaProvider`, `CreditReport` value object, `RequestCreditReportCommand`, `CreditReportReceivedEvent`, credit report fields from `Customer` aggregate
- **Infrastructure (CustomerManagement)**: Remove `MockSchufaProvider` registration from DI
- **Infrastructure (RiskManagement)**: Add `MockSchufaProvider` registration, extend `ICustomerProfileService` to include address/DOB
- **API**: Remove `POST /api/customers/{id}/credit-report`, update internal customer endpoint response
- **Frontend**: Remove credit report request UI/flow, simplify application creation (no prerequisite credit check)
- **Database**: Migration to add `CreditReport` columns to `Application` table, migration to remove `CreditReport` columns from `Customer` table
- **Tests**: Move and adapt `MockSchufaProviderTests` and `RequestCreditReportHandlerTests` to RiskManagement
