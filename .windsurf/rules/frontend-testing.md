---
trigger: model_decision
description: When writing or modifying frontend tests (E2E with Playwright, component tests with Vitest)
---

# Frontend E2E Testing Rule

- Prefer `data-testid` selectors over CSS selectors, text selectors, XPath, or brittle DOM structure selectors.
- Add `data-testid` attributes to UI elements that need to be tested.
- Use framework-native helpers where available:
  - Playwright: `page.getByTestId('...')`
- Only fall back to accessible roles/names (e.g. `getByRole`) if `data-testid` is not feasible. Never use XPath.
- Never implement workarounds that make the test succeed — fix actual functionality instead.
- E2E tests reside in `src/frontend/e2e/`.
- Run E2E tests with `npm run test:e2e:ci` (headless, CI mode).

# Frontend Component Testing Rule

- Use Vitest for component testing.
- Test components in isolation.
- Mock external dependencies.
- Test both happy path and error cases.