# CSV-Export für Processor

## User Story

**Als** Processor  
**möchte ich** alle gefilterten Anträge als CSV-Datei exportieren können  
**damit** ich die kompletten Daten für Reporting, Analyse oder Weitergabe außerhalb des Systems nutzen kann

---

## Akzeptanzkriterien

### Szenario 1: CSV-Export aller Anträge ohne Filter
```gherkin
Given ich bin als Processor eingeloggt
And ich befinde mich auf der Seite "/processor"
And es sind keine Filter aktiv
And es gibt 25 Anträge über 3 Seiten verteilt
When ich auf den CSV-Export-Button im Table-Header klicke
Then wird eine CSV-Datei heruntergeladen
And die Datei enthält alle 25 Anträge (nicht nur die aktuelle Seite)
And die CSV enthält folgende Spalten: ID, Name, Einkommen, Fixkosten, Zahlungsverzug, Gewünschte Rate, Beschäftigungsstatus, Status, Score, Ampel, Kommentar, Erstellt von, Erstellt am, Eingereicht am, Bearbeitet am
And die Datensätze sind absteigend nach Erstelldatum sortiert
```

### Szenario 2: CSV-Export mit beliebigem Status-Filter über mehrere Seiten
```gherkin
Given ich bin als Processor eingeloggt
And ich befinde mich auf der Seite "/processor"
And ich habe einen Status-Filter gesetzt (z.B. "Eingereicht", "Genehmigt" oder "Abgelehnt")
And es werden gefilterte Anträge über mehrere Seiten angezeigt
And ich befinde mich auf einer beliebigen Seite
When ich auf den CSV-Export-Button klicke
Then wird eine CSV-Datei heruntergeladen
And die Datei enthält alle gefilterten Anträge über alle Seiten
And die Datei enthält nur Anträge mit dem gesetzten Status-Filter
And keine Anträge mit anderen Status-Werten sind enthalten
```

### Szenario 3: CSV-Export bei leerer gefilterter Liste
```gherkin
Given ich bin als Processor eingeloggt
And ich befinde mich auf der Seite "/processor"
And ich habe einen Filter gesetzt, der keine Ergebnisse liefert
When ich auf den CSV-Export-Button klicke
Then wird eine CSV-Datei mit nur den Spaltenüberschriften heruntergeladen
And die Datei enthält keine Datenzeilen
```

### Szenario 4: CSV-Format und vollständige Spalten
```gherkin
Given ich habe eine CSV-Datei exportiert
When ich die Datei öffne
Then enthält die CSV folgende 15 Spalten:
  | ID | Name | Einkommen | Fixkosten | Zahlungsverzug | Gewünschte Rate | Beschäftigungsstatus | Status | Score | Ampel | Kommentar | Erstellt von | Erstellt am | Eingereicht am | Bearbeitet am |
And deutsche Umlaute sind korrekt dargestellt (UTF-8 mit BOM)
And numerische Werte verwenden Punkt als Dezimaltrennzeichen
And Datumswerte sind im Format "TT.MM.JJJJ HH:MM"
And Ampel-Werte sind als Text ("Grün", "Gelb", "Rot", "-")
And Status-Werte sind auf Deutsch ("Entwurf", "Eingereicht", "Genehmigt", "Abgelehnt")
And Beschäftigungsstatus ist auf Deutsch ("Angestellt", "Selbstständig", "Arbeitslos", "Ruhestand")
And Zahlungsverzug ist als "Ja"/"Nein" dargestellt
And der Dateiname folgt dem Format "antraege-processor-YYYY-MM-DD-HHMM.csv"
```

### Szenario 5: UI-Button (nur Desktop)
```gherkin
Given ich bin als Processor eingeloggt
And ich befinde mich auf der Seite "/processor" in der Desktop-Ansicht
Then sehe ich im Table-Header-Bereich einen CSV-Export-Button
And der Button hat ein Download-Icon
And der Button hat den data-testid "processor-csv-export-button"
And wenn ich die Seite auf mobilem Viewport öffne
Then ist der CSV-Export-Button nicht sichtbar
```

---

## Business Rules

### Export-Scope
- Export umfasst **alle gefilterten Anträge über alle Seiten hinweg**
- Aktuelle Paginierung wird für Export ignoriert
- Nur die vom User gesetzten Filter werden berücksichtigt

### Exportierte Felder (15 Spalten)
1. `id` - Antrags-ID
2. `name` - Name des Antragstellers
3. `income` - Monatliches Einkommen
4. `fixedCosts` - Monatliche Fixkosten
5. `hasPaymentDefault` - Zahlungsverzug (Ja/Nein)
6. `desiredRate` - Gewünschte Rate
7. `employmentStatus` - Beschäftigungsstatus
8. `status` - Antragsstatus
9. `score` - Scoring-Wert
10. `trafficLight` - Ampel-Bewertung
11. `processorComment` - Kommentar des Processors
12. `createdBy` - Erstellt von (User-ID)
13. `createdAt` - Erstelldatum
14. `submittedAt` - Eingereicht am
15. `processedAt` - Bearbeitet am

**NICHT exportiert:**
- ❌ `scoringReasons` (interne Berechnungsdetails)

### Formatierung
- **Dateiname:** `antraege-processor-YYYY-MM-DD-HHMM.csv`
- **Encoding:** UTF-8 mit BOM
- **Dezimaltrennzeichen:** Punkt
- **Datumsformat:** TT.MM.JJJJ HH:MM
- **Boolean:** "Ja"/"Nein"
- **Ampel:** "Grün", "Gelb", "Rot", "-"
- **Status:** Deutsche Labels
- **employmentStatus:** Deutsche Labels
- **Null-Werte:** "-" oder leer

### Plattform
- Nur Desktop-Ansicht (Tabellen-View)
- Nicht in mobiler Card-Ansicht

---

## Implementierungs-Blueprint

### 1) Implementierungsziel

Implementierung eines CSV-Export-Features für die Processor-Ansicht (`/processor`), das alle gefilterten Anträge über alle Seiten hinweg exportiert. Der Export erfolgt serverseitig über eine neue API-Route und berücksichtigt die vom User gesetzten Filter (z.B. Status). Die CSV-Datei enthält 15 Spalten aus dem DB-Schema im deutschen Format mit UTF-8-BOM-Encoding für Excel-Kompatibilität.

---

### 2) Annahmen

- ⚠️ Export erfolgt **serverseitig** (bessere Kontrolle über Encoding, keine Client-Memory-Probleme)
- ⚠️ Keine Performance-Optimierung für >10.000 Anträge im MVP (kann später mit Streaming ergänzt werden)
- ⚠️ Dateiname enthält Timestamp für Eindeutigkeit

---

### 3) Impact Map

#### Layer/Module betroffen
- **Backend/API Layer:** Neue API-Route für CSV-Export
- **Repository Layer:** Neue Funktion für ungepaginierte, gefilterte Anträge
- **Frontend/UI Layer:** Export-Button in Processor-Tabellen-Header
- **Utility Layer:** CSV-Generierung Service

#### Neue Komponenten
- `src/routes/api/processor/export/+server.ts` - API-Endpunkt
- `src/lib/server/services/csv-export.ts` - CSV-Generierungs-Service

#### Geänderte Komponenten
- `src/lib/server/services/repositories/application.repository.ts` - neue Funktion
- `src/routes/processor/+page.svelte` - Export-Button
- `e2e/processor.test.ts` - E2E-Tests

#### Nicht betroffen
- Mobile Card-Ansicht (bewusst ausgeschlossen)
- DB-Schema (keine Änderungen)
- Applicant-View
- Bestehende Paginierung

---

### 4) Änderungsplan auf Code-Ebene

#### 4.1 Repository Layer - Gefilterte Anträge ohne Paginierung

**Datei:** `src/lib/server/services/repositories/application.repository.ts`  
**Art:** ändern

**Neue Methodensignatur:**
```typescript
export async function getProcessorApplicationsFiltered(params: {
	status?: ApplicationStatus;
}): Promise<Application[]>
```

**Verantwortlichkeit:** Abruf aller Anträge mit optionalem Status-Filter, ohne Paginierung, sortiert nach `createdAt DESC`

**Logik in Pseudocode:**
```typescript
1. Falls params.status gesetzt:
   - whereClause = eq(applications.status, params.status)
2. Sonst:
   - whereClause = undefined
3. Query ausführen:
   - SELECT * FROM applications
   - WHERE whereClause (falls gesetzt)
   - ORDER BY createdAt DESC
4. Rückgabe: Array<Application>
```

**Implementierungs-Hinweis:**
```typescript
const query = db.select().from(applications).orderBy(desc(applications.createdAt));
const items = whereClause ? query.where(whereClause).all() : query.all();
return items;
```

**Error/Edge Cases:**
- Leeres Ergebnis: `[]` zurückgeben (kein Fehler)
- Ungültiger Status: Wird vom API-Handler validiert

---

#### 4.2 CSV-Export Service - Generierung

**Datei:** `src/lib/server/services/csv-export.ts`  
**Art:** neu

**Hauptfunktion:**
```typescript
export function generateApplicationsCsv(applications: Application[]): string
```

**Verantwortlichkeit:** Konvertiert Application-Array in CSV-String mit deutschen Labels, UTF-8-BOM, Excel-kompatibel

**Logik in Pseudocode:**
```typescript
1. UTF-8 BOM hinzufügen: "\uFEFF"

2. CSV-Header definieren (deutsch):
   ["ID", "Name", "Einkommen", "Fixkosten", "Zahlungsverzug", 
    "Gewünschte Rate", "Beschäftigungsstatus", "Status", "Score", 
    "Ampel", "Kommentar", "Erstellt von", "Erstellt am", 
    "Eingereicht am", "Bearbeitet am"]

3. Für jede Application:
   a. Werte konvertieren:
      - employmentStatus → employmentStatusLabels
      - status → statusLabels
      - trafficLight → "Grün"/"Gelb"/"Rot" oder "-"
      - hasPaymentDefault → "Ja"/"Nein"
      - score: null → "-"
      - Datum: ISO → "TT.MM.JJJJ HH:MM"
   
   b. Zeile erstellen mit allen 15 Feldern
   c. CSV-Escape anwenden (Quotes, Kommas, Newlines)

4. Zeilen mit "\n" verbinden
5. Return: UTF-8-BOM + Header + Datenzeilen
```

**Hilfsfunktionen:**
```typescript
function formatDateForCsv(isoDate: string | null): string
function escapeCsvValue(value: string | number | null): string
function getTrafficLightLabel(light: TrafficLight | null): string
```

---

#### 4.3 API Route - CSV-Download

**Datei:** `src/routes/api/processor/export/+server.ts`  
**Art:** neu

**Handler:** `GET`

**Methodensignatur:**
```typescript
export const GET: RequestHandler = async ({ url, locals }) => { ... }
```

**Verantwortlichkeit:** Auth, Validation, Datenabruf, CSV-Generierung, Download-Response

**Logik in Pseudocode:**
```typescript
1. Auth/Authz prüfen:
   if (!locals.user) → error(401, 'Login erforderlich')
   if (locals.user.role !== 'processor') → error(403, 'Keine Berechtigung')

2. Query-Parameter validieren:
   const statusParam = url.searchParams.get('status')
   const allowedStatuses = ['submitted', 'approved', 'rejected', 'draft']
   
   Wenn statusParam gesetzt und nicht in allowedStatuses:
     → error(400, 'Ungültiger Status-Filter')

3. Daten abrufen:
   const applications = await getProcessorApplicationsFiltered({ status: statusFilter })

4. CSV generieren:
   const csvContent = generateApplicationsCsv(applications)

5. Timestamp für Filename erstellen:
   Format: "2026-01-29-0946"

6. Response mit Headers:
   return new Response(csvContent, {
     headers: {
       'Content-Type': 'text/csv; charset=utf-8',
       'Content-Disposition': `attachment; filename="antraege-processor-${timestamp}.csv"`
     }
   })
```

**Error Cases:**
- 401: Nicht eingeloggt
- 403: Falsche Rolle
- 400: Ungültiger Status-Parameter

**Logging:**
```typescript
console.log(`[CSV-Export] User: ${locals.user.id}, Filter: ${statusFilter || 'none'}, Count: ${applications.length}`)
```

---

#### 4.4 Frontend - Export-Button

**Datei:** `src/routes/processor/+page.svelte`  
**Art:** ändern

**Import hinzufügen:**
```typescript
import { Download } from 'lucide-svelte';
```

**Handler-Funktion:**
```typescript
function handleCsvExport() {
  const url = new URL('/api/processor/export', window.location.origin);
  if (data.statusFilter) {
    url.searchParams.set('status', data.statusFilter);
  }
  window.location.href = url.toString();
}
```

**Button im Header (nach Filter-Select):**
```svelte
<button
  onclick={handleCsvExport}
  data-testid="processor-csv-export-button"
  class="btn-secondary flex items-center gap-2"
  title="Als CSV exportieren"
>
  <Download class="w-4 h-4" />
  <span class="hidden sm:inline">CSV Export</span>
</button>
```

**Positionierung:** Im Filter-Header-Bereich (Zeile ~101-118)

**Responsive:** 
- Desktop: Icon + Text
- Mobile: Button versteckt

---

### 5) API Contract

**Endpoint:** `GET /api/processor/export`

**Query Parameters:**
```typescript
{
  status?: 'draft' | 'submitted' | 'approved' | 'rejected'
}
```

**Response Success (200):**
- Content-Type: `text/csv; charset=utf-8`
- Content-Disposition: `attachment; filename="antraege-processor-YYYY-MM-DD-HHMM.csv"`
- Body: CSV-String mit UTF-8-BOM

**Response Errors:**
- 400: Ungültiger Status-Parameter
- 401: Nicht authentifiziert
- 403: Keine Berechtigung

**CSV-Format:**
- Delimiter: `,`
- Encoding: UTF-8 mit BOM
- Line Endings: `\n`
- Quoting: Felder mit `,` oder `"` oder `\n` in Quotes

**Spalten (15):**
```
ID,Name,Einkommen,Fixkosten,Zahlungsverzug,Gewünschte Rate,Beschäftigungsstatus,Status,Score,Ampel,Kommentar,Erstellt von,Erstellt am,Eingereicht am,Bearbeitet am
```

---

### 6) Testplan

#### 6.1 Unit Tests

**Datei:** `src/lib/server/services/csv-export.test.ts` (neu)

**Testfälle:**

1. **CSV mit vollständigen Daten**
   - Given: Application mit allen ausgefüllten Feldern
   - When: generateApplicationsCsv() aufgerufen
   - Then: CSV enthält UTF-8 BOM, korrekte Header, formatierte Werte

2. **Null-Werte**
   - Given: Application mit null-Werten (score, trafficLight, comment)
   - Then: Null-Werte als "-" exportiert

3. **CSV-Escaping**
   - Given: Application mit Komma/Newline/Quote in Feldern
   - Then: Werte korrekt gequoted und escaped

4. **Leeres Array**
   - Given: Leeres applications-Array
   - Then: CSV nur mit Header-Zeile

5. **Datums-Formatierung**
   - Given: ISO-Datum
   - Then: Format "TT.MM.JJJJ HH:MM"

6. **Übersetzungen**
   - Given: status='submitted', employmentStatus='employed'
   - Then: CSV enthält "Eingereicht" und "Angestellt"

---

#### 6.2 Integration Tests

**Datei:** `src/routes/api/processor/export/+server.test.ts` (neu)

**Testfälle:**

1. **Erfolgreicher Export ohne Filter**
   - Given: Processor eingeloggt, 5 Anträge in DB
   - When: GET /api/processor/export
   - Then: Status 200, CSV mit 5 Datenzeilen

2. **Export mit Status-Filter**
   - Given: 3 submitted, 2 approved Anträge
   - When: GET /api/processor/export?status=submitted
   - Then: CSV enthält nur 3 submitted Anträge

3. **Authentifizierung erforderlich**
   - Given: Nicht eingeloggt
   - Then: Status 401

4. **Processor-Rolle erforderlich**
   - Given: Als Applicant eingeloggt
   - Then: Status 403

5. **Ungültiger Status-Parameter**
   - Given: GET /api/processor/export?status=invalid
   - Then: Status 400

---

#### 6.3 E2E Tests

**Datei:** `e2e/processor.test.ts` (erweitern)

**Test-Gruppe: CSV-Export**

1. **Button sichtbar**
   - Given: Processor auf /processor
   - Then: CSV-Export-Button sichtbar

2. **Download ohne Filter**
   - Given: Applications in DB
   - When: Klick auf CSV-Export-Button
   - Then: Download startet, Dateiname korrekt, Inhalt mit BOM und allen Anträgen

3. **Download mit Filter**
   - Given: Status-Filter auf "Eingereicht"
   - When: Klick auf Export-Button
   - Then: CSV enthält nur eingereichte Anträge

4. **Button nicht sichtbar auf Mobile**
   - Given: Viewport 360px
   - Then: Button nicht sichtbar

---

### 7) Risiken & Abhängigkeiten

#### Technische Risiken

**Performance bei großen Datenmengen:**
- **Risiko:** Bei >1000 Anträgen langsamer Export
- **Mitigation:** Akzeptabel für MVP; später Streaming
- **Monitoring:** Log Export-Größe

**Excel-Kompatibilität:**
- **Risiko:** UTF-8-BOM funktioniert nicht überall
- **Mitigation:** Standard für deutsche Excel-Versionen
- **Fallback:** Manueller Import in Excel

**Browser-Download:**
- **Risiko:** Popup-Blocker
- **Mitigation:** `window.location.href` verwenden

#### Abhängigkeiten
Keine - alle Dependencies vorhanden

#### Rollout-Risiken
Minimal - Additive Änderung, keine Breaking Changes

**Rollback:** 
- Button entfernen
- API-Route entfernen
- Keine DB-Änderungen

---

### 8) Verifikationsplan

#### Lokale Entwicklung

```bash
# Unit Tests
npm run test -- csv-export

# Integration Tests
npm run test -- api/processor/export

# E2E Tests
npm run test:e2e -- processor.test.ts

# Alle Tests
npm run test:all
```

#### Manuelle Verifikation

1. Als Processor einloggen → `/processor`
2. Export-Button sichtbar ✓
3. CSV ohne Filter herunterladen → Alle Anträge in Datei ✓
4. Status-Filter setzen → Export → Nur gefilterte Anträge ✓
5. In Excel öffnen → Umlaute korrekt ✓
6. Viewport 360px → Button versteckt ✓

---

## Aufwandsschätzung

**Gesamt: M-L (Medium bis Large)**

- Repository-Funktion: S
- CSV-Service: M
- API-Route: S
- Frontend-Button: S
- Unit Tests: M
- Integration Tests: S
- E2E Tests: M
