---
name: run-e2e
description: E2E Tests ausführen. Verwende diesen Skill wenn du End-to-End (e2e) Tests, Playwright Tests oder Integrationstests ausführen möchtest. Auch relevant wenn du nach Code-Änderungen prüfen willst ob die Anwendung noch korrekt funktioniert, wenn du einen Bug verifizieren oder eine Regression ausschließen willst, oder wenn du eine neue Feature-Implementierung gegen bestehende Tests validieren möchtest. Nutze diesen Skill immer bevor du dem User sagst dass eine Aufgabe abgeschlossen ist, wenn die Aufgabe UI- oder Backend-Änderungen beinhaltet.
---

# Run E2E Tests

Runs E2E tests against a locally running Aspire stack.

## Prerequisites: Start the Application Stack

The app must be running locally via Aspire before executing tests.

### 1) Start Backend Stack with Aspire

```bash
dotnet run --project ./src/backend/AppHost/AppHost.csproj
```

Run this as a **non-blocking** command (the Aspire host runs continuously).

### 2) Start Frontend

In a second terminal:

```bash
cd ./src/frontend
cp .env.test .env
npm run dev -- --port 5173
```

Run this as a **non-blocking** command.

### 3) Wait for Health Checks

Wait until all services are healthy:
- Keycloak is healthy (check Aspire dashboard)
- Frontend on http://localhost:5173 is reachable
- All APIs are healthy in Aspire dashboard

## E2E Test Command

Once the stack is running:

```bash
cd ./src/frontend
npm run test:e2e:ci
```

This runs `CI=true playwright test` which:
- Runs tests headless in Chromium (2 workers, 2 retries on failure)
- Uses `.env.test` for environment variables
- Generates an HTML report in `playwright-report/`

**Always use `test:e2e:ci`** — without `:ci` Playwright launches a headed browser.

### Run a single test file

```bash
cd ./src/frontend
CI=true npx playwright test e2e/applicant.test.ts
```

### Run a single test by title

```bash
cd ./src/frontend
CI=true npx playwright test -g "should display the home page"
```

## Browser-Based Login Credentials

For manual browser testing or debugging:
- **applicant** / `applicant`
- **processor** / `processor`

## Workflow Summary

1. Start Aspire backend (**non-blocking**)
2. Start frontend dev server (**non-blocking**)
3. Wait for all services to be healthy
4. Run E2E tests and wait for completion
5. Report results to the user

## Project Structure

- **Config**: `playwright.config.ts` — baseURL `http://localhost:5173`, testDir `e2e/`
- **Tests**: `e2e/*.test.ts` — import `test` and `expect` from `e2e/fixtures.ts`
- **Fixtures**: `e2e/fixtures.ts` — provides `authenticatedPage` and `authenticatedContext` fixtures
- **Auth helper**: `e2e/helpers/auth.ts` — `createTestSession(page, role)` and `clearTestSessions(page)` via `/api/test/session`
- **Roles**: `applicant` (default) and `processor` — switch with `test.use({ userRole: 'processor' })`

## Interpreting Failures

- **Port 5173 already in use**: Another dev server is running. Kill it first: `lsof -ti:5173 | xargs kill -9`
- **Aspire services not healthy**: Check the Aspire dashboard and wait for all services to start
- **Session creation failed**: The `/api/test/session` endpoint is missing or broken. Check `src/routes/api/test/session/`
- **Timeout on navigation**: The page did not load in time. Check if the frontend dev server started successfully
- **`data-testid` not found**: A UI element is missing its `data-testid` attribute. Fix the component, not the test
- **Flaky tests**: If a test passes on retry (shown as "flaky" in output), investigate the root cause rather than ignoring it
