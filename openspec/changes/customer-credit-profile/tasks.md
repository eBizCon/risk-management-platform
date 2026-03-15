## 1. SharedKernel: Move EmploymentStatus

- [x] 1.1 Move `EmploymentStatus` Enumeration VO from `RiskManagement.Domain/ValueObjects/` to `SharedKernel/ValueObjects/`
- [x] 1.2 Update namespace references in `RiskManagement.Domain`, `RiskManagement.Application`, and `RiskManagement.Infrastructure` to use SharedKernel EmploymentStatus
- [x] 1.3 Verify RiskManagement solution builds with SharedKernel EmploymentStatus

## 2. CustomerManagement Domain: EmploymentStatus on Customer

- [x] 2.1 Add `EmploymentStatus` property (required) to Customer aggregate with guard in `Create()` and `Update()` methods
- [x] 2.2 Update `Customer.Create()` factory to accept `EmploymentStatus` parameter
- [x] 2.3 Update `Customer.Update()` method to accept `EmploymentStatus` parameter
- [x] 2.4 Add EF Core mapping for `employment_status` column (varchar, required) on `customer.customers` table in `CustomerEntityTypeConfiguration`

## 3. CustomerManagement Domain: CreditReport Value Object

- [x] 3.1 Create `CreditReport` Value Object in `CustomerManagement.Domain/ValueObjects/` with fields: `HasPaymentDefault` (bool), `CreditScore` (int?, range 100-600), `CheckedAt` (DateTime), `Provider` (string). Add validation in factory method.
- [x] 3.2 Add nullable `CreditReport` property to Customer aggregate
- [x] 3.3 Add `UpdateCreditReport(CreditReport)` method on Customer aggregate with active status guard
- [x] 3.4 Create `CreditReportReceivedEvent(CustomerId, bool HasPaymentDefault, int? CreditScore)` domain event
- [x] 3.5 Add EF Core mapping for `credit_report_*` columns (all nullable) on `customer.customers` table as owned entity

## 4. CustomerManagement Domain: ICreditReportProvider Port

- [x] 4.1 Create `ICreditReportProvider` interface in `CustomerManagement.Domain/Services/` with `CheckAsync(string firstName, string lastName, DateOnly dateOfBirth, Address address) → Task<CreditReport>`

## 5. CustomerManagement Infrastructure: MockSchufaProvider

- [x] 5.1 Create `MockSchufaProvider` implementation in `CustomerManagement.Infrastructure/ExternalServices/` with deterministic logic: name contains "Verzug"/"Default" → HasPaymentDefault=true, Score=250; age>65 → Score=520; age<25 → Score=350; default → HasPaymentDefault=false, Score=420
- [x] 5.2 Register `MockSchufaProvider` as `ICreditReportProvider` in DI container (`DependencyInjection.cs`)

## 6. CustomerManagement Application: Request Credit Check Command

- [x] 6.1 Create `RequestCreditReportCommand(int CustomerId, string UserEmail)` and `RequestCreditReportHandler` that loads customer, calls `ICreditReportProvider.CheckAsync(...)`, calls `Customer.UpdateCreditReport(...)`, saves, publishes domain events
- [x] 6.2 Add `POST /api/customers/{id}/credit-check` endpoint in `CustomersController` dispatching the command
- [x] 6.3 Return updated customer response (including CreditReport) on success

## 7. CustomerManagement Application: Update DTOs and Flows

- [x] 7.1 Add `employmentStatus` (string, required) to `CustomerCreateDto` and `CustomerUpdateDto`
- [x] 7.2 Add employment status validation to `CustomerCreateValidator` and `CustomerUpdateValidator`
- [x] 7.3 Update `CreateCustomerHandler` to pass `EmploymentStatus` to `Customer.Create()`
- [x] 7.4 Update `UpdateCustomerHandler` to pass `EmploymentStatus` to `Customer.Update()`
- [x] 7.5 Add `employmentStatus` (string) and `creditReport` (nullable object) to `CustomerResponse`
- [x] 7.6 Update `CustomerMapper.ToResponse()` to include employment status and credit report
- [x] 7.7 Update `CustomerMapper.ToInternalResponse()` to include `employmentStatus` and `creditReport`

## 8. RiskManagement Domain: Remove EmploymentStatus/HasPaymentDefault from Application

- [x] 8.1 Remove `EmploymentStatus` and `HasPaymentDefault` properties from Application aggregate
- [x] 8.2 Add snapshot fields to Application: `ScoringEmploymentStatus` (string), `ScoringHasPaymentDefault` (bool), `ScoringCreditScore` (int?)
- [x] 8.3 Update `Application.Create()` to accept snapshot values (string employmentStatus, bool hasPaymentDefault, int? creditScore) instead of EmploymentStatus VO and bool
- [x] 8.4 Update `Application.UpdateDetails()` to accept snapshot values instead of EmploymentStatus VO and bool
- [x] 8.5 Update `ApplyScoring()` to use snapshot fields for `ScoringService.CalculateScore()`
- [x] 8.6 Update EF Core mapping: remove `employment_status` and `has_payment_default` columns, add `scoring_employment_status` (varchar), `scoring_has_payment_default` (bool), `scoring_credit_score` (int?) columns on `applications` table

## 9. RiskManagement Application: Update Scoring Flow

- [x] 9.1 Update internal customer client/DTO to parse `employmentStatus` and `creditReport` from Customer Service response
- [x] 9.2 Update `CreateApplicationHandler`: fetch customer profile (incl. employment status + credit report), validate credit report exists, pass snapshot values to `Application.Create()`
- [x] 9.3 Update `UpdateApplicationHandler`: fetch customer profile, pass snapshot values to `Application.UpdateDetails()`
- [x] 9.4 Update `SubmitApplicationHandler`: fetch customer profile for scoring with current data
- [x] 9.5 Update `RescoreApplicationsHandler`: fetch customer profile for each application being rescored
- [x] 9.6 Remove `employmentStatus` and `hasPaymentDefault` from `ApplicationCreateDto` and `ApplicationUpdateDto`
- [x] 9.7 Remove employment status and payment default validation from `ApplicationValidator` and `ApplicationUpdateValidator`
- [x] 9.8 Add `scoringEmploymentStatus`, `scoringHasPaymentDefault`, `scoringCreditScore` to `ApplicationResponse`
- [x] 9.9 Update `ApplicationMapper.ToResponse()` to include snapshot fields

## 10. Frontend: Customer Forms with Employment Status

- [x] 10.1 Add employment status dropdown ("Beschäftigungsstatus") to Customer create form (`/customers/new`) with options: Angestellt, Selbstständig, Arbeitslos, Rentner
- [x] 10.2 Add employment status dropdown to Customer edit form (`/customers/{id}/edit`), pre-selecting current value
- [x] 10.3 Update Customer TypeScript types to include `employmentStatus` and `creditReport`

## 11. Frontend: Credit Check on Customer Detail Page

- [x] 11.1 Display CreditReport section on Customer detail page (`/customers/{id}`): show HasPaymentDefault (Ja/Nein), CreditScore, CheckedAt, Provider — or "Keine Bonitätsprüfung vorhanden" if null
- [x] 11.2 Add "Bonität prüfen" button that calls `POST /api/customers/{id}/credit-check` and refreshes the page
- [x] 11.3 Show loading state during credit check request

## 12. Frontend: Application Form Cleanup

- [x] 12.1 Remove employment status dropdown from Application create/edit form
- [x] 12.2 Remove payment default checkbox from Application create/edit form
- [x] 12.3 Update Application TypeScript types: remove `employmentStatus` and `hasPaymentDefault`, add `scoringEmploymentStatus`, `scoringHasPaymentDefault`, `scoringCreditScore`
- [x] 12.4 Update Application detail view to display scoring snapshot values (employment status, payment default, credit score) as read-only info alongside score and traffic light

## 13. Backend Tests

- [x] 13.1 Unit tests for `CreditReport` Value Object (valid creation, invalid score range, empty provider)
- [x] 13.2 Unit tests for `MockSchufaProvider` (all deterministic scenarios: name-based, age-based, default)
- [x] 13.3 Unit tests for `Customer.UpdateCreditReport()` (success, archived guard, event published)
- [x] 13.4 Unit tests for `RequestCreditReportHandler` (success, not found, archived customer)
- [x] 13.5 Unit tests for updated `Application.Create()` and `UpdateDetails()` with snapshot values
- [x] 13.6 Unit tests for `CreateApplicationHandler` with customer credit report validation (success, no credit report, customer not found)
