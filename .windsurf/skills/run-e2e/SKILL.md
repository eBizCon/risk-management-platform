---
name: run-e2e
description: E2E Tests ausführen. Verwende diesen Skill wenn du End-to-End (e2e) Tests, Playwright Tests oder Integrationstests ausführen möchtest. Auch relevant wenn du nach Code-Änderungen prüfen willst ob die Anwendung noch korrekt funktioniert, wenn du einen Bug verifizieren oder eine Regression ausschließen willst, oder wenn du eine neue Feature-Implementierung gegen bestehende Tests validieren möchtest. Nutze diesen Skill immer bevor du dem User sagst dass eine Aufgabe abgeschlossen ist, wenn die Aufgabe UI- oder Backend-Änderungen beinhaltet.
---

# Run E2E Tests

Runs E2E tests with automatic startup/shutdown of backend and frontend via Playwright `webServer`.

## Prerequisites

No manual startup is required for the default workflow.

Playwright starts these services automatically from `src/frontend/playwright.config.ts`:
- Backend: `dotnet run --project ../backend/AppHost/AppHost.csproj --launch-profile http-testmode`
- Frontend: `npm run build && TEST=true npm run preview`

It waits for readiness URLs before running tests:
- Backend health: `http://localhost:5627/health`
- Frontend preview: `http://localhost:4173`

Dedicated E2E ports are used to avoid collisions with other local Aspire instances:
- Risk API: `5627`
- Customer API: `5400`
- Keycloak: `8181`
- Aspire dashboard OTLP: `29032`
- Aspire resource service: `30132`

## E2E Test Commands

Run from frontend directory:

```bash
cd ./src/frontend
npm run test:e2e:ci
```

Alternative local command:

```bash
cd ./src/frontend
npm run test:e2e
```

This runs Playwright in Chromium and generates an HTML report in `playwright-report/`.

`test:e2e:ci` runs `CI=true playwright test` and is preferred for deterministic CI-like behavior.

## Run targeted tests

### Single test file

```bash
cd ./src/frontend
CI=true npx playwright test e2e/applicant.test.ts
```

### Single test by title

```bash
cd ./src/frontend
CI=true npx playwright test -g "should display the home page"
```

## Browser-Based Login Credentials

For manual browser testing or debugging:
- **applicant** / `applicant`
- **processor** / `processor`

## Workflow Summary

1. Run `npm run test:e2e:ci` from `src/frontend`
2. Playwright starts backend AppHost automatically
3. Playwright starts frontend preview automatically
4. Playwright waits for health/readiness URLs
5. Tests execute and services are shut down automatically

## Project Structure

- **Config**: `playwright.config.ts` — `webServer` starts backend + frontend, baseURL `http://localhost:4173`, testDir `e2e/`
- **Tests**: `e2e/*.test.ts` — import `test` and `expect` from `e2e/fixtures.ts`
- **Fixtures**: `e2e/fixtures.ts` — provides `authenticatedPage` and `authenticatedContext` fixtures
- **Auth helper**: `e2e/helpers/auth.ts` — `createTestSession(page, role)` and `clearTestSessions(page)` via `/api/test/session`
- **Roles**: `applicant` (default) and `processor` — switch with `test.use({ userRole: 'processor' })`

## Interpreting Failures

- **Backend startup timeout**: Check AppHost output and verify `http://localhost:5627/health` becomes reachable.
- **Port conflict on E2E ports**: Free `5627`, `5400`, `8181`, `29032`, `30132`, `4173` or adjust `playwright.config.ts`.
- **`reuseExistingServer` behavior**: Locally (`!CI`) Playwright may reuse existing processes; in CI it starts fresh.
- **Session creation failed**: The `/api/test/session` endpoint is missing or broken. Check `src/routes/api/test/session/`
- **Timeout on navigation**: Check that frontend preview startup completed (`npm run build && TEST=true npm run preview`).
- **`data-testid` not found**: A UI element is missing its `data-testid` attribute. Fix the component, not the test.
- **Flaky tests**: If a test passes on retry (shown as "flaky"), investigate the root cause instead of ignoring it.
