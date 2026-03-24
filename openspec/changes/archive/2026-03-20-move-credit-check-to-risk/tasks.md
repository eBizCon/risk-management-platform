## 1. RiskManagement Domain — Credit Check Service & Value Object

- [x] 1.1 Create `CreditCheckResult` value object in `RiskManagement.Domain.ValueObjects` with `HasPaymentDefault` (bool), `CreditScore` (int?), `CheckedAt` (DateTime), `Provider` (string) and factory method with validation
- [x] 1.2 Create `ICreditCheckService` interface in `RiskManagement.Domain.Services` with `CheckAsync(string firstName, string lastName, DateOnly dateOfBirth, string street, string city, string zipCode, string country)` returning `Task<CreditCheckResult>`
- [x] 1.3 Create `CreditReport` value object in `RiskManagement.Domain.ValueObjects` (for persistence on Application aggregate) with same fields as `CreditCheckResult`, factory method `FromCheckResult(CreditCheckResult result)`

## 2. RiskManagement Infrastructure — MockSchufaProvider

- [x] 2.1 Create `MockSchufaProvider` in `RiskManagement.Infrastructure.ExternalServices` implementing `ICreditCheckService` with identical mock logic (Verzug/Default → score 250, age >65 → 520, age <25 → 350, default → 420)
- [x] 2.2 Register `ICreditCheckService` → `MockSchufaProvider` in RiskManagement DI

## 3. Application Aggregate — Replace flat credit fields with CreditReport

- [x] 3.1 Replace `HasPaymentDefault` and `CreditScore` properties on `Application` aggregate with `CreditReport` value object property
- [x] 3.2 Update `Application.Create` factory method: remove `hasPaymentDefault` and `creditScore` parameters, add `CreditReport creditReport` parameter instead
- [x] 3.3 Update `Application.UpdateDetails` method: remove `hasPaymentDefault` and `creditScore` parameters, add `CreditReport creditReport` parameter
- [x] 3.4 Update `ApplyScoring` to read `HasPaymentDefault` from `CreditReport` property
- [x] 3.5 Update `ApplicationMapper.ToResponse` to map from `CreditReport` value object

## 4. RiskManagement Persistence — EF Core Mapping

- [x] 4.1 Add EF Core value object mapping for `CreditReport` on `Application` entity (owned type or value conversion for `HasPaymentDefault`, `CreditScore`, `CreditCheckedAt`, `CreditProvider` columns)
- [x] 4.2 Create EF Core migration to add `CreditReport` columns and remove old `HasPaymentDefault`/`CreditScore` columns from Application table

## 5. Customer Profile ACL — Extend with Address and DOB

- [x] 5.1 Extend `CustomerProfile` record in `ICustomerProfileService` with `DateOfBirth` (string) and `Address` (record with Street, City, ZipCode, Country); remove `CreditReport` field
- [x] 5.2 Extend `CustomerInternalResponse` in CustomerManagement to include `DateOfBirth`, `Street`, `City`, `ZipCode`, `Country` fields; remove `CreditReport` field
- [x] 5.3 Update `CustomerMapper.ToInternalResponse` to map address and DOB fields, remove credit report mapping
- [x] 5.4 Update `CustomerServiceClient` in RiskManagement.Infrastructure to parse new fields and remove credit report parsing

## 6. Command Handlers — Integrate Credit Check

- [x] 6.1 Update `CreateApplicationHandler` to inject `ICreditCheckService`, fetch customer profile, perform credit check, pass `CreditReport` to `Application.Create`
- [x] 6.2 Update `CreateAndSubmitApplicationHandler` to inject `ICreditCheckService`, fetch customer profile, perform credit check, pass `CreditReport` to `Application.Create`
- [x] 6.3 Remove the `customerProfile.CreditReport is null` guard from both handlers (credit check is now automatic)

## 7. Remove Credit Report from CustomerManagement

- [x] 7.1 Remove `ICreditReportProvider` interface from `CustomerManagement.Domain.Services`
- [x] 7.2 Remove `MockSchufaProvider` from `CustomerManagement.Infrastructure.ExternalServices`
- [x] 7.3 Remove `CreditReport` value object from `CustomerManagement.Domain.ValueObjects`
- [x] 7.4 Remove `CreditReportReceivedEvent` from CustomerManagement domain events
- [x] 7.5 Remove `CreditReport` property and `UpdateCreditReport` method from `Customer` aggregate
- [x] 7.6 Remove `RequestCreditReportCommand` and handler from CustomerManagement.Application
- [x] 7.7 Remove credit report API endpoint from CustomerManagement.Api controllers
- [x] 7.8 Remove `ICreditReportProvider` registration from CustomerManagement DI
- [x] 7.9 Create EF Core migration to remove credit report columns from Customer table

## 8. Frontend — Remove Credit Report Flow

- [x] 8.1 Remove "Bonitätsprüfung anfordern" button/action from customer detail page
- [x] 8.2 Remove credit report display from customer detail page (or keep as read-only if still in API response)
- [x] 8.3 Update application creation flow to no longer check for existing credit report
- [x] 8.4 Update application detail/list views to read credit data from application response instead of customer

## 9. Tests

- [x] 9.1 Write unit tests for `CreditCheckResult` and `CreditReport` value objects in RiskManagement
- [x] 9.2 Move and adapt `MockSchufaProviderTests` to RiskManagement.Api.Tests targeting `ICreditCheckService`
- [x] 9.3 Update `Application` aggregate unit tests to use `CreditReport` value object instead of flat fields
- [x] 9.4 Update command handler tests (`CreateApplicationHandler`, `CreateAndSubmitApplicationHandler`) to mock `ICreditCheckService`
- [x] 9.5 Remove `RequestCreditReportHandlerTests` from test project
- [x] 9.6 Update any E2E tests that reference the credit report request flow
