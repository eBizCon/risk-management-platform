# AGENTS.md

This file defines instructions for the `src/frontend/e2e/` scope.

## E2E Test Principles

- Keep tests deterministic, isolated, and role-aware.
- Prefer stable `data-testid` selectors.
- Use explicit waits tied to meaningful app state (URL change, response completion, visible state markers).
- Avoid arbitrary sleeps unless there is no reliable signal.

## Reliability

- Fix flaky behavior at the source instead of masking failures with long timeouts.
- Keep each test independent from prior test data where possible.
- Assert critical intermediate states for multi-step workflows.

## Session and Role Handling

- Keep authentication setup explicit per test context.
- Avoid side effects across parallel workers.
- Ensure role switches happen deliberately and are validated by page state.

## Scope

- This file has priority for the `src/frontend/e2e/` subtree.
- Parent frontend instructions still apply unless they conflict with this file.
