## Context

Authentication is currently handled by each C# backend service independently. Both `RiskManagement.Api` and `CustomerManagement.Api` carry identical OIDC configuration (`AuthenticationExtensions.cs`), cookie auth setup (cookie name `"session"`, same DataProtection keys), and role extraction logic. The frontend is a static SPA (`adapter-static`, `ssr = false`) that proxies API calls via Vite dev proxy to the two backend services.

The current architecture:
- Browser → Vite Proxy → RiskManagement.Api (OIDC owner, `/login`, `/logout`, `/auth/callback`)
- Browser → Vite Proxy → CustomerManagement.Api (reads shared cookie, no login endpoints)
- Shared DataProtection keys on filesystem (`shared-keys/`) enable cookie sharing
- `TestSessionController` in RiskManagement.Api creates test sessions for E2E

## Goals / Non-Goals

**Goals:**
- Centralize OIDC authentication in the SvelteKit server (BFF pattern)
- Make backend services stateless (no OIDC, no cookies, no DataProtection sharing)
- Eliminate duplicated auth code across C# services
- Enable cloud/container deployments without shared filesystem dependencies
- Maintain all existing E2E test functionality

**Non-Goals:**
- Full SSR migration (page load functions stay as `+page.ts`, not `+page.server.ts`)
- JWT-based service-to-service auth (using simpler header-based identity forwarding)
- Token refresh / silent re-authentication (session expires, user re-logs)
- Multi-tenant or multi-IdP support

## Decisions

### D1: SvelteKit as BFF with `adapter-node`

**Decision**: Replace `@sveltejs/adapter-static` with `@sveltejs/adapter-node`. SvelteKit runs as a Node.js server that owns authentication and proxies API calls.

**Alternatives considered**:
- *Dedicated API Gateway (YARP/Ocelot)*: More infrastructure, separate deployment, overkill for 2-3 services
- *Shared Auth Library in C#*: Solves duplication but keeps DataProtection coupling and OIDC in every service
- *Nginx as auth proxy*: Complex OIDC flow in Nginx config, hard to maintain

**Rationale**: SvelteKit already serves the frontend. Adding auth + proxy is natural and avoids a new infrastructure component.

### D2: Hybrid SSR — Only auth is server-side

**Decision**: Remove `ssr = false` from root layout. Create `+layout.server.ts` to provide user data from the session. All `+page.ts` files remain as universal load functions (client-side after initial load).

**Rationale**: Minimal migration effort. Pages don't need SSR. Only the user/session needs to be server-provided so the layout can render auth state on first load.

### D3: Encrypted cookie for session storage

**Decision**: Store session data (user ID, email, name, role, id_token for logout) in an AES-256-GCM encrypted httpOnly cookie managed by the SvelteKit server.

**Alternatives considered**:
- *Server-side session store (Redis/DB)*: Adds infrastructure dependency
- *Unencrypted signed cookie*: Leaks user data to browser

**Rationale**: Stateless, no external dependencies, small payload (~300 bytes encrypted). Session secret configured via `SESSION_SECRET` environment variable.

### D4: API proxy in `hooks.server.ts`

**Decision**: The SvelteKit `handle` hook intercepts all `/api/*` requests. It validates the session, then forwards the request to the appropriate backend service with internal auth headers (`X-Api-Key`, `X-User-Id`, `X-User-Email`, `X-User-Name`, `X-User-Role`).

**Routing rules**:
- `/api/customers/*` → `CUSTOMER_SERVICE_URL` (default `http://localhost:5000`)
- `/api/*` (everything else) → `RISK_MANAGEMENT_API_URL` (default `http://localhost:5227`)

**Rationale**: Centralizes routing logic. Backend services never receive unauthenticated browser requests. Same routing logic that was previously in Vite proxy config.

### D5: Hybrid auth — JWT Bearer for user requests, API key for service-to-service

**Decision**: Replace the existing `ApiKeyAuthMiddleware` with a hybrid `InternalAuthMiddleware` in SharedKernel:

1. **All `/api/*` routes**: Validate `X-Api-Key` header (identifies BFF as trusted caller)
2. **User requests** (`/api/*` except `/api/internal/*`): BFF forwards the OIDC `access_token` as `Authorization: Bearer` header. Backend validates JWT signature via Keycloak JWKS, extracts claims including roles.
3. **Service-to-service** (`/api/internal/*`): API key only, no user context needed.
4. **Dev/test fallback**: When no Bearer token is present and the environment is non-production, accept `X-User-Id`, `X-User-Email`, `X-User-Name`, `X-User-Role` headers as fallback (for E2E tests without running IdP).

**Middleware pipeline**: `ExceptionHandling → Authentication (JWT Bearer) → InternalAuthMiddleware (API key + routing) → Authorization → Controllers`

**Alternatives considered**:
- *API key + user headers only*: Simpler but a leaked API key allows full identity spoofing with no revocation mechanism.
- *mTLS*: Too heavy for dev environment

**Rationale**: JWT tokens are cryptographically signed (not forgeable), time-limited (auto-expire), and revocable via IdP. The API key adds defense-in-depth but is no longer the sole security mechanism. Dev/test fallback avoids requiring a running IdP for E2E tests.

### D6: OIDC via `openid-client` library

**Decision**: Use the `openid-client` npm package for the OIDC Authorization Code + PKCE flow in SvelteKit server routes.

**Flow**:
1. `/auth/login` → Generate PKCE verifier, state, nonce; store in short-lived cookies; redirect to IdP
2. `/auth/callback` → Exchange code for tokens, validate ID token, extract roles from access token, create encrypted session cookie, redirect to `returnTo` or `/`
3. `/auth/logout` → Clear session cookie, redirect to IdP end_session_endpoint

**Rationale**: `openid-client` is the standard Node.js OIDC library, well-maintained, handles discovery and token validation.

### D7: Test session endpoint in SvelteKit

**Decision**: Create `/api/test/session` as a SvelteKit server route (`+server.ts`) that creates session cookies directly without OIDC flow. Only available when `NODE_ENV === 'development'` or `TEST=true`.

**Rationale**: E2E tests need to create sessions without a running IdP. Mirrors the existing `TestSessionController` behavior but in the BFF layer.

## Risks / Trade-offs

- **[Node.js runtime required in production]** → Previously a static SPA could be served by any web server. Now requires Node.js. Mitigation: `adapter-node` produces a standalone `build/` directory with minimal dependencies.
- **[Single point of failure]** → BFF becomes the gateway for all traffic. Mitigation: SvelteKit is lightweight, stateless (session in cookie). Can be horizontally scaled.
- **[Internal headers spoofable if network exposed]** → If backend services are exposed to the internet, anyone could set `X-User-*` headers. Mitigation: Backend services must only be reachable from the BFF (network segmentation + API key).
- **[Session size in cookie]** → Limited to ~4KB per cookie. Mitigation: Only essential data stored (user ID, email, name, role, id_token reference). Well under limit.
- **[openid-client API stability]** → v6 has a new API vs v5. Mitigation: Pin version, follow migration guide if needed.

## Migration Plan

1. **Phase 1 — Frontend BFF**: Install dependencies, create auth routes, hooks.server.ts, session encryption, API proxy. Remove `ssr = false`, switch adapter.
2. **Phase 2 — Backend Internal Auth**: Create `InternalAuthMiddleware` in SharedKernel. Remove OIDC/cookie auth from both C# services. Remove `AuthController`, DataProtection setup.
3. **Phase 3 — Test Adaptation**: Move test session endpoint to SvelteKit. Update E2E helpers. Verify all tests pass.
4. **Rollback**: Revert to previous commit. Both old (OIDC in backend) and new (BFF) approaches are self-contained.
