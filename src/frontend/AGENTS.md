# AGENTS.md

This file defines instructions for the `src/frontend/` scope.

## Frontend Development Principles

- Keep TypeScript and Svelte code strongly typed and easy to read.
- Keep functions/components focused and avoid duplicated logic.
- Frontend decides navigation targets after successful API operations.
- Prefer async/await over promise chains.

## UI and Testability

- Add stable `data-testid` attributes for important interactive elements.
- Prefer semantic and deterministic test IDs; avoid dynamic identifiers.
- Keep test selectors resilient to layout/style changes.

## Svelte Components

- Use Svelte 5 runes for component state and derived values.
- Keep side effects explicit and minimal.
- Prefer clear prop contracts and predictable event/callback flow.

## Styling

- Follow utility-first styling with Tailwind classes.
- Keep responsive behavior mobile-first.
- Prefer reusable style patterns over repeated ad-hoc class combinations.

## Frontend Testing

- For E2E, prefer `getByTestId` selectors.
- For component tests, keep tests isolated and mock external dependencies where needed.
- Do not hide flaky behavior with brittle waits; stabilize the underlying flow.

## Precedence

- This file applies to the entire `src/frontend/` subtree.
- A deeper `AGENTS.md` overrides this file for its own subtree when needed.
