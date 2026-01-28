# OIDC Auth – Implementierungs-Blueprint

## Kontext
Diese Planung implementiert Anmeldung via lokalem OIDC Provider inkl. rollenbasierter Autorisierung (Source of Truth: Provider) und SSO Logout.

### Rollen
- `applicant`
- `processor`

### Schutzumfang
- Geschützt ist alles **außer** Startseite (`/`) und Login (`/login`) sowie technische Auth-Routen (`/auth/callback`, `/logout`).

### UX/Behavior Vorgaben
- **API Calls:** niemals automatisch redirecten; stattdessen `401/403`.
- **Frontend Pages:** keine automatische Weiterleitung zum Login; stattdessen verständliche Meldung (z.B. über `+error.svelte`) und Link zur Login-Seite.
- **Keine Berechtigung:** Anzeige „Keine Berechtigung“ + Link zu Login.
- Rollenänderungen werden **beim nächsten Login** wirksam.
- Logout muss **zusätzlich beim Provider** passieren (RP-initiated logout via `end_session_endpoint`).

---

## 1) Implementierungsziel
- OIDC Login (Authorization Code + PKCE) integrieren.
- Serverseitige Session etablieren und den eingeloggten Nutzer in `event.locals.user` bereitstellen.
- Autorisierung auf Basis der Provider-Rolle für Pages und API-Routen erzwingen.
- Logout in App + OIDC Provider umsetzen.

---

## 2) Annahmen & offene Punkte
### Offene Punkte
- Rollen-Claim (Pfad im Token/UserInfo) ist noch nicht final.
- Issuer/Client Details sind noch nicht final und werden per ENV gesetzt.

### Design-Entscheid
- Rollen-Claim Mapping wird **konfigurierbar** (ENV `OIDC_ROLES_CLAIM_PATH`).

---

## 3) Impact Map (Was ändert sich wo?)
### Betroffene Layer/Module
- SvelteKit Transport Layer:
  - `src/hooks.server.ts` (AuthN/AuthZ zentral)
  - Auth Routes (`/login`, `/auth/callback`, `/logout`)
  - Fehler-/No-Access Darstellung (`+error.svelte` und/oder `/no-access`)
- Server Services:
  - `src/lib/server/services/auth/oidc.ts` (OIDC Discovery, Auth URL, Token Exchange, Logout URL)
  - `src/lib/server/services/auth/session.ts` (Session Cookie + Session Store)

### Neu
- `src/hooks.server.ts`
- `src/lib/server/services/auth/oidc.ts`
- `src/lib/server/services/auth/session.ts`
- `src/routes/login/+page.svelte`
- `src/routes/login/+server.ts`
- `src/routes/auth/callback/+server.ts`
- `src/routes/logout/+server.ts`
- `src/routes/+error.svelte` (für 401/403 Meldungen)

### Ändern
- `src/app.d.ts` (Typen für `locals.user`)
- `src/routes/+layout.svelte` (Login/Logout Anzeige, User-Infos)
- `src/routes/+layout.server.ts` (liefert `user` aus `locals`)
- `src/routes/**/+page.server.ts` (statt Cookie-Fallbacks `locals.user` nutzen)
- `src/routes/api/**/+server.ts` (AuthZ für API; 401/403 statt Redirect)

### Bewusst ausgeschlossen
- Persistenter Session Store (Redis/DB) – initial In-Memory.
- Token Refresh/Long-lived Sessions – initial statische TTL.

---

## 4) Änderungsplan auf Code-Ebene (Developer To-Do)

### A) `src/app.d.ts`
- **Art:** ändern
- **Ziel:** `event.locals.user` typisieren.

**Vorschlag Typ:**
- `id: string`
- `name: string`
- `role: 'applicant' | 'processor'`
- optional `idToken: string` (für SSO logout)

---

### B) Session Layer
#### Datei
- `src/lib/server/services/auth/session.ts` (neu)

#### Verantwortlichkeit
- Session-ID erstellen, in httpOnly Cookie setzen
- Session lesen/löschen

#### Pseudocode
- `createSession(cookies, user)`:
  - `sessionId = crypto.randomUUID()`
  - store `sessionId -> { user, expiresAt }`
  - `cookies.set('session', sessionId, { httpOnly: true, sameSite: 'lax', path: '/', secure: prod, maxAge })`
- `getSession(sessionId)`:
  - lookup, TTL prüfen
- `deleteSession(cookies, sessionId)`:
  - remove store
  - `cookies.delete('session', { path: '/' })`

#### Edge Cases
- Abgelaufene Session => entfernen und als nicht eingeloggt behandeln.

---

### C) OIDC Layer (openid-client)
#### Datei
- `src/lib/server/services/auth/oidc.ts` (neu)

#### Abhängigkeit
- `openid-client` (panva)

#### ENV
- `OIDC_ISSUER`
- `OIDC_CLIENT_ID`
- optional `OIDC_CLIENT_SECRET`
- `OIDC_REDIRECT_URI`
- `OIDC_POST_LOGOUT_REDIRECT_URI`
- `OIDC_SCOPE` (z.B. `openid profile`)
- `OIDC_ROLES_CLAIM_PATH` (z.B. `roles` oder `realm_access.roles`)

#### Funktionen (Signaturen)
- `getOidcConfig(): Promise<Configuration>`
- `createAuthorizationUrl(): Promise<{ url: URL; codeVerifier: string; state?: string }>`
- `exchangeCodeForTokens(params): Promise<{ idToken: string; accessToken?: string; claims: { sub: string; name?: string; roles: string[] } }>`
- `createEndSessionUrl(params): Promise<URL>`

#### Pseudocode
- Discovery:
  - `config = await client.discovery(new URL(OIDC_ISSUER), OIDC_CLIENT_ID, ...)`
- Login URL:
  - `codeVerifier = client.randomPKCECodeVerifier()`
  - `codeChallenge = await client.calculatePKCECodeChallenge(codeVerifier)`
  - optional `state = client.randomState()`
  - `url = client.buildAuthorizationUrl(config, { redirect_uri, scope, code_challenge, code_challenge_method: 'S256', state })`
- Callback:
  - `tokens = await client.authorizationCodeGrant(config, request, { pkceCodeVerifier: codeVerifier, expectedState: state })`
  - `idToken = tokens.id_token`
  - optional `userinfo = await client.fetchUserInfo(config, tokens.access_token)`
  - `roles = extractRolesFromClaims(claims, OIDC_ROLES_CLAIM_PATH)`
- Logout:
  - `url = client.buildEndSessionUrl(config, { post_logout_redirect_uri, id_token_hint: idToken })`

#### Role Extraction
- `extractRolesFromClaims(claims, path)`:
  - dot-path traversal
  - normalize to `string[]`
  - filter to allowed roles (`applicant`, `processor`)

---

### D) SvelteKit Hook (AuthN/AuthZ)
#### Datei
- `src/hooks.server.ts` (neu)

#### Verantwortlichkeit
- `locals.user` setzen
- Guards für Pages und API

#### Regeln
- **Public:** `/`, `/login`, `/auth/callback`, `/logout` (und ggf. statische assets)
- **Protected:** alles andere

#### Verhalten
- **API (`/api/**`):**
  - not logged in => `401`
  - wrong role => `403`
  - niemals redirect
- **Pages (alles andere):**
  - not logged in => `throw error(401, 'Login erforderlich')`
  - wrong role => `throw error(403, 'Keine Berechtigung')`
  - niemals redirect zum Login

---

### E) Login / Callback / Logout Routes
#### E1) Login UI
- `src/routes/login/+page.svelte` (neu)
- Button mit `data-testid="auth-login-button"`

#### E2) Login Redirect Endpoint
- `src/routes/login/+server.ts` (neu)
- erstellt Auth URL, setzt kurzlebige Cookies:
  - `pkce_verifier` (httpOnly, maxAge ~300s)
  - `oidc_state` (httpOnly, maxAge ~300s)
  - optional `returnTo`
- redirect 303 zum Provider

#### E3) Callback Endpoint
- `src/routes/auth/callback/+server.ts` (neu)
- liest verifier/state, tauscht code gegen tokens
- validiert Rollen:
  - keine gültige Rolle => `throw error(403, 'Keine Berechtigung')`
- erstellt Session, löscht kurzlebige Cookies
- redirect 303 zurück nach `/` oder `returnTo`

#### E4) Logout Endpoint
- `src/routes/logout/+server.ts` (neu)
- löscht Session
- erstellt Provider logout URL via `buildEndSessionUrl` (id_token_hint)
- redirect 303 zum Provider logout

---

### F) Fehlerdarstellung
#### Datei
- `src/routes/+error.svelte` (neu)

#### Verhalten
- wenn status `401`: „Login erforderlich“ + Link `/login`
- wenn status `403`: „Keine Berechtigung“ + Link `/login`

#### TestIDs
- `auth-error-unauthorized`
- `auth-error-forbidden`
- `auth-error-login-link`

---

### G) Layout Anpassung (Login/Logout Anzeige)
#### Dateien
- `src/routes/+layout.server.ts` (neu): return `{ user: locals.user ?? null }`
- `src/routes/+layout.svelte` (ändern):
  - zeigt Login-Link wenn `!user`
  - zeigt User-Info + Logout-Link wenn `user`

---

### H) Anpassung bestehender Server Loads/Actions & API
- Ersetze Cookie-Fallbacks wie `cookies.get('userId') || 'applicant-1'` durch `locals.user.id`
- Ergänze AuthZ in API routes (oder verlasse dich auf Hook):
  - API liefert `401/403` (JSON)

---

## 5) Testplan
### Unit (Vitest)
- `session.test.ts`: create/get/delete/ttl
- `oidc.test.ts`: role extraction, end-session url building (mock)

### E2E (Playwright)
Aktuell nutzen Tests Cookie `risk-management-user-role`. Das wird ersetzt.
- Neue Tests nutzen `data-testid`:
  - Login Button
  - Fehleranzeigen (401/403) + Login-Link

**Hinweis:** Echter OIDC Flow im E2E erfordert lokalen Provider im Test-Setup. Alternativ: für E2E einen testbaren Stub (nur falls fachlich akzeptiert).

---

## 6) Risiken & Abhängigkeiten
- Provider-Konfiguration (Issuer, Client, Redirect URIs)
- Rollen-Claim Mapping
- In-memory Sessions (nicht skalierbar, aber OK für lokale/Single-Instance)
