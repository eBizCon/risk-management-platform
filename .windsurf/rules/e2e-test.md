---
trigger: always_on
---

# E2E Testing Rule: Prefer data-testid

- Never implement workarounds that make the test succeed
- The e2e tests must test the actual functionality

## Selector strategy (highest priority)
- In E2E tests ALWAYS prefer `data-testid` selectors over CSS selectors, text selectors, XPath, or brittle DOM structure selectors.
- Use framework-native helpers where available:
  - Playwright: `page.getByTestId('...')`
  - Testing Library: `getByTestId('...')`
  - Cypress: `[data-testid="..."]`
- Only fall back to accessible roles/names (e.g. `getByRole`) if `data-testid` is not feasible. Never use XPath.

## UI implementation requirements
- When implementing or modifying UI, ensure stable `data-testid` attributes exist for:
  - primary actions (buttons/links)
  - form fields and validation messages
  - modals/dialogs
  - tables/lists and their key rows/items
  - navigation elements
- If you add a new interactive element, you MUST add a `data-testid` suitable for E2E testing.

## Naming convention
- Use kebab-case and a consistent prefix: `<area>-<component>-<element>`
  - Examples: `checkout-submit`, `login-email-input`, `users-table-row`, `users-delete-button`
- Avoid dynamic/unstable values in testids (no GUIDs/timestamps). Prefer semantic identifiers.

## Code review / change policy
- Any PR that adds or changes UI must include appropriate `data-testid` additions/updates.
- Any E2E test change must replace non-testid selectors with `data-testid` when possible.