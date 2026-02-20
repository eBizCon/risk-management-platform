# Ampel-Filter in Processor-Übersicht

## User Story

**Als** Processor
**möchte ich** die Antragsliste nach Ampelfarben (grün, gelb, rot) filtern können – als Mehrfachauswahl und kombinierbar mit dem bestehenden Statusfilter –
**damit** ich Anträge gezielt nach Risikobewertung eingrenzen und priorisiert abarbeiten kann.

## Akzeptanzkriterien

**Szenario 1: Ampel-Filter wird angezeigt**
Given ich bin als Processor auf der Übersichtsseite `/processor`
When die Seite geladen ist
Then sehe ich neben dem bestehenden Statusfilter einen Ampel-Filter mit den Optionen „Positiv" (green), „Prüfung erforderlich" (yellow) und „Kritisch" (red)

**Szenario 2: Einzelne Ampelfarbe filtern**
Given ich bin auf der Processor-Übersicht
When ich die Ampelfarbe „Kritisch" auswähle
Then werden nur Anträge mit Ampelstatus „rot" angezeigt
And die Anzahl der gefundenen Anträge wird aktualisiert

**Szenario 3: Mehrere Ampelfarben gleichzeitig filtern**
Given ich bin auf der Processor-Übersicht
When ich die Ampelfarben „Kritisch" und „Prüfung erforderlich" auswähle
Then werden nur Anträge mit Ampelstatus „rot" oder „gelb" angezeigt

**Szenario 4: Ampel-Filter mit Statusfilter kombinieren**
Given ich habe den Statusfilter auf „Eingereicht" gesetzt
When ich zusätzlich die Ampelfarbe „Kritisch" auswähle
Then werden nur eingereichte Anträge mit Ampelstatus „rot" angezeigt

**Szenario 5: Ampel-Filter zurücksetzen**
Given ich habe einen Ampel-Filter gesetzt
When ich alle Ampelfarben abwähle
Then werden wieder alle Anträge (gemäß ggf. gesetztem Statusfilter) angezeigt

**Szenario 6: Pagination wird bei Filterwechsel zurückgesetzt**
Given ich bin auf Seite 3 der Antragsliste
When ich eine Ampelfarbe auswähle oder abwähle
Then wird die Pagination auf Seite 1 zurückgesetzt

**Szenario 7: Filter-Zustand in URL persistiert**
Given ich habe Ampelfarben „green" und „yellow" ausgewählt
When ich die Seite neu lade
Then sind die Ampelfarben „green" und „yellow" weiterhin ausgewählt und die Liste entsprechend gefiltert

## Business Rules

- Mehrfachauswahl als Checkbox-Gruppe (kein Dropdown)
- Keine Ampelfarbe ausgewählt = kein Filter aktiv (alle Anträge)
- Ampel-Filter und Statusfilter sind UND-verknüpft
- Anträge ohne Ampelwert (null) werden bei aktivem Ampel-Filter nicht angezeigt

## Abhängigkeiten

- Bestehender Statusfilter auf `/processor` (vorhanden)
- `trafficLight`-Spalte in DB (vorhanden)
- `TrafficLight`-Komponente und Labels (vorhanden)

---

## Implementation Blueprint

### 1) Implementierungsziel

Erweiterung der Processor-Übersicht (`/processor`) um einen Ampel-Filter als Mehrfachauswahl (green, yellow, red). Der Filter ist UND-verknüpft mit dem bestehenden Statusfilter, wird als URL-Parameter persistiert und setzt die Pagination bei Änderung zurück.

### 2) Annahmen & offene Fragen

- Keine offenen Fragen. Alle Informationen liegen vor.
- Annahme: Drizzle ORM `inArray()` ist verfügbar (Standard-Operator in drizzle-orm).

### 3) Impact Map (Was ändert sich wo?)

**Layer/Module betroffen:**
- **Repository:** `src/lib/server/services/repositories/application.repository.ts`
- **Server Load:** `src/routes/processor/+page.server.ts`
- **UI:** `src/routes/processor/+page.svelte`
- **E2E Tests:** `e2e/processor.test.ts`

**Neue Komponenten:**
- Keine neuen Dateien

**Geänderte Komponenten:**
- `application.repository.ts` – `getProcessorApplicationsPaginated` um `trafficLight`-Filter erweitern
- `+page.server.ts` – URL-Parameter `trafficLight` parsen und validieren, an Repository weiterreichen
- `+page.svelte` – Checkbox-Gruppe für Ampel-Filter im UI ergänzen

**Nicht betroffen / bewusst ausgeschlossen:**
- DB-Schema (keine Migration nötig)
- `TrafficLight.svelte` Komponente (unverändert)
- `ApplicationTable.svelte` (unverändert)
- Applicant-Seiten

### 4) Änderungsplan auf Code-Ebene (Developer To-Do)

#### 4.1 Repository: `src/lib/server/services/repositories/application.repository.ts`

- **Art:** ändern
- **Betroffene Funktion:** `getProcessorApplicationsPaginated`
- **Neue Signatur:**
  ```ts
  export async function getProcessorApplicationsPaginated(params: {
    status?: ApplicationStatus;
    trafficLight?: TrafficLight[];
    page: number;
    pageSize: number;
  }): Promise<{ items: Application[]; totalCount: number }>
  ```
- **Import hinzufügen:** `inArray` aus `drizzle-orm`, `TrafficLight` aus Schema
- **Logik (Pseudocode):**
  1. Conditions-Array aufbauen: `const conditions: SQL[] = []`
  2. Wenn `params.status` → `conditions.push(eq(applications.status, params.status))`
  3. Wenn `params.trafficLight?.length` → `conditions.push(inArray(applications.trafficLight, params.trafficLight))`
  4. `whereClause = conditions.length > 0 ? and(...conditions) : undefined`
  5. Rest der Funktion bleibt gleich (count + paginated select mit whereClause)
- **Edge Cases:** Leeres `trafficLight`-Array → kein Filter (wie `undefined`)

#### 4.2 Server Load: `src/routes/processor/+page.server.ts`

- **Art:** ändern
- **Logik (Pseudocode):**
  1. `trafficLight`-Parameter aus URL lesen: `url.searchParams.getAll('trafficLight')`
  2. Validierung mit Zod: `z.array(z.enum(['green', 'yellow', 'red']))` – ungültige Werte filtern
  3. Validiertes Array an `getProcessorApplicationsPaginated` übergeben als `trafficLight`
  4. `trafficLightFilter` im Return-Objekt zurückgeben für UI-State
- **Return erweitern:** `trafficLightFilter: TrafficLight[]`

#### 4.3 UI: `src/routes/processor/+page.svelte`

- **Art:** ändern
- **Neue UI-Elemente:** Checkbox-Gruppe neben dem Status-Dropdown
- **Logik (Pseudocode):**
  1. `trafficLightOptions` definieren: `[{ value: 'green', label: 'Positiv' }, { value: 'yellow', label: 'Prüfung erforderlich' }, { value: 'red', label: 'Kritisch' }]`
  2. `handleTrafficLightChange(value: string)` Funktion:
     - Aktuelle Auswahl aus URL lesen (`$page.url.searchParams.getAll('trafficLight')`)
     - Toggle: Wenn Wert vorhanden → entfernen, sonst → hinzufügen
     - URL aktualisieren: alle `trafficLight`-Params setzen
     - `page`-Param löschen (Pagination reset)
     - `goto(url.toString())`
  3. Checkboxen rendern mit `checked`-State aus `data.trafficLightFilter`
- **data-testid Attribute:**
  - `data-testid="processor-traffic-light-filter"` auf Container
  - `data-testid="processor-traffic-light-green"` / `yellow` / `red` auf Checkboxen

### 5) Daten- & Contract-Änderungen

- **DB/Entity:** Keine Änderungen
- **Migration:** Keine
- **URL-API-Contract:**
  - Neuer Query-Parameter: `trafficLight` (wiederholbar)
  - Beispiel: `/processor?status=submitted&trafficLight=red&trafficLight=yellow`
  - Ungültige Werte werden ignoriert (gefiltert durch Zod)
- **Rückwärtskompatibilität:** Vollständig gegeben. Ohne `trafficLight`-Parameter verhält sich die Seite wie bisher.

### 6) Testplan (aus ACs abgeleitet)

#### Unit Tests: `src/lib/server/services/repositories/application.repository.test.ts`

- **Testfall 1:** `getProcessorApplicationsPaginated` ohne trafficLight-Filter → alle Anträge
- **Testfall 2:** `getProcessorApplicationsPaginated` mit `trafficLight: ['red']` → nur rote Anträge
- **Testfall 3:** `getProcessorApplicationsPaginated` mit `trafficLight: ['red', 'yellow']` → rote und gelbe
- **Testfall 4:** `getProcessorApplicationsPaginated` mit `status` + `trafficLight` kombiniert → UND-Verknüpfung
- **Testfall 5:** `getProcessorApplicationsPaginated` mit leerem Array `[]` → alle Anträge

#### E2E Tests: `e2e/processor.test.ts`

- **Testfall 1 (Szenario 1):** Ampel-Filter-Checkboxen sind sichtbar auf `/processor`
- **Testfall 2 (Szenario 2):** Klick auf „Kritisch"-Checkbox → URL enthält `trafficLight=red`, Tabelle zeigt nur rote Anträge
- **Testfall 3 (Szenario 4):** Status „Eingereicht" + Ampel „Kritisch" → URL enthält beide Parameter
- **Testfall 4 (Szenario 5):** Alle Checkboxen abwählen → kein `trafficLight`-Parameter in URL
- **Testfall 5 (Szenario 7):** Seite mit `?trafficLight=green&trafficLight=yellow` laden → Checkboxen sind gecheckt

#### Testdaten / Mocks / Stubs
- Seed-Daten enthalten bereits Anträge mit verschiedenen `trafficLight`-Werten (green, yellow, red) – keine zusätzlichen Fixtures nötig

### 7) Risiken & Abhängigkeiten

- **Technische Risiken:** Gering. Alle benötigten DB-Spalten, Types und Komponenten existieren bereits.
- **Abhängigkeiten:** Keine externen. Nur interne Codeänderungen.
- **Rollout/Migrationsrisiken:** Keine. Kein DB-Schema-Change, volle Rückwärtskompatibilität.
