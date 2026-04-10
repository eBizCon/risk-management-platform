---
trigger: glob
globs: src/frontend/**
---

# Frontend Code Style Rule

Ensure consistent, clean, and idiomatic TypeScript/Svelte code across the frontend.

## UI Test Attributes

- When implementing or modifying UI, ensure stable `data-testid` attributes exist for:
  - primary actions (buttons/links)
  - form fields and validation messages
  - modals/dialogs
  - tables/lists and their key rows/items
  - navigation elements
- If you add a new interactive element, you MUST add a `data-testid` suitable for E2E testing.

## Naming Convention for data-testid

- Use kebab-case with a consistent prefix: `<area>-<component>-<element>`
  - Examples: `checkout-submit`, `login-email-input`, `users-table-row`, `users-delete-button`
- Avoid dynamic/unstable values in testids (no GUIDs/timestamps). Prefer semantic identifiers.

## TypeScript Types

- Use TypeScript for all new code.
- Prefer interfaces over types for object shapes.
- Use enums for fixed sets of related values.
- Use generics for reusable, type-safe components.
- Avoid using `any` — use `unknown` instead when type is not known.

## Icons

- Use **lucide-svelte** as the standard icon library.
- Import icons from `lucide-svelte` (e.g., `import { Plus, Edit, Trash } from 'lucide-svelte'`).
- Icons are Svelte components that accept size, color, and other props.
- Use consistent icon sizing (typically `w-4 h-4`, `w-5 h-5`, or `w-6 h-6`).

## Code Quality

- Follow SOLID principles.
- Keep functions small and focused on a single responsibility.
- Avoid code duplication.
- Use meaningful variable and function names.
- All async functions MUST use async/await — not .then() chains.

## Frontend Owns Navigation

- The frontend determines navigation targets client-side after successful API mutations.
- Use the response data (e.g., `application.id`) to build navigation paths.
- Never rely on server-provided redirect paths.

## Authentication & Authorization

- **OIDC Login Flow**: Use openid-client for OIDC authentication with PKCE (Proof Key for Code Exchange).
- **Login Handler**: Server endpoint sets PKCE state, nonce, and returnTo cookies, then redirects to Keycloak.
- **Callback Handler**: Exchange authorization code for tokens, validate roles, create session, handle returnTo.
- **Logout Handler**: Clear session and redirect to Keycloak logout or post-logout redirect URL.
- **Role-Based Authorization**: Use RoleGuard component to protect routes based on user roles (applicant, processor, risk-manager).
- **Session Management**: Session is stored in cookies; check session validity on protected routes.
- **No Auto-Redirect**: For unauthorized access, show "Keine Berechtigung" message with link to login instead of auto-redirecting.
- **Role Changes**: Role changes take effect on next login (not immediate).
