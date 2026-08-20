### 1) Implementierungsziel
- Kurzbeschreibung in 2–3 Sätzen

### 2) Annahmen & offene Fragen
- Annahmen (falls nötig, klar markiert)
- Max. 3 gezielte Rückfragen, wenn kritische Infos fehlen

### 3) Impact Map (Was ändert sich wo?)
- Layer/Module betroffen: …
- Neue Komponenten: …
- Geänderte Komponenten: …
- Nicht betroffen / bewusst ausgeschlossen: …

### 4) Änderungsplan auf Code-Ebene (Developer To-Do)
Für jede Änderung:
- Datei/Ordner:
- Art: neu / ändern / löschen
- Betroffene Klasse(n):
- Neue/angepasste Methodensignaturen:
- Verantwortlichkeit:
- Logik in Pseudocode (kurz, schrittweise):
- Error/Edge Cases:
- Logging/Telemetry:

### 5) Daten- & Contract-Änderungen
- DB/Entity Änderungen (falls relevant)
- Migration(en) (Name/Schrittfolge)
- API/Events/DTOs (Felder, Typen, Validierungen)
- Rückwärtskompatibilität

### 6) Testplan (aus ACs abgeleitet)
- Unit Tests:
  - Testfälle + betroffene Testdateien
  - Pseudocode für Testlogik
- Integration Tests:
  - Testfälle + Setup/Fixtures
  - Pseudocode für Testlogik
- Contract Tests (falls relevant):
  - Verträge, Breaking Changes
- E2E Tests:
  - Kritische Journeys
  - Pseudocode für Testlogik
- Testdaten / Mocks / Stubs

### 7) Risiken & Abhängigkeiten
- Technische Risiken
- Abhängigkeiten (Teams, Systeme, Deployments)
- Rollout/Migrationsrisiken
