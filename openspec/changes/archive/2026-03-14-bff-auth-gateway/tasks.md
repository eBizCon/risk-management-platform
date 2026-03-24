## 1. Frontend Dependencies & Config

- [x] 1.1 Replace `@sveltejs/adapter-static` with `@sveltejs/adapter-node` in `package.json` and run `npm install`
- [x] 1.2 Update `svelte.config.js` to use `adapter-node` instead of `adapter-static`
- [x] 1.3 Remove `ssr = false` from `src/routes/+layout.ts`
- [x] 1.4 Remove proxy configuration from `vite.config.ts` (server.proxy + preview.proxy sections)
- [x] 1.5 Update `app.d.ts` to add `Locals` interface with optional `user` property

## 2. Session Encryption

- [x] 2.1 Create `src/lib/server/auth/session.ts` with AES-256-GCM encrypt/decrypt functions using `SESSION_SECRET` env var
- [x] 2.2 Create `src/lib/server/auth/session.ts` helpers: `createSessionCookie()`, `readSessionCookie()`, `clearSessionCookie()` that set/read/clear the encrypted `session` cookie

## 3. OIDC Client

- [x] 3.1 Install `openid-client` npm package
- [x] 3.2 Create `src/lib/server/auth/oidc.ts` with OIDC configuration from env vars (`OIDC_ISSUER`, `OIDC_CLIENT_ID`, `OIDC_CLIENT_SECRET`, `OIDC_SCOPE`, `OIDC_ROLES_CLAIM_PATH`)
- [x] 3.3 Implement OIDC discovery (fetch `.well-known/openid-configuration`), PKCE helper, and role extraction from access token in `oidc.ts`

## 4. Auth Server Routes

- [x] 4.1 Create `src/routes/auth/login/+server.ts` — generate PKCE verifier, state, nonce; store in short-lived cookies; redirect to IdP authorization endpoint. Support `returnTo` query param (reject absolute URLs).
- [x] 4.2 Create `src/routes/auth/callback/+server.ts` — exchange code for tokens, validate state, extract user identity + roles from access token, create encrypted session cookie, redirect to returnTo or `/`
- [x] 4.3 Create `src/routes/auth/logout/+server.ts` — clear session cookie, redirect to IdP end_session_endpoint with id_token_hint
- [x] 4.4 Update login page (`src/routes/login/+page.svelte`) — form action points to `/auth/login` instead of `/login`
- [x] 4.5 Update logout link in layout (`src/routes/+layout.svelte`) — href points to `/auth/logout` instead of `/logout`

## 5. Server Hooks & API Proxy

- [x] 5.1 Create `src/hooks.server.ts` with `handle` function that reads session cookie and sets `event.locals.user`
- [x] 5.2 Add API proxy logic in `handle`: intercept `/api/*` requests, validate session, forward to backend with `X-Api-Key`, `X-User-Id`, `X-User-Email`, `X-User-Name`, `X-User-Role` headers
- [x] 5.3 Implement routing rules: `/api/customers/*` → `CUSTOMER_SERVICE_URL`, `/api/test/*` → handled locally, all other `/api/*` → `RISK_MANAGEMENT_API_URL`

## 6. Layout Migration

- [x] 6.1 Convert `src/routes/+layout.ts` to `src/routes/+layout.server.ts` — read user from `event.locals.user` and return as page data
- [x] 6.2 Remove the old `role.ts` store (client-side cookie-based role switching) — user comes from server session now

## 7. Test Session Endpoint (SvelteKit)

- [x] 7.1 Create `src/routes/api/test/session/+server.ts` — POST creates encrypted session cookie from request body `{id, email, name, role}`, DELETE clears it. Only available in dev/test mode.
- [x] 7.2 Update `e2e/helpers/auth.ts` — `createTestSession` and `clearTestSessions` now call the SvelteKit test endpoint (same URL `/api/test/session`, no changes needed if URL is identical)

## 8. Backend — InternalAuthMiddleware

- [x] 8.1 Create `SharedKernel/Middleware/InternalAuthMiddleware.cs` — validates `X-Api-Key` on all `/api/*` routes, constructs `ClaimsPrincipal` from `X-User-Id`, `X-User-Email`, `X-User-Name`, `X-User-Role` headers. Bypasses `/health`. Allows `/api/internal/*` with API key only (no user headers required).
- [x] 8.2 Add unit tests for `InternalAuthMiddleware` in `SharedKernel.Tests` or appropriate test project

## 9. Backend — RiskManagement.Api Cleanup

- [x] 9.1 Remove `AuthController.cs` (login/logout/user endpoints now handled by SvelteKit)
- [x] 9.2 Remove `Extensions/AuthenticationExtensions.cs`, `Extensions/OidcOptions.cs`, `Extensions/RoleClaimHelper.cs`, `Extensions/JsonAuthorizationResultHandler.cs`
- [x] 9.3 Remove OIDC/Cookie authentication setup from `Program.cs` — replace `AddOidcAuthentication()` with simple authorization policy registration + `InternalAuthMiddleware`
- [x] 9.4 Remove DataProtection shared-keys setup from `Program.cs`
- [x] 9.5 Remove `GetCurrentUser` API endpoint (no longer needed — layout.server.ts provides user) (replacement for `AuthController.GetCurrentUser`) — reads user from `HttpContext.User` (set by InternalAuthMiddleware) and returns DTO. Or remove if layout.server.ts provides user directly.
- [x] 9.6 Update `Program.cs` to use `InternalAuthMiddleware` instead of `ApiKeyAuthMiddleware` + OIDC auth
- [x] 9.7 Remove `TestSessionController` (test sessions now handled by SvelteKit endpoint) — in dev/test mode, set `HttpContext.User` directly for integration test compatibility

## 10. Backend — CustomerManagement.Api Cleanup

- [x] 10.1 Remove `Extensions/AuthenticationExtensions.cs`, `Extensions/OidcOptions.cs`, `Extensions/RoleClaimHelper.cs`, `Extensions/JsonAuthorizationResultHandler.cs` and related files
- [x] 10.2 Remove OIDC/Cookie authentication setup from `Program.cs` — replace with simple authorization policies + `InternalAuthMiddleware`
- [x] 10.3 Remove DataProtection shared-keys setup from `Program.cs`
- [x] 10.4 Update `Program.cs` to use `InternalAuthMiddleware` instead of `ApiKeyAuthMiddleware`

## 11. Environment Configuration

- [x] 11.1 Create/update `.env.example` or `.env.development` with all required env vars: `SESSION_SECRET`, `OIDC_ISSUER`, `OIDC_CLIENT_ID`, `OIDC_CLIENT_SECRET`, `OIDC_SCOPE`, `OIDC_ROLES_CLAIM_PATH`, `OIDC_POST_LOGOUT_REDIRECT_URI`, `OIDC_REDIRECT_URI`, `RISK_MANAGEMENT_API_URL`, `CUSTOMER_SERVICE_URL`, `SERVICE_API_KEY`
- [x] 11.2 Update frontend `package.json` scripts if needed (dev/build/preview commands for adapter-node)

## 12. Verification

- [x] 12.1 Verify frontend builds without errors (`npm run build`)
- [x] 12.2 Verify backend builds without errors (`dotnet build`)
- [ ] 12.3 Run E2E tests (`npm run test:e2e:ci`) and fix any failures
- [ ] 12.4 Manual smoke test: login via Keycloak → navigate → API calls work → logout
