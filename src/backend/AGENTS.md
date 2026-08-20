# AGENTS.md

This file defines instructions for the `src/backend/` scope.

## Architecture

- Follow strict DDD and Clean Architecture boundaries.
- Keep layers separated: `Api -> Application -> Domain`, with `Infrastructure` implementing Domain/Application contracts.
- Keep bounded contexts decoupled. Do not introduce direct project references between `RiskManagement.*` and `CustomerManagement.*`.

## CQRS and Application Flow

- Use command/query handlers for all writes/reads.
- Use the dispatcher pattern in controllers instead of injecting individual handlers.
- Keep controllers thin: map transport input, dispatch, map result to HTTP response.

## Dispatcher Pattern

- `IDispatcher` is the only abstraction injected into controllers.
- Handlers (`ICommandHandler<,>`, `IQueryHandler<,>`, `IDomainEventHandler<>`) are auto-registered by assembly scanning in `Infrastructure/DependencyInjection.cs`.
- Dispatch uses `IServiceProvider` + reflection (`MakeGenericType` + `GetMethod("HandleAsync").Invoke`).
- Domain events are published explicitly after saving changes, then cleared from the aggregate.
- Use `PublishDomainEventsAsync(AggregateRoot aggregate)` to capture, publish, and clear events in one call.
- Event dispatching tolerates zero registered handlers; command/query dispatching throws when no handler is found.

## Domain Modeling

- Preserve aggregate invariants via domain methods and guard clauses.
- Prefer value objects and strongly typed IDs over primitive obsession.
- Raise domain events from aggregates for domain-significant state changes.

## Backend Code Style

- Use clear naming and strong typing.
- Keep methods focused and small.
- Use async/await for async flows; avoid blocking calls.
- Keep authentication/authorization behavior API-safe (return status codes, do not redirect API requests).

## Data and Persistence

- Keep repository interfaces in the Domain layer and implementations in Infrastructure.
- Do not expose DbContext outside Infrastructure.
- Use dedicated read models for query-heavy read use cases.

## Seeding

- Seeding is handled by a dedicated central backend project invoked via AppHost.
- The seeder seeds both `CustomerManagement` and `RiskManagement` contexts automatically.
- APIs must not run in-process seeding.
- Keep strict bounded-context decoupling: no direct project references between `RiskManagement.*` and `CustomerManagement.*`.

## Testing

- Use backend tests to validate domain behavior, handlers, and integration boundaries.
- Prefer Arrange-Act-Assert structure and explicit assertions for business failures.

## Known Issues

- Keep `Microsoft.IdentityModel.*` package versions aligned. A mismatch between `Microsoft.IdentityModel.Protocols.OpenIdConnect` (7.1.2) and `System.IdentityModel.Tokens.Jwt` (7.7.1) caused `OpenIdConnectConfiguration` to parse `authorization_endpoint` as empty. Align all IdentityModel packages to the same version (e.g., 7.7.1) with explicit `PackageReference` entries.

## Precedence

- This file applies to the entire `src/backend/` subtree.
- A deeper `AGENTS.md` overrides this file for its own subtree when needed.
