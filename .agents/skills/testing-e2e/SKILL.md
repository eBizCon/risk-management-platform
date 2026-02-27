# E2E Testing: Risk Management Platform

## Overview
This skill covers end-to-end testing of the SvelteKit risk management platform with Keycloak OIDC authentication.

## Devin Secrets Needed
No secrets required — test users are provisioned in the Keycloak realm import.

## Local Environment Setup

1. **Copy environment file**: `cp .env.example .env`
2. **Start Keycloak**: `./dev/keycloak/keycloak-up.sh` — wait for "Realm 'risk-management' successfully created" log before proceeding
3. **Start dev server**: `npm run dev` — starts at http://localhost:5173
4. **If `better-sqlite3` errors occur** (NODE_MODULE_VERSION mismatch): run `npm rebuild better-sqlite3` before starting the dev server. This happens when the Node.js version changes.

## Test Users (from Keycloak realm import)

| Username | Password | Role | Nav Links Visible |
|----------|----------|------|-------------------|
| admin | admin | admin | "Scoring konfigurieren" |
| applicant | applicant | applicant | "Meine Anträge", "Neuer Antrag" |
| processor | processor | processor | "Anträge bearbeiten" |

## Authentication Flow

1. Navigate to http://localhost:5173/login
2. Click "Login" button → redirects to Keycloak at http://localhost:8081
3. Enter username/password on Keycloak form, click "Sign In"
4. Redirects back to app homepage with role-specific navigation
5. To logout: click "Logout" in nav bar → redirects to homepage
6. **Important**: Sessions are lost when the dev server restarts. You must re-login after any server restart.

## Form Interaction Tips

- **Text/number inputs**: Browser `type` action may not always trigger Svelte 5 reactivity. Use JavaScript `dispatchEvent(new Event('input', {bubbles: true}))` after setting `.value` as a reliable fallback.
- **Radio buttons**: Use JavaScript to set `.checked = true` and dispatch `change` event. Direct click on radio devinid may not work.
- **Select dropdowns**: `select_option` with index works well.
- **Confirmation dialogs**: The "Antrag einreichen" button (type="button") shows a modal dialog. The confirm button inside the modal may need JavaScript `.click()` to activate — use `Array.from(buttons).find(b => b.textContent.trim() === 'Antrag einreichen' && b.closest('[aria-modal]'))` to target it.
- **Navigation links**: If nav link clicks are blocked, use direct URL navigation as fallback.

## Scoring Logic

Score starts at 100 and deductions are applied:
- Income ratio (available/total): <50% → -15, <30% → -30, <10% → -50
- Rate to available income ratio: 30-50% → -10, 50-70% → -25, >70% → -40
- Employment: self_employed → -10, retired → -5, unemployed → -35
- Payment default: yes → -25

Traffic light thresholds (configurable via /admin):
- Score >= thresholdGreen → green ("Positiv")
- Score >= thresholdYellow → yellow ("Prüfung erforderlich")
- Score < thresholdYellow → red ("Kritisch")

Default thresholds: green=75, yellow=50.

## Key Test Scenarios

### Admin Scoring Config
1. Login as admin → navigate to /admin
2. Verify default values displayed
3. Change threshold values → click "Speichern"
4. Verify success message and updated audit info (updatedAt, updatedBy)

### Applicant Submission with Custom Thresholds
1. After changing thresholds as admin, login as applicant
2. Create application with financials that produce a score between old and new thresholds
3. Verify the traffic light color reflects the NEW threshold, not the default
4. Example: With thresholdGreen=95, an application scoring 80 should show yellow, not green

### Access Control (403)
1. While logged in as applicant, navigate to /admin
2. Verify 403 "Keine Berechtigung" page is shown

## Common Issues

- **Keycloak not ready**: The Keycloak startup script may take 30-60 seconds. Wait for the realm creation log message.
- **Database locked**: If you see SQLite "database is locked" errors, restart the dev server.
- **Session expired**: After long idle periods, the session may expire. Re-login via the normal flow.
