---
trigger: always_on
---
# Backend Architecture Rule

Maintain a strict separation between the transport layer (API/Routes) and the data/logic layer.

## Repository Pattern
- Create a dedicated repository file for each database entity in `src/lib/server/services/repositories/`.
- All direct database interactions for a specific entity MUST be implemented in its corresponding repository file.
- SvelteKit `load` functions and API routes should call repository functions instead of using the `db` object directly.

## Service Layer
- Complex business logic (e.g., risk scoring, complex validations) must reside in dedicated service files in `src/lib/server/services/`.
- Services should be pure and easily testable with Vitest.

## Input Validation
- Always use `Zod` to validate incoming data in API routes (`+server.ts`) or Form Actions (`+page.server.ts`).
- Validation should happen before passing data to services or repositories.
