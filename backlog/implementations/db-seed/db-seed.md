# Implementation Blueprint: Datenbank-Seed

## User Story

Als Projektverantwortlicher
möchte ich, dass beim Start der Anwendung automatisch ein initialer Datenbank-Seed ausgeführt wird, wenn keine Anträge vorhanden sind,
damit ich nach dem Löschen der Datenbank immer einen definierten Ausgangszustand mit realistischen Demo-Daten habe.

## Akzeptanzkriterien

### Szenario 1: Seed wird bei leerer Datenbank ausgeführt
Given die Anwendung wird gestartet
And die Tabelle `applications` enthält keine Einträge
When die Datenbank-Initialisierung abgeschlossen ist
Then enthält die Tabelle mindestens 30 Anträge

### Szenario 2: Gleichmäßige Verteilung der Status
Given der Seed wurde ausgeführt
When ich die Anträge nach Status gruppiere
Then enthält jeder Status (`draft`, `submitted`, `approved`, `rejected`) mindestens 7 Anträge

### Szenario 3: Alle Traffic-Light-Stufen vertreten
Given der Seed wurde ausgeführt
When ich die Scoring-Ergebnisse der Anträge betrachte
Then sind die Traffic-Light-Stufen `green`, `yellow` und `red` jeweils mindestens einmal vertreten

### Szenario 4: Zuordnung zu Keycloak-Usern
Given der Seed wurde ausgeführt
When ich die `createdBy`-Werte der Anträge betrachte
Then verwenden alle Anträge die Keycloak-User-ID des `applicant`-Users

### Szenario 5: Processor-Kommentare bei bearbeiteten Anträgen
Given der Seed wurde ausgeführt
When ich genehmigte oder abgelehnte Anträge betrachte
Then enthalten diese einen realistischen deutschsprachigen `processorComment`

### Szenario 6: Konsistente Zeitstempel
Given der Seed wurde ausgeführt
When ich eingereichte Anträge betrachte
Then haben diese ein `submittedAt`-Datum
And genehmigte/abgelehnte Anträge haben zusätzlich ein `processedAt`-Datum
And `processedAt` liegt zeitlich nach `submittedAt`

### Szenario 7: Seed wird bei vorhandenen Daten übersprungen
Given die Anwendung wird gestartet
And die Tabelle `applications` enthält bereits Einträge
When die Datenbank-Initialisierung abgeschlossen ist
Then werden keine zusätzlichen Anträge eingefügt

### Szenario 8: Scoring-Konsistenz
Given der Seed wurde ausgeführt
When ich Score und TrafficLight eines Antrags betrachte
Then stimmen diese mit dem Ergebnis der `calculateScore`-Funktion für die jeweiligen Antragsdaten überein

## Business Rules

- Der Seed nutzt die bestehende `calculateScore`-Funktion für konsistente Scoring-Werte
- Alle Seed-Daten müssen die Validierungsregeln einhalten (Fixkosten < Einkommen, Rate ≤ verfügbares Einkommen)
- `createdBy` verwendet die Keycloak-User-ID des `applicant`-Users
- Nur der `applicant`-User erstellt Anträge (fachlich korrekt)

## Abhängigkeiten

- Bestehende DB-Schema-Definition (`applications`-Tabelle)
- Scoring-Service (`calculateScore`)
- Keycloak Dev-Setup mit Demo-Usern

---

## 1) Implementierungsziel

Beim App-Start wird geprüft, ob die `applications`-Tabelle leer ist. Falls ja, werden mindestens 30 Anträge mit gleichmäßiger Status-Verteilung, allen Traffic-Light-Stufen, realistischen Processor-Kommentaren und konsistenten Zeitstempeln eingefügt. Die Seed-Logik nutzt die bestehende `calculateScore`-Funktion und ordnet alle Anträge dem `applicant`-Keycloak-User zu.

## 2) Annahmen & offene Fragen

- **Annahme:** `createdBy` wird mit dem Keycloak-`username` "applicant" befüllt, da die UUID erst zur Laufzeit bekannt ist. Falls die App aktuell UUIDs speichert, muss ggf. ein fester Demo-UUID-Wert verwendet werden.
- Keine offenen Fragen.

## 3) Impact Map

| Bereich | Details |
|---|---|
| **Neue Dateien** | `src/lib/server/db/seed.ts` |
| **Geänderte Dateien** | `src/lib/server/db/index.ts` |
| **Neue Testdateien** | `src/lib/server/db/seed.test.ts` |
| **Nicht betroffen** | Schema, Scoring-Service, Validation, Routes, UI, Hooks |

## 4) Änderungsplan auf Code-Ebene

### 4.1 Neue Datei: `src/lib/server/db/seed.ts`

- **Art:** neu
- **Verantwortlichkeit:** Seed-Daten definieren und in DB einfügen, wenn leer
- **Exports:**
  ```ts
  export function seedDatabase(sqliteDb: BetterSqlite3.Database): void
  ```
- **Logik (Pseudocode):**
  1. Prüfe `SELECT COUNT(*) FROM applications` → wenn > 0, return (kein Seed)
  2. Definiere Array `SEED_APPLICATIONS` mit 32 Einträgen (8 pro Status: draft, submitted, approved, rejected)
  3. Jeder Eintrag enthält: `name`, `income`, `fixedCosts`, `desiredRate`, `employmentStatus`, `hasPaymentDefault`
  4. Die Werte sind so gewählt, dass `calculateScore` alle drei Traffic-Light-Stufen erzeugt (green/yellow/red)
  5. Alle Validierungsregeln eingehalten: `fixedCosts < income`, `desiredRate <= income - fixedCosts`
  6. Für jeden Eintrag:
     - Rufe `calculateScore(...)` auf
     - Setze `createdBy = 'applicant'`
     - Setze `createdAt` auf gestaffelte Zeitstempel (vergangene Tage)
     - Je nach Ziel-Status:
       - `draft`: nur `createdAt`
       - `submitted`: + `submittedAt` (nach `createdAt`)
       - `approved`: + `submittedAt` + `processedAt` (nach `submittedAt`) + `processorComment`
       - `rejected`: + `submittedAt` + `processedAt` (nach `submittedAt`) + `processorComment`
  7. Batch-Insert via `db.insert(applications).values([...])` (Drizzle)
  8. Console-Log: `"[Seed] Inserted X demo applications"`

- **Processor-Kommentare:** Array mit realistischen deutschen Texten, z.B.:
  - Approved: "Bonität geprüft, Antrag genehmigt.", "Einkommensverhältnisse positiv bewertet.", ...
  - Rejected: "Zu hohes Risiko aufgrund bestehender Zahlungsverzüge.", "Verfügbares Einkommen reicht nicht aus.", ...

- **Error/Edge Cases:**
  - DB bereits befüllt → kein Insert
  - Keine Fehlerbehandlung nötig (synchroner SQLite-Zugriff beim Start)

### 4.2 Geänderte Datei: `src/lib/server/db/index.ts`

- **Art:** ändern
- **Änderung:** Nach dem `CREATE TABLE IF NOT EXISTS`-Block die Seed-Funktion aufrufen
- **Pseudocode:**
  ```ts
  import { seedDatabase } from './seed';
  seedDatabase(sqlite);
  ```

## 5) Daten- & Contract-Änderungen

- **Keine DB-Schema-Änderungen** – die bestehende `applications`-Tabelle wird unverändert genutzt
- **Keine API-Änderungen**
- **Keine Migrationen**

## 6) Testplan

### 6.1 Unit Tests: `src/lib/server/db/seed.test.ts`

**Testfall 1: Seed bei leerer DB**
- Given: Leere In-Memory-SQLite-DB mit `applications`-Tabelle
- When: `seedDatabase(db)` aufgerufen
- Then: Mindestens 30 Einträge in `applications`

**Testfall 2: Gleichmäßige Status-Verteilung**
- Given: Seed wurde ausgeführt
- When: Gruppiere nach `status`
- Then: Jeder Status (`draft`, `submitted`, `approved`, `rejected`) hat mindestens 7 Einträge

**Testfall 3: Alle Traffic-Light-Stufen**
- Given: Seed wurde ausgeführt
- When: Gruppiere nach `traffic_light`
- Then: `green`, `yellow`, `red` jeweils mindestens 1x vorhanden

**Testfall 4: createdBy korrekt**
- Given: Seed wurde ausgeführt
- When: Prüfe alle `created_by`-Werte
- Then: Alle sind `'applicant'`

**Testfall 5: Processor-Kommentare**
- Given: Seed wurde ausgeführt
- When: Prüfe `approved`/`rejected` Einträge
- Then: Alle haben einen nicht-leeren `processor_comment`

**Testfall 6: Konsistente Zeitstempel**
- Given: Seed wurde ausgeführt
- When: Prüfe `submitted`/`approved`/`rejected` Einträge
- Then: `submitted_at` ist gesetzt; bei `approved`/`rejected` ist `processed_at` gesetzt und liegt nach `submitted_at`

**Testfall 7: Scoring-Konsistenz**
- Given: Seed wurde ausgeführt
- When: Für jeden Eintrag `calculateScore` mit den Antragsdaten aufrufe
- Then: `score` und `traffic_light` stimmen überein

**Testfall 8: Seed bei vorhandenen Daten überspringen**
- Given: DB enthält bereits mindestens 1 Eintrag
- When: `seedDatabase(db)` aufgerufen
- Then: Anzahl der Einträge bleibt unverändert

**Test-Setup:** In-Memory `better-sqlite3`-DB mit dem gleichen `CREATE TABLE`-Statement wie in `index.ts`.

### Verifikation
```bash
npm run test -- --run src/lib/server/db/seed.test.ts
```

## 7) Risiken & Abhängigkeiten

| Risiko | Mitigation |
|---|---|
| `createdBy` erwartet UUID statt Username | Prüfen, was die App aktuell in `createdBy` speichert; ggf. festen Demo-UUID verwenden |
| Seed-Daten verletzen Validierungsregeln | Alle Werte werden vorab gegen `calculateScore` und Validierungsregeln geprüft |
| Performance bei vielen Seed-Einträgen | 32 Einträge sind unkritisch; Batch-Insert verwenden |
