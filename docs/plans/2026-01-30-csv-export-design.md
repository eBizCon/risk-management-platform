# CSV-Export für Processor: Design Dokument

## User Story

> Als processor möchte ich die "meine Anträge" Liste als CSV herunterladen können. Dabei sollen Filter und Sortierkriterien berücksichtigt werden. Es sollen alle verfügbaren Daten im Datenmodell berücksichtigt werden. Den CSV Export kann ich über einen Button in der "meine Anträge" Liste starten.

## Entscheidungen

| Aspekt | Entscheidung |
|--------|--------------|
| **Filter** | Bestehender Status-Filter, erweiterbar für zukünftige Filter |
| **Daten** | Alle Felder außer `scoringReasons` |
| **Enum-Werte** | Technisch (`submitted`, `employed`, `green`) |
| **Dateiname** | `antraege_YYYY-MM-DD_HH-mm.csv` |
| **Ansatz** | Server-side API |

---

## API Design

### Endpoint

```
GET /api/applications/export?status=submitted
```

### Request

- Query-Parameter spiegeln die Filter der UI (aktuell nur `status`)
- Keine Pagination – alle gefilterten Daten werden exportiert

### Response

- Content-Type: `text/csv; charset=utf-8`
- Content-Disposition: `attachment; filename="antraege_2026-01-30_11-17.csv"`
- BOM für Excel-Kompatibilität (`\uFEFF`)

### Autorisierung

- Nur für `processor`-Rolle
- 401 bei fehlendem Login, 403 bei falscher Rolle

---

## CSV-Spalten

| Spalte | Quelle |
|--------|--------|
| `id` | `id` |
| `name` | `name` |
| `income` | `income` |
| `fixedCosts` | `fixedCosts` |
| `desiredRate` | `desiredRate` |
| `employmentStatus` | `employmentStatus` |
| `hasPaymentDefault` | `hasPaymentDefault` |
| `status` | `status` |
| `score` | `score` |
| `trafficLight` | `trafficLight` |
| `processorComment` | `processorComment` |
| `createdAt` | `createdAt` |
| `submittedAt` | `submittedAt` |
| `processedAt` | `processedAt` |
| `createdBy` | `createdBy` |

---

## UI Design

### Button-Platzierung

Der Export-Button wird in der Filter-Leiste platziert, neben dem Status-Dropdown:

```
[Filter-Icon] [Status-Dropdown ▼] "X Anträge gefunden" [CSV Export ⬇]
```

### Button-Details

- Icon: `Download` aus lucide-svelte
- Text: "CSV Export" (oder nur Icon auf Mobile)
- `data-testid="processor-csv-export-button"`

---

## Implementierung

### Dateistruktur

```
src/
├── routes/
│   └── api/
│       └── applications/
│           └── export/
│               └── +server.ts       # API Endpoint
├── lib/
│   └── server/
│       └── services/
│           ├── repositories/
│           │   └── application.repository.ts  # Neue Funktion hinzufügen
│           └── csv-export.service.ts          # CSV-Generierung
```

### Repository-Erweiterung

Neue Funktion in `application.repository.ts`:

```typescript
getProcessorApplicationsForExport(params: { status?: ApplicationStatus }): Promise<Application[]>
```

- Wie `getProcessorApplicationsPaginated`, aber ohne Limit/Offset
- Gibt alle gefilterten Anträge zurück

### CSV-Service

Neuer Service `csv-export.service.ts`:

```typescript
generateApplicationsCsv(applications: Application[]): string
```

- Reine Funktion, einfach testbar
- Escaped Werte korrekt (Kommas, Anführungszeichen, Zeilenumbrüche)
- Fügt BOM für Excel-Kompatibilität hinzu

---

## Fehlerbehandlung

| Szenario | Response |
|----------|----------|
| Nicht eingeloggt | 401 JSON |
| Keine Processor-Rolle | 403 JSON |
| Ungültiger Status-Filter | Wird ignoriert, alle Daten exportiert |
| Keine Daten gefunden | Leere CSV mit nur Header-Zeile |

---

## Tests

### Unit Tests (Vitest)

`csv-export.service.test.ts`:
- Korrekte Header-Reihenfolge
- Escaping von Sonderzeichen (Kommas, Anführungszeichen)
- Leere/null Werte
- BOM am Anfang

### E2E Tests (Playwright)

- Button ist sichtbar für Processor
- Button ist nicht sichtbar für Applicant
- Download startet bei Klick
- Dateiname enthält Timestamp
- CSV enthält gefilterte Daten (mit Status-Filter)
