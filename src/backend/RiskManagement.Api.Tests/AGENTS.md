# AGENTS.md

This file defines instructions for the `src/backend/RiskManagement.Api.Tests/` scope.

## Test-specific focus

When editing tests in this scope, always apply:

- Use xUnit style test structure with clear Arrange-Act-Assert sections.
- Keep tests deterministic and isolated.
- Mock only direct dependencies of the unit under test.
- Assert both happy paths and failure paths.
- Validate domain invariants, handler behavior, and dispatching behavior when relevant.

Apply additionally when tests touch database access logic:

- Verify query correctness and persistence behavior without leaking Infrastructure internals into Application/Domain assertions.

## Precedence

- This file is standalone and does not extend any other AGENTS file.
- This file has priority for this test subtree.
