# User Story: Paginierung Übersicht eingereichter Kreditanträge

Als **Kreditsachbearbeiter**  
möchte ich **die Übersicht aller eingereichten Kreditanträge seitenweise mit einer klaren Navigation nutzen können**  
damit **ich auch bei vielen Anträgen effizient navigieren, einzelne Seiten gezielt ansteuern und meinen Arbeitskontext behalten kann**.

---

## Akzeptanzkriterien (Auszug)

1. **Feste Seitengröße von 20 Einträgen**
2. **Wechsel auf eine konkrete Seite über Seitennummer**
3. **Vor- und Zurück-Buttons (eine Seite weiter/zurück)**
4. **Anzeige von maximal 5 „laufenden“ Seiten plus erster/letzter Seite (z. B. `1 2 3 4 5 … 10 20 >`)**
5. **Sprung zur ersten Seite**
6. **Sprung zur letzten Seite**

---

## 1) Implementierungsziel

- Die Übersicht „Anträge bearbeiten" (`/processor`) soll eine **serverseitige Pagination** für eingereichte Kreditanträge bekommen.  
- Page Size ist **fix 20**.  
- Der Processor kann über **Seitennummern (max. 5 sichtbar), Erste/Letzte, Vor/Zurück** zwischen den Seiten wechseln – in etwa wie im Beispiel `1 2 3 4 5 … 10 20 >`.

---

## 2) Annahmen & offene Fragen

**Annahmen**

- Die Pagination gilt für die **Processor-Übersicht** („Übersicht aller eingereichten Kreditanträge“ auf `/processor`), inkl. Status-Filter.  
- Page Size ist **fest 20**, nicht konfigurierbar.  
- Filter (Status) bleiben beim Seitenwechsel erhalten.  
- Sortierung: Standard nach `createdAt` absteigend (neueste zuerst).

**Offene Fragen**

- Soll die Pagination auch für andere Listen (z. B. `/applications` für Antragsteller) wiederverwendbar sein?  
- Sollen Stats-Kacheln (`total`, `submitted`, `approved`, `rejected`) **alle Anträge** oder nur den aktuell gefilterten Status zeigen? (geplant: alle, wie heute)

---

## 3) Impact Map (Was ändert sich wo?)

**Layer/Module betroffen**

- Backend:
  - `src/lib/server/services/repositories/application.repository.ts`
  - `src/routes/processor/+page.server.ts`
- Frontend:
  - `src/routes/processor/+page.svelte`
  - neue UI-Komponente: `src/lib/components/Pagination.svelte`
- Tests:
  - `e2e/processor.test.ts`

**Neue Komponenten**

- `src/lib/components/Pagination.svelte`  
  Generische Pagination-Komponente mit Props für `page`, `totalPages` und Callback `onPageChange`.

**Geänderte Komponenten**

- `application.repository.ts`: neue Funktionen für paginierte Abfrage + Statistiken.  
- `+page.server.ts` (Processor): nutzt neue Repo-Funktionen, berechnet Pagination-Metadaten.  
- `+page.svelte` (Processor): rendert neue Pagination-Komponente und wertet Pagination-Daten aus.

**Nicht betroffen / bewusst ausgeschlossen**

- DB-Schema (keine Migration nötig).  
- Auth/Role-Handling.  
- Applicant-Ansicht `/applications` (separate Story).

---

## 4) Änderungsplan auf Code-Ebene (Developer To-Do)

### 4.1 Repository: `src/lib/server/services/repositories/application.repository.ts`

**Art**: ändern

**Neue Konstanten und Funktionen (Signaturen auf hoher Ebene)**

- `const PAGE_SIZE = 20;`
- `export async function getProcessorApplicationsPaginated(params: { status?: ApplicationStatus; page: number; pageSize: number }): Promise<{ items: Application[]; totalCount: number }>`
- `export async function getProcessorApplicationStats(): Promise<{ total: number; submitted: number; approved: number; rejected: number }>`

**Verantwortlichkeit**

- Kapselt die DB-Queries für die Processor-Übersicht inkl. Pagination & Statistiken.

**Logik (Pseudocode)**

```ts
// getProcessorApplicationsPaginated
whereClause = status ? eq(applications.status, status) : undefined

// total count
totalCount = db.select({
  count: sql<number>`count(*)`
}).from(applications)
 .where(whereClause)
 .get()

// page items
items = db.select()
  .from(applications)
  .where(whereClause)
  .orderBy(desc(applications.createdAt))
  .limit(pageSize)
  .offset((page - 1) * pageSize)
  .all()

return { items, totalCount }
```

```ts
// getProcessorApplicationStats
// Variante 1: mehrere count-Queries pro Status
// Variante 2: eine Aggregations-Query und in JS aufteilen
```

**Edge Cases**

- `page` kann außerhalb des gültigen Bereichs liegen – wird im Load (siehe 4.2) gecappt.

---

### 4.2 Server Load: `src/routes/processor/+page.server.ts`

**Art**: ändern

**Ziel**

- Page- und Status-Query-Parameter auslesen, Seite validieren, paginierte Daten + Statistiken laden und Pagination-Metadaten an das Frontend weitergeben.

**Logik (Pseudocode)**

```ts
const PAGE_SIZE = 20

const rawPage = url.searchParams.get('page')
const parsedPage = parseInt(rawPage ?? '1', 10)
const safePage = Number.isNaN(parsedPage) || parsedPage < 1 ? 1 : parsedPage

const statusFilter = url.searchParams.get('status') as ApplicationStatus | null

const { items, totalCount } = await getProcessorApplicationsPaginated({
  status: statusFilter ?? undefined,
  page: safePage,
  pageSize: PAGE_SIZE
})

const totalPages = Math.max(1, Math.ceil(totalCount / PAGE_SIZE))
const currentPage = Math.min(safePage, totalPages)

// Optional: falls currentPage != safePage, Items für currentPage nachladen

const stats = await getProcessorApplicationStats()

return {
  applications: items,
  statusFilter,
  stats,
  pagination: {
    page: currentPage,
    pageSize: PAGE_SIZE,
    totalItems: totalCount,
    totalPages
  }
}
```

**Edge Cases**

- Ungültige oder negative `page` → auf 1 setzen.  
- `page` größer als `totalPages` → auf `totalPages` cappen.  
- `totalCount = 0` → `totalPages = 1`, `page = 1`, leere Liste.

---

### 4.3 Pagination-Komponente: `src/lib/components/Pagination.svelte`

**Art**: neu

**Props (Beispiel)**

- `page: number`  
- `totalPages: number`  
- `onPageChange: (targetPage: number) => void`

**Verantwortung**

- Berechnung der sichtbaren Seiten (max. 5 am Stück) plus erste/letzte Seite und Rendering der Buttons:
  - Erste Seite `«`  
  - Vorherige Seite `<`  
  - Laufende Seiten `1 2 3 4 5`  
  - Ellipsen `…` bei Lücken  
  - Nächste Seite `>`  
  - Letzte Seite `»`

**Seiten-Berechnung (Pseudocode)**

```ts
function getVisiblePages(page: number, totalPages: number): number[] {
  if (totalPages <= 5) {
    return Array.from({ length: totalPages }, (_, i) => i + 1)
  }

  if (page <= 3) {
    return [1, 2, 3, 4, 5]
  }

  if (page >= totalPages - 2) {
    return [totalPages - 4, totalPages - 3, totalPages - 2, totalPages - 1, totalPages]
  }

  return [page - 2, page - 1, page, page + 1, page + 2]
}
```

**E2E-relevante Test-IDs**

- Container: `data-testid="processor-pagination"`  
- Erste Seite: `data-testid="processor-pagination-first"`  
- Vorherige Seite: `data-testid="processor-pagination-prev"`  
- Konkrete Seite: `data-testid="processor-pagination-page-<n>"`  
- Nächste Seite: `data-testid="processor-pagination-next"`  
- Letzte Seite: `data-testid="processor-pagination-last"`

---

### 4.4 Processor-Page UI: `src/routes/processor/+page.svelte`

**Art**: ändern

**Änderungen**

- `Pagination`-Komponente importieren.  
- Unterhalb der `ApplicationTable` die Pagination einfügen.  
- `handleFilterChange` so erweitern, dass beim Filterwechsel `page` wieder auf 1 gesetzt wird.

**Page-Wechsel (Pseudocode)**

```ts
function handlePageChange(targetPage: number) {
  const url = new URL($page.url)
  url.searchParams.set('page', targetPage.toString())
  goto(url.toString())
}
```

Im Markup wird `Pagination` mit `page={data.pagination.page}` und `totalPages={data.pagination.totalPages}` sowie `onPageChange={handlePageChange}` eingebunden.

---

## 5) Daten- & Contract-Änderungen

**DB/Entity**

- Keine Änderungen am Schema.

**API/Contracts**

- Neuer Query-Parameter: `page` (optional, `number >= 1`) auf Route `/processor`.  
- `status`-Filter bleibt wie bisher bestehen und wird bei Page-Wechsel übernommen.

**Rückwärtskompatibilität**

- Aufruf ohne `page` verhält sich wie bisher, allerdings mit Limitierung auf die erste Seite (max. 20 Elemente).

---

## 6) Testplan

**Unit Tests**

- Funktion `getVisiblePages`:
  - `totalPages <= 5` → alle Seiten zurückgeben.  
  - `page = 1` und viele Seiten → `[1,2,3,4,5]`.  
  - `page` im mittleren Bereich → zentriertes 5er-Fenster.  
  - `page` nahe `totalPages` → letzte 5 Seiten.

- Repository-Funktion `getProcessorApplicationsPaginated`:
  - Korrektes `limit`/`offset` für Page 1, 2, letzte Seite.  
  - Filter nach Status (falls gesetzt).

**Integration / Load-Tests**

- `+page.server.ts`:
  - Ungültige `page` → wird zu 1 normalisiert.  
  - `page > totalPages` → wird auf `totalPages` gecappt.  
  - `totalCount = 0` → `totalPages = 1`, `page = 1`, leere Liste.

**E2E Tests (`e2e/processor.test.ts`)**

- Setup mit > 40 Anträgen, damit mehrere Seiten existieren.  
- Szenarien:
  - Erste Seite zeigt maximal 20 Einträge (`application-row-*`).  
  - Klick auf `processor-pagination-page-2` → Seite 2 wird angezeigt.  
  - Klick auf `processor-pagination-next` von Seite 1 → Seite 2.  
  - Klick auf `processor-pagination-last` → letzte Seite, Darstellung ähnlich `… 10 20 >`.  
  - Status-Filter setzen und dann Seitenwechsel → Filter bleibt aktiv.

**Testdaten / Mocks**

- Seed-Daten mit vielen Applications, unterschiedlichen Status, damit Pagination und Filter realistisch getestet werden können.

---

## 7) Risiken & Abhängigkeiten

**Technische Risiken**

- Performance bei sehr großen Datenmengen, falls `count(*)`-Queries teuer werden (kann bei Bedarf mit Indexen optimiert werden).  
- UI-Logik der Pagination (insbesondere Ellipsen) muss gut getestet werden, damit das Verhalten immer intuitiv bleibt.

**Abhängigkeiten**

- Bestehendes `applications`-Schema in der DB.  
- E2E-Setup mit ausreichend Testdaten.
