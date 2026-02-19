---
name: run-e2e
description: E2E Tests ausführen. Verwende diesen Skill wenn du End-to-End (e2e) Tests, Playwright Tests oder Integrationstests ausführen möchtest.
---

# Run E2E Tests

## Workflow

1. Run `npm run test:e2e:ci` (the `:ci` suffix is required — without it Windsurf launches a headed browser it cannot control)
2. Wait for the tests to complete
3. Report the results to the user
