## Why

Authentication logic (OIDC flow, cookie management, session handling, DataProtection key sharing) is duplicated across both backend services (RiskManagement.Api and CustomerManagement.Api). Both services carry full OIDC configuration, cookie auth middleware, and role extraction — even though only RiskManagement.Api initiates login/logout. Adding a third service would require yet another copy. The shared DataProtection keys on a filesystem path make cloud/container deployments fragile.

Moving authentication into the SvelteKit server (BFF pattern) centralizes session ownership in one place, makes backend services stateless, and eliminates the DataProtection coupling.

## What Changes

- **SvelteKit becomes the BFF (Backend-for-Frontend)**: Owns the OIDC flow (login, callback, logout), session cookie, and user identity.
- **SvelteKit proxies API calls**: All `/api/*` requests from the browser go through SvelteKit's server hooks, which inject internal auth headers before forwarding to backend services.
- **Backend services become stateless**: Both C# APIs drop OIDC/cookie auth entirely. They validate requests via an internal auth mechanism (API key + user claim headers set by the BFF).
- **`adapter-static` → `adapter-node`**: SvelteKit needs a running server to handle auth and API proxying.
- **`ssr = false` removed**: The root layout becomes a server layout to provide user data from the session. Page load functions (`+page.ts`) remain client-side (hybrid SSR).
- **DataProtection shared-keys removed**: No longer needed — session lives only in the BFF.
- **BREAKING**: `AuthController` and `AuthenticationExtensions` removed from both C# services. Backend `/login`, `/logout` endpoints no longer exist on the C# services.
- **BREAKING**: Vite proxy configuration removed — SvelteKit server handles all routing.

## Capabilities

### New Capabilities
- `bff-session`: SvelteKit server-side session management (encrypted cookie, OIDC integration, API proxy with internal auth headers)
- `internal-service-auth`: Backend middleware that authenticates requests from the BFF via API key and reads user identity from forwarded headers

### Modified Capabilities
- `risk-manager-authorization`: Authorization policies remain, but the authentication mechanism changes from OIDC cookie to internal header-based identity.

## Impact

- **Frontend**: `svelte.config.js`, `package.json`, `vite.config.ts`, `app.d.ts`, `+layout.ts`, new `hooks.server.ts`, new auth server routes, new API proxy logic
- **Backend (RiskManagement.Api)**: Remove `AuthController.cs`, `AuthenticationExtensions.cs`, `OidcOptions.cs`, OIDC NuGet packages, DataProtection shared-keys setup. Add internal auth middleware.
- **Backend (CustomerManagement.Api)**: Same removals. Add internal auth middleware.
- **SharedKernel**: New `InternalAuthMiddleware` replacing `ApiKeyAuthMiddleware` (extends it with user identity headers)
- **E2E Tests**: Test session creation moves from C# `TestSessionController` to a SvelteKit server endpoint. `e2e/helpers/auth.ts` updated.
- **Dependencies**: Add `@sveltejs/adapter-node`, `openid-client` to frontend. Remove OIDC NuGet packages from backend (optional, can keep for future use).
- **DevOps**: Dev workflow changes — SvelteKit dev server handles routing instead of Vite proxy. Production needs Node.js runtime for SvelteKit.
