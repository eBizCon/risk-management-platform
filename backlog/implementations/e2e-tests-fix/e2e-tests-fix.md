# E2E-Tests ohne OIDC-Provider

## User Story

**Als** Entwickler  
**möchte ich** die E2E-Tests ohne OIDC-Provider ausführen können  
**damit** ich automatisierte Tests der Anwendungslogik lokal und in der CI/CD-Pipeline durchführen kann

---

## Akzeptanzkriterien

**Szenario 1: E2E-Tests mit Applicant-Rolle ausführen**  
- **Given** ich führe die E2E-Tests aus `e2e/applicant.test.ts` aus  
- **And** kein OIDC-Provider ist verfügbar  
- **When** die Tests eine Session für die Rolle "applicant" benötigen  
- **Then** werden die Sessions über Test-Helper-Funktionen erstellt  
- **And** alle Tests laufen erfolgreich durch  

**Szenario 2: E2E-Tests mit Processor-Rolle ausführen**  
- **Given** ich führe die E2E-Tests aus `e2e/processor.test.ts` aus  
- **And** kein OIDC-Provider ist verfügbar  
- **When** die Tests zwischen Rollen wechseln müssen  
- **Then** werden für jede Rolle separate Sessions erstellt  
- **And** alle Tests laufen erfolgreich durch  

**Szenario 3: Test-Isolation**  
- **Given** mehrere E2E-Tests laufen parallel oder sequenziell  
- **When** jeder Test eine Session erstellt  
- **Then** beeinflussen sich die Sessions nicht gegenseitig  
- **And** jeder Test startet mit einem sauberen Session-Zustand  

---

## Business Rules

- Session-Erstellung muss ohne OIDC-Provider funktionieren
- Die Test-Helper-Funktionen sollen nur im Test-Kontext verfügbar sein
- Bestehende E2E-Tests sollen minimal angepasst werden
- Die produktive OIDC-Authentifizierung bleibt unverändert

---

# Implementierungsplan

## 1) Implementierungsziel

Die E2E-Tests sollen ohne OIDC-Provider funktionieren, indem Test-Helper-Funktionen Sessions direkt erstellen. Die Helper nutzen die bestehende Session-Infrastruktur und werden in Playwright-Fixtures integriert, um Test-Isolation zu gewährleisten.

## 2) Annahmen & offene Fragen

**Annahmen:**
- Playwright läuft gegen den Preview-Server (Port 4173) wie in `playwright.config.ts` definiert
- Die Session-Map in `session.ts` ist im Preview-Modus für Test-Zugriffe erreichbar
- Test-User benötigen nur minimale User-Daten (id, name, role)

**Keine offenen Fragen** - alle kritischen Informationen liegen vor

## 3) Impact Map

**Layer/Module betroffen:**
- Test-Infrastruktur (Playwright Setup)
- Session-Service (neue Test-Export-Funktion)

**Neue Komponenten:**
- `e2e/helpers/auth.ts` - Test-Helper für Session-Erstellung
- `e2e/fixtures.ts` - Playwright Fixtures für Session-Management

**Geänderte Komponenten:**
- `src/lib/server/services/auth/session.ts` - Export von `clearSessions` für Tests
- `e2e/applicant.test.ts` - Verwendung neuer Fixtures statt Cookie-Manipulation
- `e2e/processor.test.ts` - Verwendung neuer Fixtures statt Cookie-Manipulation
- `playwright.config.ts` - Import und Verwendung von Test-Fixtures

**Nicht betroffen:**
- Produktive OIDC-Authentifizierung (`src/routes/login`, `src/routes/auth/callback`)
- Hooks (`src/hooks.server.ts`) - keine Änderungen
- Alle anderen Routes und Services

## 4) Änderungsplan auf Code-Ebene

### 4.1 Datei: `e2e/helpers/auth.ts` (NEU)

**Art:** neu  
**Verantwortlichkeit:** Erstellt Test-Sessions über API-Endpoint, setzt Session-Cookie im Browser-Kontext

**Methodensignaturen:**
```typescript
export async function createTestSession(
  page: Page,
  role: 'applicant' | 'processor',
  userData?: Partial<{ id: string; name: string }>
): Promise<void>

export async function clearTestSessions(page: Page): Promise<void>
```

**Logik:**
```
createTestSession:
  1. userData mit Defaults mergen (id: random UUID, name: Test User {role})
  2. POST request an /api/test/session mit { role, id, name }
  3. Extrahiere session-Cookie aus Response
  4. Setze Cookie im page.context()
  5. Return

clearTestSessions:
  1. DELETE request an /api/test/session
  2. Lösche alle Session-Cookies aus page.context()
  3. Return
```

**Error Cases:**
- API-Endpoint nicht erreichbar → aussagekräftige Fehlermeldung
- Session-Cookie nicht in Response → Error werfen

---

### 4.2 Datei: `e2e/fixtures.ts` (NEU)

**Art:** neu  
**Verantwortlichkeit:** Erweitert Playwright Test mit Custom Fixtures, garantiert Test-Isolation

**Methodensignaturen:**
```typescript
type TestFixtures = {
  authenticatedPage: Page;
  authenticatedContext: BrowserContext;
  userRole: 'applicant' | 'processor';
};

export const test = base.extend<TestFixtures>({ ... })
```

**Logik:**
```
userRole fixture:
  - Default: 'applicant'
  - Kann pro Test überschrieben werden

authenticatedPage fixture:
  1. Hole userRole aus fixture
  2. Erstelle neue Page
  3. Rufe createTestSession(page, userRole)
  4. Yield page
  5. Nach Test: clearTestSessions(page)
  6. Close page

authenticatedContext fixture:
  1. Hole userRole aus fixture
  2. Erstelle neuen BrowserContext
  3. Erstelle Page aus context
  4. Rufe createTestSession(page, userRole)
  5. Yield context
  6. Nach Test: clearTestSessions(page aus context)
  7. Close context
```

**Error Cases:**
- Cleanup schlägt fehl → trotzdem Context/Page schließen (try/finally)

---

### 4.3 Datei: `src/routes/api/test/session/+server.ts` (NEU)

**Art:** neu  
**Verantwortlichkeit:** Test-Only Endpoint zum Erstellen/Löschen von Sessions, nur im Dev/Test-Modus aktiv

**Methodensignaturen:**
```typescript
export async function POST({ request, cookies }): Promise<Response>
export async function DELETE({ cookies }): Promise<Response>
```

**Logik:**
```
POST:
  1. Prüfe ob dev-Mode oder TEST env variable
  2. Falls nicht: return 404
  3. Parse request.json() → { role, id, name }
  4. Validiere mit Zod: role in ['applicant', 'processor'], id/name sind strings
  5. Falls invalid: return 400 mit Fehlermeldung
  6. Erstelle user object: { id, name, role }
  7. Rufe createSession(cookies, user)
  8. Return 200 mit { sessionId }

DELETE:
  1. Prüfe ob dev-Mode oder TEST env variable
  2. Falls nicht: return 404
  3. Hole sessionId aus cookies
  4. Rufe clearSessions() (cleared alle Sessions in der Map)
  5. Rufe deleteSession(cookies, sessionId)
  6. Return 204
```

**Error Cases:**
- Production-Mode → 404 Not Found
- Validierungsfehler → 400 mit Details
- Missing data → 400

---

### 4.4 Datei: `e2e/applicant.test.ts`

**Art:** ändern  
**Änderungen:**
1. Import von `test` und `expect` aus `./fixtures` statt `@playwright/test`
2. Entfernen von `beforeEach` mit Cookie-Manipulation (Zeilen 4-13)
3. Fixture `authenticatedPage` als Parameter verwenden statt `page`

**Beispiel:**
```typescript
// Import
import { test, expect } from './fixtures'

// beforeEach entfernen

// Tests umschreiben
test('should display the home page', async ({ authenticatedPage }) => {
  await authenticatedPage.goto('/')
  await expect(authenticatedPage.locator('h1')).toContainText('Risikomanagement')
})
```

---

### 4.5 Datei: `e2e/processor.test.ts`

**Art:** ändern  
**Änderungen:**
1. Import von `test` und `expect` aus `./fixtures` statt `@playwright/test`
2. `beforeEach` mit Cookie-Manipulation entfernen (Zeilen 4-12)
3. Für Tests mit Rollenwechsel: `createTestSession` manuell aufrufen

**Spezialfall - Rollenwechsel:**
```typescript
import { test, expect } from './fixtures'
import { createTestSession } from './helpers/auth'

test('should switch between roles', async ({ page }) => {
  // Applicant role
  await createTestSession(page, 'applicant')
  await page.goto('/applications/new')
  // ... create application
  
  // Switch to processor
  await createTestSession(page, 'processor')
  await page.goto('/processor')
  // ... processor actions
})
```

---

### 4.6 Datei: `src/lib/server/services/auth/session.ts`

**Art:** keine Änderung notwendig  
**Begründung:** Export von `clearSessions` existiert bereits (Zeile 60-62)

---

## 5) API Contracts

### POST /api/test/session

**Request Body:**
```typescript
{
  role: 'applicant' | 'processor',
  id: string,
  name: string
}
```

**Response 200:**
```typescript
{
  sessionId: string
}
```

**Response 400:**
```typescript
{
  error: string
}
```

**Response 404:** (Production mode)
```typescript
{
  error: 'Not found'
}
```

### DELETE /api/test/session

**Response 204:** No Content  
**Response 404:** (Production mode)

---

## 6) Testplan

### Unit Tests

**Testdatei:** `src/routes/api/test/session/+server.test.ts` (NEU, optional)

**Testfälle:**
1. POST erstellt Session mit gültigen Daten
2. POST validiert Eingabedaten
3. POST nicht verfügbar in Production
4. DELETE löscht Sessions

### E2E Tests

**Testdateien:** `e2e/applicant.test.ts`, `e2e/processor.test.ts`

**Kritische Journeys:**
1. Applicant kann Antrag erstellen (mit Session-Auth)
2. Processor kann Anträge bearbeiten (mit Session-Auth)
3. Rollenwechsel zwischen Tests (Isolation)
4. Session-Cleanup zwischen Tests

---

## 7) Risiken & Abhängigkeiten

**Technische Risiken:**
1. **Session-Map ist in-memory**
   - Risk: Bei Server-Restart gehen Sessions verloren
   - Mitigation: In Tests akzeptabel, da jeder Test-Lauf neu startet
   
2. **Race Conditions bei parallel laufenden Tests**
   - Risk: `clearSessions()` löscht ALLE Sessions
   - Mitigation: Playwright workers sind isoliert, jeder hat eigenen Browser-Context

3. **Test-Endpoint in Production**
   - Risk: Sicherheitslücke wenn nicht deaktiviert
   - Mitigation: Strikte Check auf `dev` oder `TEST` env variable

**Abhängigkeiten:**
- SvelteKit Preview-Server muss laufen (bereits in `playwright.config.ts` konfiguriert)

---

## 8) Ausführungsschritte

1. Erstelle Test-API-Endpoint (`src/routes/api/test/session/+server.ts`)
2. Erstelle Auth-Helper (`e2e/helpers/auth.ts`)
3. Erstelle Fixtures (`e2e/fixtures.ts`)
4. Passe `applicant.test.ts` an (Import + Fixture-Verwendung)
5. Passe `processor.test.ts` an (Import + Fixture-Verwendung)
6. Unit-Tests für API-Endpoint schreiben (optional, empfohlen)
7. E2E-Tests ausführen: `npm run test:e2e`
8. Bei Fehlern: Logs prüfen, Session-Erstellung debuggen, erneut ausführen

---

## 9) Verifikation

```bash
# E2E Tests lokal ausführen
npm run test:e2e

# E2E Tests mit UI ausführen
npm run test:e2e:ui

# Alle Tests ausführen
npm run test:all
```

**Erwartetes Ergebnis:**
- Alle bestehenden E2E-Tests laufen erfolgreich durch
- Keine OIDC-Provider-Abhängigkeit mehr
- Test-Isolation ist gewährleistet
