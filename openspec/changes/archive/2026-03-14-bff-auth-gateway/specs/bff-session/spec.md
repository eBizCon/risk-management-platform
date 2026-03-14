## ADDED Requirements

### Requirement: SvelteKit adapter-node configuration
The frontend SHALL use `@sveltejs/adapter-node` instead of `@sveltejs/adapter-static`. The `svelte.config.js` SHALL configure the node adapter. The root layout SHALL NOT set `ssr = false`.

#### Scenario: SvelteKit runs as Node.js server
- **WHEN** the frontend is built and started
- **THEN** a Node.js HTTP server SHALL serve the application with server-side capabilities

#### Scenario: Static adapter removed
- **WHEN** `svelte.config.js` is inspected
- **THEN** it SHALL reference `@sveltejs/adapter-node` and NOT `@sveltejs/adapter-static`

### Requirement: Encrypted session cookie
The SvelteKit server SHALL manage user sessions via an AES-256-GCM encrypted httpOnly cookie named `session`. The encryption key SHALL be derived from a `SESSION_SECRET` environment variable (minimum 32 characters). The cookie SHALL have `SameSite=Lax`, `HttpOnly=true`, and `Secure=true` in production (`Secure=false` in development). The session SHALL expire after 1 hour with sliding expiration.

#### Scenario: Session cookie is encrypted and httpOnly
- **WHEN** a user logs in successfully
- **THEN** the server SHALL set a cookie named `session` that is httpOnly, encrypted, and contains user identity data

#### Scenario: Session cookie cannot be read by client-side JavaScript
- **WHEN** client-side JavaScript attempts to read `document.cookie` for the `session` cookie
- **THEN** the cookie SHALL NOT be visible due to httpOnly flag

#### Scenario: Missing SESSION_SECRET prevents startup
- **WHEN** the `SESSION_SECRET` environment variable is not set or is shorter than 32 characters
- **THEN** the server SHALL fail to start with a descriptive error message

### Requirement: OIDC login flow via SvelteKit server routes
The SvelteKit server SHALL implement the OIDC Authorization Code + PKCE flow via server routes. The login route SHALL be `/auth/login`. It SHALL generate a PKCE code verifier, state parameter, and nonce, store them in short-lived httpOnly cookies, and redirect the browser to the IdP authorization endpoint.

#### Scenario: User initiates login
- **WHEN** a user navigates to `/auth/login`
- **THEN** the server SHALL redirect to the IdP authorization endpoint with `response_type=code`, PKCE `code_challenge`, `state`, and `nonce` parameters

#### Scenario: Login with returnTo parameter
- **WHEN** a user navigates to `/auth/login?returnTo=/applications`
- **THEN** after successful authentication the user SHALL be redirected to `/applications`

#### Scenario: returnTo rejects absolute URLs
- **WHEN** a user navigates to `/auth/login?returnTo=https://evil.com`
- **THEN** the server SHALL ignore the returnTo and redirect to `/`

### Requirement: OIDC callback handling
The SvelteKit server SHALL implement `/auth/callback` that exchanges the authorization code for tokens, validates the ID token, extracts user identity and roles from the access token, creates an encrypted session cookie, and redirects to the stored returnTo path or `/`.

#### Scenario: Successful callback creates session
- **WHEN** the IdP redirects to `/auth/callback` with a valid authorization code
- **THEN** the server SHALL exchange the code for tokens, create a session cookie with user identity, and redirect to `/`

#### Scenario: Role extraction from access token
- **WHEN** the access token contains roles at the configured claim path
- **THEN** the server SHALL extract the first matching allowed role (`applicant`, `processor`, `risk_manager`) and include it in the session

#### Scenario: Invalid state parameter rejected
- **WHEN** the callback `state` parameter does not match the stored state
- **THEN** the server SHALL reject the request and redirect to `/login` with an error

### Requirement: OIDC logout flow
The SvelteKit server SHALL implement `/auth/logout` that clears the session cookie and redirects to the IdP end_session_endpoint with the `id_token_hint` and `post_logout_redirect_uri`.

#### Scenario: User logs out
- **WHEN** a user navigates to `/auth/logout`
- **THEN** the session cookie SHALL be cleared and the browser SHALL be redirected to the IdP logout endpoint

#### Scenario: Logout without active session
- **WHEN** a user navigates to `/auth/logout` without an active session
- **THEN** the server SHALL redirect to `/` without error

### Requirement: Server-side user data in layout
The root layout SHALL be a server layout (`+layout.server.ts`) that reads the user from `event.locals.user` (set by `hooks.server.ts`) and passes it to the client as page data.

#### Scenario: Authenticated user data available in layout
- **WHEN** an authenticated user loads any page
- **THEN** the layout data SHALL contain the user object with `id`, `email`, `name`, and `role`

#### Scenario: Unauthenticated user gets null
- **WHEN** an unauthenticated user loads any page
- **THEN** the layout data SHALL contain `user: null`

### Requirement: API proxy in hooks.server.ts
The SvelteKit `handle` hook SHALL intercept all requests to `/api/*` paths. For authenticated requests, it SHALL forward the request to the appropriate backend service with internal auth headers (`X-Api-Key`, `X-User-Id`, `X-User-Email`, `X-User-Name`, `X-User-Role`). For unauthenticated requests to `/api/*`, it SHALL return HTTP 401.

#### Scenario: Authenticated API call is proxied with headers
- **WHEN** an authenticated user makes a request to `/api/applications`
- **THEN** the hook SHALL forward the request to `RISK_MANAGEMENT_API_URL` with `X-Api-Key`, `X-User-Id`, `X-User-Email`, `X-User-Name`, `X-User-Role` headers

#### Scenario: Customer API calls routed to customer service
- **WHEN** an authenticated user makes a request to `/api/customers/5`
- **THEN** the hook SHALL forward the request to `CUSTOMER_SERVICE_URL`

#### Scenario: Unauthenticated API call returns 401
- **WHEN** an unauthenticated user makes a request to `/api/applications`
- **THEN** the hook SHALL return HTTP 401 with `{"error": "Login erforderlich"}`

#### Scenario: Request body and method forwarded correctly
- **WHEN** an authenticated user makes a POST request to `/api/applications` with a JSON body
- **THEN** the hook SHALL forward the same HTTP method, headers (except cookie), and body to the backend

### Requirement: Vite proxy configuration removed
The `vite.config.ts` SHALL NOT contain proxy configuration for `/api/*`, `/login`, `/logout`, or `/auth` paths. SvelteKit server handles all routing.

#### Scenario: No Vite proxy in dev mode
- **WHEN** `vite.config.ts` is inspected
- **THEN** the `server.proxy` and `preview.proxy` sections SHALL NOT exist

### Requirement: Login page redirects to OIDC
The login page at `/login` SHALL contain a form/link that directs to `/auth/login` to initiate the OIDC flow. The logout link in the navigation SHALL point to `/auth/logout`.

#### Scenario: Login button initiates OIDC flow
- **WHEN** a user clicks the login button on `/login`
- **THEN** the browser SHALL navigate to `/auth/login` which initiates the OIDC redirect

#### Scenario: Logout link triggers OIDC logout
- **WHEN** a user clicks logout in the navigation
- **THEN** the browser SHALL navigate to `/auth/logout`

### Requirement: OIDC configuration via environment variables
The OIDC configuration SHALL be read from environment variables: `OIDC_ISSUER`, `OIDC_CLIENT_ID`, `OIDC_CLIENT_SECRET`, `OIDC_SCOPE`, `OIDC_ROLES_CLAIM_PATH`, `OIDC_POST_LOGOUT_REDIRECT_URI`, `OIDC_REDIRECT_URI`. The `OIDC_CALLBACK_PATH` SHALL default to `/auth/callback`.

#### Scenario: OIDC discovery from issuer
- **WHEN** the server starts with `OIDC_ISSUER` set to a valid OpenID Connect provider
- **THEN** the server SHALL discover the authorization, token, and end_session endpoints from the `.well-known/openid-configuration`

### Requirement: Test session endpoint in SvelteKit
The SvelteKit server SHALL provide a `/api/test/session` POST endpoint that creates a session cookie directly without OIDC flow. This endpoint SHALL only be available when `NODE_ENV === 'development'` or the `TEST` environment variable is `'true'`. A DELETE endpoint at the same path SHALL clear the session. The POST endpoint SHALL accept `{id, email, name, role}` in the request body and return `{sessionId}`.

#### Scenario: E2E test creates session
- **WHEN** a POST request is made to `/api/test/session` with `{id: "test-1", email: "test@example.com", name: "Test User", role: "applicant"}` in development mode
- **THEN** the server SHALL return 200 with `{sessionId: "..."}` and set an encrypted session cookie

#### Scenario: E2E test clears session
- **WHEN** a DELETE request is made to `/api/test/session` in development mode
- **THEN** the server SHALL clear the session cookie and return 204

#### Scenario: Test endpoint disabled in production
- **WHEN** a POST request is made to `/api/test/session` in production mode
- **THEN** the server SHALL return 404

### Requirement: app.d.ts Locals interface
The `App.Locals` interface in `app.d.ts` SHALL include an optional `user` property of type `App.User` so that `event.locals.user` is typed throughout the application.

#### Scenario: Typed locals in server hooks
- **WHEN** `hooks.server.ts` sets `event.locals.user`
- **THEN** TypeScript SHALL recognize the `user` property without type errors
