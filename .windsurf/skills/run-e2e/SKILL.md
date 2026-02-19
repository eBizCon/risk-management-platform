---
name: run-e2e
description: E2E Tests ausführen. Verwende diesen Skill wenn du End-to-End (e2e) Tests, Playwright Tests oder Integrationstests ausführen möchtest. Auch relevant wenn du nach Code-Änderungen prüfen willst ob die Anwendung noch korrekt funktioniert, wenn du einen Bug verifizieren oder eine Regression ausschließen willst, oder wenn du eine neue Feature-Implementierung gegen bestehende Tests validieren möchtest. Nutze diesen Skill immer bevor du dem User sagst dass eine Aufgabe abgeschlossen ist, wenn die Aufgabe UI- oder Backend-Änderungen beinhaltet.
---

# Run E2E Tests

## Command

```bash
npm run test:e2e:ci
```

This runs `CI=true playwright test` which:

- Builds the app (`npm run build`) and starts a preview server on port 4173
- Runs tests headless in Chromium (2 workers, 2 retries on failure)
- Uses `.env.test` for environment variables
- Generates an HTML report in `playwright-report/`

**Always use `test:e2e:ci`** — without `:ci` Playwright launches a headed browser that Windsurf cannot control.

### Run a single test file

```bash
CI=true npx playwright test e2e/applicant.test.ts
```

### Run a single test by title

```bash
CI=true npx playwright test -g "should display the home page"
```

## Workflow

1. Run the command as a **non-blocking** command (it takes 30-90 seconds)
2. Wait for completion using `command_status` with `WaitDurationSeconds: 60`
3. If still running, wait again
4. Parse the output and report results to the user

## Project Structure

- **Config**: `playwright.config.ts` — webServer, baseURL `http://localhost:4173`, testDir `e2e/`
- **Tests**: `e2e/*.test.ts` — import `test` and `expect` from `e2e/fixtures.ts`
- **Fixtures**: `e2e/fixtures.ts` — provides `authenticatedPage` and `authenticatedContext` fixtures
- **Auth helper**: `e2e/helpers/auth.ts` — `createTestSession(page, role)` and `clearTestSessions(page)` via `/api/test/session`
- **Roles**: `applicant` (default) and `processor` — switch with `test.use({ userRole: 'processor' })`

## Interpreting Failures

- **Port 4173 already in use**: Another preview server is running. Kill it first: `lsof -ti:4173 | xargs kill -9`
- **Session creation failed**: The `/api/test/session` endpoint is missing or broken. Check `src/routes/api/test/session/`
- **Timeout on navigation**: The page did not load in time. Check if the build succeeded (look for build errors above the test output)
- **`data-testid` not found**: A UI element is missing its `data-testid` attribute. Fix the component, not the test
- **Flaky tests**: If a test passes on retry (shown as "flaky" in output), investigate the root cause rather than ignoring it

## Rules

- Prefer `data-testid` selectors (`page.getByTestId(...)`)
- Fall back to accessible roles/names (`getByRole`) only if `data-testid` is not feasible
- Never use XPath or brittle CSS selectors
- Never implement workarounds that make a test succeed — fix the actual functionality
- Always add `data-testid` attributes to new interactive UI elements
