# CSV-Export: Implementierungs-Blueprint

## 1) Implementierungsziel

Server-side CSV-Export für die Processor-Antragsliste mit Status-Filter-Unterstützung. Der Export wird über einen Button in der Filter-Leiste ausgelöst und liefert alle gefilterten Anträge (ohne Pagination) als CSV-Download.

## 2) Annahmen & offene Fragen

**Annahmen:**
- Keine Performance-Limits für Export (alle Daten auf einmal)
- UTF-8 mit BOM reicht für Excel-Kompatibilität
- Semicolon (`;`) als Trennzeichen für deutsche Excel-Versionen

**Keine offenen Fragen** - Design ist vollständig abgestimmt.

## 3) Impact Map

### Layer/Module betroffen:
- **API Layer**: Neuer Export-Endpoint
- **Service Layer**: Neuer CSV-Service
- **Repository Layer**: Neue Export-Funktion
- **UI Layer**: Button in Processor-Page

### Neue Komponenten:
- `src/routes/api/applications/export/+server.ts`
- `src/lib/server/services/csv-export.service.ts`
- `src/lib/server/services/csv-export.service.test.ts`

### Geänderte Komponenten:
- `src/lib/server/services/repositories/application.repository.ts`
- `src/routes/processor/+page.svelte`

### Nicht betroffen:
- Database Schema (keine Migration nötig)
- Andere API-Endpoints
- Applicant-Views

---

## 4) Änderungsplan auf Code-Ebene

### Task 1: Repository-Erweiterung

**Datei:** `src/lib/server/services/repositories/application.repository.ts`  
**Art:** ändern  
**Betroffene Klasse(n):** -  
**Neue Methodensignatur:**

```typescript
export async function getProcessorApplicationsForExport(params: {
  status?: ApplicationStatus;
}): Promise<Application[]>
```

**Verantwortlichkeit:** Alle Anträge für Export abrufen (ohne Pagination)

**Logik (Pseudocode):**
```
1. whereClause erstellen wenn status vorhanden
2. SELECT * FROM applications WHERE [status] ORDER BY createdAt DESC
3. Return alle Ergebnisse
```

**Error/Edge Cases:**
- Leere Ergebnismenge → leeres Array zurückgeben

**Logging:** Nicht erforderlich (Repository-Ebene)

---

### Task 2: CSV-Export Service

**Datei:** `src/lib/server/services/csv-export.service.ts`  
**Art:** neu  
**Neue Methodensignaturen:**

```typescript
export function generateApplicationsCsv(applications: Application[]): string
```

**Verantwortlichkeit:** CSV-String aus Application-Array generieren

**Logik (Pseudocode):**
```
1. BOM hinzufügen (\uFEFF)
2. Header-Zeile erstellen: id;name;income;fixedCosts;...
3. Für jeden Antrag:
   a. Werte escapen (Anführungszeichen verdoppeln)
   b. Werte in Anführungszeichen setzen wenn nötig
   c. Zeile mit Semicolon verbinden
4. Zeilen mit \r\n verbinden
5. Return CSV-String
```

**CSV-Spalten (Reihenfolge):**
```typescript
const CSV_COLUMNS = [
  'id',
  'name', 
  'income',
  'fixedCosts',
  'desiredRate',
  'employmentStatus',
  'hasPaymentDefault',
  'status',
  'score',
  'trafficLight',
  'processorComment',
  'createdAt',
  'submittedAt',
  'processedAt',
  'createdBy'
] as const;
```

**Hilfsfunktionen:**
```typescript
function escapeCsvValue(value: unknown): string
function formatCsvRow(values: string[]): string
```

**Error/Edge Cases:**
- `null`/`undefined` → leerer String
- Werte mit Semicolon, Anführungszeichen, Zeilenumbruch → escapen
- Leeres Array → nur Header-Zeile

---

### Task 3: API Endpoint

**Datei:** `src/routes/api/applications/export/+server.ts`  
**Art:** neu  
**Neue Handler:**

```typescript
export const GET: RequestHandler = async ({ url, locals }) => { ... }
```

**Verantwortlichkeit:** CSV-Export-Endpoint für Processor

**Logik (Pseudocode):**
```
1. Auth prüfen: locals.user vorhanden?
   - Nein → 401 JSON { error: 'Login erforderlich' }
2. Role prüfen: locals.user.role === 'processor'?
   - Nein → 403 JSON { error: 'Keine Berechtigung' }
3. Status-Parameter aus URL extrahieren
4. Status validieren (allowedStatuses)
5. Repository aufrufen: getProcessorApplicationsForExport({ status })
6. CSV generieren: generateApplicationsCsv(applications)
7. Dateiname generieren: antraege_YYYY-MM-DD_HH-mm.csv
8. Response mit Headers:
   - Content-Type: text/csv; charset=utf-8
   - Content-Disposition: attachment; filename="..."
```

**Error/Edge Cases:**
- Ungültiger Status → ignorieren, alle Daten exportieren
- Keine Daten → CSV nur mit Header

**Logging:** Nicht erforderlich (Standard SvelteKit Logging)

---

### Task 4: UI Button

**Datei:** `src/routes/processor/+page.svelte`  
**Art:** ändern  
**Betroffene Elemente:** Filter-Leiste

**Änderungen:**

1. Import hinzufügen: `Download` von lucide-svelte
2. Export-Funktion erstellen:

```typescript
function handleExport() {
  const url = new URL('/api/applications/export', window.location.origin);
  const status = $page.url.searchParams.get('status');
  if (status) {
    url.searchParams.set('status', status);
  }
  window.location.href = url.toString();
}
```

3. Button in Filter-Leiste einfügen (nach "X Anträge gefunden"):

```svelte
<button
  onclick={handleExport}
  class="btn btn-secondary btn-sm inline-flex items-center gap-2"
  data-testid="processor-csv-export-button"
>
  <Download class="w-4 h-4" />
  <span class="hidden sm:inline">CSV Export</span>
</button>
```

**Error/Edge Cases:**
- Download-Fehler → Browser zeigt Fehler (kein spezielles Handling nötig)

---

## 5) Daten- & Contract-Änderungen

### DB/Entity Änderungen
Keine

### Migration(en)
Keine

### API Contract

**Request:**
```
GET /api/applications/export?status=submitted
```

| Parameter | Typ | Optional | Beschreibung |
|-----------|-----|----------|--------------|
| status | string | ja | Filter: draft, submitted, approved, rejected |

**Response (Success):**
```
HTTP/1.1 200 OK
Content-Type: text/csv; charset=utf-8
Content-Disposition: attachment; filename="antraege_2026-01-30_11-17.csv"

<BOM>id;name;income;...
1;"Max Müller";4500;...
```

**Response (Error):**
```json
// 401
{ "error": "Login erforderlich" }

// 403
{ "error": "Keine Berechtigung" }
```

### Rückwärtskompatibilität
Neuer Endpoint, keine Breaking Changes

---

## 6) Testplan

### Unit Tests

**Datei:** `src/lib/server/services/csv-export.service.test.ts`

| Testfall | Given | When | Then |
|----------|-------|------|------|
| Header korrekt | - | generateApplicationsCsv([]) | Header mit allen 15 Spalten |
| BOM vorhanden | - | generateApplicationsCsv([]) | Startet mit \uFEFF |
| Semicolon-Escaping | Wert enthält ";" | escapeCsvValue("a;b") | "\"a;b\"" |
| Anführungszeichen-Escaping | Wert enthält "\"" | escapeCsvValue("a\"b") | "\"a\"\"b\"" |
| Zeilenumbruch-Escaping | Wert enthält "\n" | escapeCsvValue("a\nb") | "\"a\nb\"" |
| Null-Werte | score = null | Zeile generieren | Leerer String |
| Boolean-Werte | hasPaymentDefault = true | Zeile generieren | "true" |
| Mehrere Anträge | 3 Applications | generateApplicationsCsv(apps) | 4 Zeilen (Header + 3 Daten) |

**Pseudocode:**
```typescript
describe('CSV Export Service', () => {
  describe('generateApplicationsCsv', () => {
    it('should include BOM at start', () => {
      const result = generateApplicationsCsv([]);
      expect(result.charCodeAt(0)).toBe(0xFEFF);
    });

    it('should have correct header columns', () => {
      const result = generateApplicationsCsv([]);
      const header = result.split('\r\n')[0].substring(1); // skip BOM
      expect(header).toBe('id;name;income;...');
    });

    it('should escape semicolons in values', () => {
      const apps = [mockApplication({ processorComment: 'test;comment' })];
      const result = generateApplicationsCsv(apps);
      expect(result).toContain('"test;comment"');
    });

    // ... weitere Tests
  });
});
```

---

### E2E Tests

**Datei:** `e2e/processor.test.ts` (erweitern)

| Testfall | Vorbedingung | Aktion | Erwartung |
|----------|--------------|--------|-----------|
| Button sichtbar | Processor eingeloggt | /processor öffnen | Button mit testid sichtbar |
| Button nicht für Applicant | Applicant eingeloggt | /processor öffnen | 403 oder kein Button |
| Download funktioniert | Processor, Anträge vorhanden | Button klicken | Download startet |
| Filter wird berücksichtigt | status=submitted gesetzt | Export | Nur submitted in CSV |

**Pseudocode:**
```typescript
test.describe('CSV Export', () => {
  test.use({ userRole: 'processor' });

  test('should show export button', async ({ authenticatedPage }) => {
    await authenticatedPage.goto('/processor');
    await expect(
      authenticatedPage.getByTestId('processor-csv-export-button')
    ).toBeVisible();
  });

  test('should trigger download on click', async ({ authenticatedPage }) => {
    await authenticatedPage.goto('/processor');
    
    const downloadPromise = authenticatedPage.waitForEvent('download');
    await authenticatedPage.getByTestId('processor-csv-export-button').click();
    const download = await downloadPromise;
    
    expect(download.suggestedFilename()).toMatch(/^antraege_\d{4}-\d{2}-\d{2}_\d{2}-\d{2}\.csv$/);
  });

  test('should include status filter in export', async ({ authenticatedPage }) => {
    await authenticatedPage.goto('/processor?status=submitted');
    
    const downloadPromise = authenticatedPage.waitForEvent('download');
    await authenticatedPage.getByTestId('processor-csv-export-button').click();
    const download = await downloadPromise;
    
    const content = await download.path();
    // Verify CSV content contains only submitted status
  });
});
```

---

## 7) Risiken & Abhängigkeiten

### Technische Risiken

| Risiko | Wahrscheinlichkeit | Impact | Mitigation |
|--------|-------------------|--------|------------|
| Große Datenmengen | Niedrig | Mittel | Streaming-Response möglich, aber YAGNI |
| Excel-Encoding-Probleme | Mittel | Niedrig | BOM + UTF-8 + Tests |

### Abhängigkeiten
- Keine externen Abhängigkeiten
- Keine Team-Abhängigkeiten

### Rollout
- Feature kann sofort deployed werden
- Kein Feature-Flag nötig
- Keine Migration

---

## 8) Ausführungsreihenfolge

1. **Task 1**: Repository-Funktion (`getProcessorApplicationsForExport`)
2. **Task 2**: CSV-Service + Unit Tests
3. **Task 3**: API Endpoint
4. **Task 4**: UI Button
5. **Task 5**: E2E Tests
6. **Verification**: `npm run test` + `npm run test:e2e`

---

## Verifikations-Commands

```bash
# Unit Tests
npm run test -- csv-export

# E2E Tests  
npm run test:e2e -- processor

# Manueller Test
curl -H "Cookie: session=..." "http://localhost:5173/api/applications/export?status=submitted"
```
