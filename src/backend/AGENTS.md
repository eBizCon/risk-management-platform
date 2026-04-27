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

## Testing

- Use backend tests to validate domain behavior, handlers, and integration boundaries.
- Prefer Arrange-Act-Assert structure and explicit assertions for business failures.

## Precedence

- This file applies to the entire `src/backend/` subtree.
- A deeper `AGENTS.md` overrides this file for its own subtree when needed.
