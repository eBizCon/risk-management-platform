## Why

The Application aggregate currently owns `EmploymentStatus` and `HasPaymentDefault` — properties that describe the **customer**, not the credit application. This leads to inconsistencies when the same customer files multiple applications (each could have different employment status or payment default flags). In a realistic credit domain, employment status is a personal attribute managed on the customer profile, and payment default history comes from an external credit reporting service (e.g., SCHUFA). As a DDD demo project, the domain model should reflect these real-world responsibilities correctly: customer profile data belongs to the Customer aggregate, and credit checks are modeled as a domain service with an Anti-Corruption Layer.

## What Changes

- **Move `EmploymentStatus`** from `Application` to `Customer` aggregate. The customer's employment status is maintained as part of the customer profile and read by the Application at scoring time.
- **Replace `HasPaymentDefault`** (bool on Application) with a `CreditReport` Value Object on the Customer aggregate, containing `HasPaymentDefault`, `Score` (optional external credit score), `CheckedAt`, and `Provider`.
- **New `ICreditReportProvider`** domain interface (Port) in `CustomerManagement.Domain` with a `MockSchufaProvider` infrastructure implementation that returns deterministic results based on customer data.
- **Explicit "Request Credit Check" action** on the Customer — a Sachbearbeiter triggers a credit check, which calls the provider and stores the result as `CreditReport` on the Customer.
- **Scoring snapshot on Application**: When an application is created/scored, the `EmploymentStatus` and `CreditReport` values are read from the Customer and stored as a denormalized snapshot on the Application for audit trail. The Application no longer accepts these as direct input.
- **Scoring adjustment**: `ScoringService.CalculateScore()` receives `EmploymentStatus` and `HasPaymentDefault` from the Customer's current data (or the snapshot). The scoring algorithm itself remains unchanged.
- **Frontend adjustment**: Application form no longer has employment status or payment default fields. These are managed on the Customer detail page. A "Bonität prüfen" button triggers the credit check.

## Capabilities

### New Capabilities
- `credit-report`: CreditReport Value Object, ICreditReportProvider domain interface (Port), MockSchufaProvider implementation (Anti-Corruption Layer), "Request Credit Check" command on Customer, CreditReportReceivedEvent domain event, and credit check UI action on Customer detail page.
- `customer-employment-status`: EmploymentStatus property on Customer aggregate (moved from Application), including Customer create/update flows, validation, and UI for managing employment status on the customer profile.

### Modified Capabilities
- `application-customer-link`: Application no longer accepts EmploymentStatus and HasPaymentDefault as direct input. Instead reads these from the linked Customer at scoring time and stores a denormalized snapshot for audit trail. Application create/update DTOs, validators, form, and scoring flow change accordingly.

## Impact

- **Backend (CustomerManagement)**: New `CreditReport` Value Object, `ICreditReportProvider` interface in Domain, `MockSchufaProvider` in Infrastructure. New `EmploymentStatus` property on Customer aggregate. New command `RequestCreditReportCommand`. New domain event `CreditReportReceivedEvent`. EF Core mapping for new fields. Database migration for `employment_status`, `credit_report_*` columns on `customers` table.
- **Backend (RiskManagement)**: Remove `EmploymentStatus` and `HasPaymentDefault` properties from Application aggregate. Add snapshot fields (`ScoringEmploymentStatus`, `ScoringHasPaymentDefault`). Update `Application.Create()`, `UpdateDetails()`, and scoring flow to read from Customer data (via service-to-service call). Update DTOs, validators, mappers.
- **Frontend**: Remove employment status and payment default fields from ApplicationForm. Add employment status field to Customer create/edit forms. Add "Bonität prüfen" button and CreditReport display on Customer detail page. Update Application detail view to show snapshot values.
- **Service-to-service**: Application Service needs to fetch Customer profile (including EmploymentStatus + CreditReport) when creating/scoring an application. Extend existing internal Customer API.
