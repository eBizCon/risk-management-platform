---
trigger: model_decision
description: When writing or modifying C# backend tests
---

# Backend Testing Rule

## Test Framework & Libraries

- **xUnit** as test framework
- **Moq** for mocking dependencies
- **FluentAssertions** for expressive assertions
- **Microsoft.Extensions.DependencyInjection** for DI-based tests (e.g., Dispatcher tests)

## Test Project

- All backend tests reside in `RiskManagement.Api.Tests`.
- Test project references: Api, Domain, Application, Infrastructure.

## Naming Convention

- Test classes: `<ClassUnderTest>Tests` (e.g., `DispatcherTests`, `ScoringServiceTests`)
- Test methods: `MethodName_Scenario_ExpectedResult` (e.g., `SendAsync_WithRegisteredHandler_ReturnsHandlerResult`)

## Test Categories

- **Domain Tests**: Test aggregate business rules, state transitions, domain events
- **Value Object Tests**: Test equality via `GetEqualityComponents()`, immutability, and factory method validation
- **Guard Clause Tests**: Test that aggregate factory/mutation methods reject invalid inputs with `DomainException` or `ArgumentException`
- **Domain Event Handler Tests**: Test that event handlers react correctly to domain events (e.g., audit logging, notifications)
- **Specification Tests**: Test that specifications produce correct filter expressions
- **Validation Tests**: Test FluentValidation validators with valid/invalid input
- **Handler Tests**: Test command/query handlers with mocked repositories
- **Dispatcher Tests**: Test dispatcher resolution and invocation via real DI container
- **Service Tests**: Test domain services (e.g., ScoringService) in isolation

## Test Structure (Arrange-Act-Assert)

- Each test MUST follow the Arrange-Act-Assert pattern.
- Mock only direct dependencies of the class under test.
- Test both happy path and error cases.
- Test domain exceptions and Result failure cases explicitly.

## What to Test

- Every new command/query handler MUST have corresponding tests.
- Every domain aggregate method with business rules MUST have tests.
- Every aggregate factory method MUST have guard clause tests (invalid inputs → exception).
- Every Value Object MUST have equality tests (two instances with same values are equal, different values are not).
- Every Domain Event MUST have at least one handler, and that handler MUST have tests.
- Every FluentValidation validator MUST have tests for valid and invalid inputs.
- Dispatcher auto-registration MUST be tested (handler resolves correctly from DI).
