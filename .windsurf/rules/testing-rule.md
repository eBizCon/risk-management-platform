---
trigger: model_decision
---

# E2E Testing Rule
- Prefer `data-testid` selectors over CSS selectors, text selectors, XPath, or brittle DOM structure selectors
- Add `data-testid` attributes to UI elements that need to be tested
- Use framework-native helpers where available:
  - Playwright: `page.getByTestId('...')`
- Only fall back to accessible roles/names (e.g. `getByRole`) if `data-testid` is not feasible. Never use XPath.
- Never implement workarounds that make the test succeed
- The e2e tests must test the actual functionality

# Component Testing Rule
- Use Vitest for component testing
- Test components in isolation
- Mock external dependencies
- Test both happy path and error cases