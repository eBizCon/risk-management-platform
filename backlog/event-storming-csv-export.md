# Event Storming: CSV-Export der Prozessor-Antragsliste

## Kontext

Der Processor möchte die aktuelle gefilterte und sortierte Ansicht der Antragstabelle unter `/processor` als CSV-Datei herunterladen können. Filter (Status) und Sortierung (createdAt desc) müssen serverseitig reproduziert werden.

---

## 1. Actor (Gelbe Sticky Notes — klein)

| Actor | Beschreibung |
|-------|--------------|
| **Processor** | Sachbearbeiter, der die Antragsliste einsieht und den Export auslöst |

---

## 2. Read Model (Grüne Sticky Notes)

Read Models stehen **vor** dem Command — sie liefern dem Actor die Information, auf deren Basis er eine Entscheidung trifft.

| # | Read Model | Beschreibung | Felder |
|---|------------|--------------|--------|
| R1 | **Gefilterte Antragstabelle** | Die aktuelle Tabellenansicht unter `/processor` mit aktivem Status-Filter und Sortierung. Der Processor sieht diese Ansicht und entscheidet sich daraufhin, den Export auszulösen. | `id`, `name`, `status`, `score`, `trafficLight`, `desiredRate`, `income`, `fixedCosts`, `employmentStatus`, `hasPaymentDefault`, `createdAt`, `submittedAt`, `processedAt`, `processorComment` |

### CSV-Spalten-Mapping

| DB-Feld | CSV-Spaltenname |
|---------|-----------------|
| `id` | ID |
| `name` | Name |
| `status` | Status |
| `employmentStatus` | Beschäftigungsstatus |
| `income` | Einkommen |
| `fixedCosts` | Fixkosten |
| `desiredRate` | Gewünschte Rate |
| `hasPaymentDefault` | Zahlungsausfall |
| `score` | Score |
| `trafficLight` | Ampel |
| `processorComment` | Kommentar |
| `createdAt` | Erstellt am |
| `submittedAt` | Eingereicht am |
| `processedAt` | Bearbeitet am |

---

## 3. Commands (Blaue Sticky Notes)

| # | Command | Ausgelöst von | Beschreibung |
|---|---------|---------------|--------------|
| C1 | **CSV-Export anfordern** | Actor: Processor | Klick auf den Export-Button — der Processor hat das Read Model R1 gesehen und möchte die Daten herunterladen |
| C2 | **Export generieren** | Policy: P1 | Anträge abfragen, CSV rendern und als Download bereitstellen |

---

## 4. Aggregate (Großes gelbes Sticky Note)

Das Aggregate sitzt **zwischen** Command und Event. Es nimmt den Command entgegen, prüft die Invarianten und erzeugt das Domain Event.

| Aggregate | Beschreibung |
|-----------|--------------|
| **AntragsExport** | Konsistenzgrenze für den Export-Vorgang. Prüft Invarianten (Berechtigung, Filter-Validität) und produziert das Export-Ergebnis. Liest aus dem Aggregate **Application** (read-only). |

### Invarianten (Geschäftsregeln am Aggregate)

Invarianten sind **keine Policies** — sie sind Regeln, die das Aggregate bei jedem Command prüft, bevor ein Event produziert wird.

| # | Invariante | Beschreibung |
|---|------------|--------------|
| I1 | **Nur Processor-Rolle** | Nur authentifizierte Benutzer mit Rolle `processor` dürfen den Export auslösen |
| I2 | **Erlaubte Status-Werte** | Filter-Parameter muss `submitted`, `approved`, `rejected`, `draft` oder leer sein |
| I3 | **Filterübernahme** | Der Export wendet exakt dieselben Filter an wie die aktuelle Tabellenansicht |
| I4 | **Sortierung beibehalten** | Sortierung nach `createdAt DESC` (identisch zur Tabellenansicht) |
| I5 | **Vollständiger Export** | Alle Datensätze, die dem Filter entsprechen — keine Pagination |
| I6 | **CSV-Spezifikation** | Semikolon-getrennt (`;`), UTF-8 mit BOM, für deutsche Excel-Kompatibilität |
| I7 | **Dynamischer Dateiname** | Format: `antraege-export-{YYYY-MM-DD}.csv` |

---

## 5. Domain Events (Orange Sticky Notes)

Domain Events beschreiben **fachlich relevante Zustandsänderungen** in der Vergangenheitsform.

| # | Event | Produziert von | Beschreibung |
|---|-------|----------------|--------------|
| E1 | **ExportAngefordert** | Aggregate: AntragsExport (nach C1) | Der Processor hat den Export ausgelöst, die initiale Berechtigung wurde geprüft |
| E2 | **AnträgeExportiert** | Aggregate: AntragsExport (nach C2) | CSV wurde erfolgreich generiert und als Download ausgeliefert |
| E3 | **ExportAbgelehnt** | Aggregate: AntragsExport (nach C1 oder C2) | Export wurde abgelehnt — Invariante verletzt (keine Berechtigung, ungültiger Filter, technischer Fehler) |

---

## 6. Policies (Lila Sticky Notes)

Policies sind **reaktive Automatismen**: *"Immer wenn [Event] eintritt, dann löse [Command] aus."*
Sie verbinden Events mit dem nächsten Command.

| # | Policy | Wenn Event… | Dann Command… |
|---|--------|-------------|---------------|
| P1 | **Wenn Export angefordert, dann Export generieren** | E1: ExportAngefordert | C2: Export generieren |

---

## 7. Hotspots / Offene Fragen (Rote Sticky Notes)

| # | Hotspot | Frage / Risiko |
|---|---------|----------------|
| H1 | **Große Datenmengen** | Was passiert bei sehr vielen Anträgen? Brauchen wir Streaming oder ein Limit? |
| H2 | **Sortierung erweiterbar?** | Aktuell nur `createdAt DESC`. Soll der Export auch benutzerdefinierte Sortierungen unterstützen? |
| H3 | **Zusätzliche Filter** | Sollen zukünftige Filter (z.B. Datumsbereich, Ampel, Score-Range) auch im Export berücksichtigt werden? |
| H4 | **Audit-Log** | Soll der Export protokolliert werden (wer hat wann was exportiert)? |
| H5 | **Label-Übersetzung** | Sollen Enum-Werte (status, employmentStatus, trafficLight) als deutsche Labels in der CSV stehen? |

---

## 8. Bounded Context

| Context | Verantwortung |
|---------|---------------|
| **Processor Workspace** | Alles rund um die Sachbearbeiter-Ansicht: Filtern, Sortieren, Exportieren |
| **Application Management** | CRUD und Statusübergänge der Anträge (hier nur lesend genutzt) |

---

## 9. Event Flow (Kanonische Grammatik)

```
Actor → Read Model → Command → Aggregate → Event → Policy → Command → Aggregate → Event
```

```
┌──────────────┐
│  Processor    │  (Actor)
│  (Sachbearb.) │
└──────┬───────┘
       │ liest
       ▼
┌──────────────────────────────────┐
│  R1: Gefilterte Antragstabelle   │  (Read Model)
│  Status-Filter + Sortierung      │
└──────┬───────────────────────────┘
       │ entscheidet sich für Export
       ▼
┌──────────────────────────────────┐
│  C1: CSV-Export anfordern        │  (Command)
│  Parameter: status={status}      │
└──────┬───────────────────────────┘
       │
       ▼
┌──────────────────────────────────┐
│  Aggregate: AntragsExport        │  (Aggregate)
│                                  │
│  Prüft Invarianten:             │
│  • I1: Processor-Rolle?          │
│  • I2: Status-Wert gültig?      │
└──────┬───────────────────────────┘
       │
       ├─── Invariante verletzt ──────────────────┐
       │                                          ▼
       │                              ┌───────────────────────┐
       │                              │  E3: ExportAbgelehnt  │  (Domain Event)
       │                              │  Grund: 401/403/400   │
       │                              └───────────────────────┘
       │
       │ Invarianten erfüllt
       ▼
┌──────────────────────────────────┐
│  E1: ExportAngefordert           │  (Domain Event)
└──────┬───────────────────────────┘
       │
       ▼
┌──────────────────────────────────┐
│  P1: Wenn ExportAngefordert,     │  (Policy)
│      dann Export generieren      │
└──────┬───────────────────────────┘
       │ löst aus
       ▼
┌──────────────────────────────────┐
│  C2: Export generieren           │  (Command)
│  • Anträge abfragen (I3–I5)     │
│  • CSV rendern (I6–I7)          │
│  • Download ausliefern           │
└──────┬───────────────────────────┘
       │
       ▼
┌──────────────────────────────────┐
│  Aggregate: AntragsExport        │  (Aggregate)
│                                  │
│  Wendet an:                      │
│  • I3: Filterübernahme          │
│  • I4: Sortierung createdAt DESC │
│  • I5: Alle Datensätze (kein    │
│         Limit)                   │
│  • I6: Semikolon-CSV, UTF-8 BOM │
│  • I7: Dynamischer Dateiname    │
└──────┬───────────────────────────┘
       │
       ├─── Fehler (DB/Rendering) ────────────────┐
       │                                          ▼
       │                              ┌───────────────────────┐
       │                              │  E3: ExportAbgelehnt  │  (Domain Event)
       │                              │  Grund: Technischer   │
       │                              │  Fehler               │
       │                              └───────────────────────┘
       │
       │ Erfolg
       ▼
┌──────────────────────────────────┐
│  E2: AnträgeExportiert           │  (Domain Event)
│  CSV-Download wurde ausgeliefert │
└──────────────────────────────────┘
```

---

## 10. Zusammenfassung der Verknüpfungen

| Von | Beziehung | Zu |
|-----|-----------|-----|
| **Processor** (Actor) | liest | **R1** (Read Model) |
| **Processor** (Actor) | löst aus | **C1** (Command) |
| **C1** (Command) | geht an | **AntragsExport** (Aggregate) |
| **AntragsExport** (Aggregate) | prüft I1, I2 und produziert | **E1** oder **E3** (Event) |
| **E1** (Event) | triggert | **P1** (Policy) |
| **P1** (Policy) | löst aus | **C2** (Command) |
| **C2** (Command) | geht an | **AntragsExport** (Aggregate) |
| **AntragsExport** (Aggregate) | wendet I3–I7 an und produziert | **E2** oder **E3** (Event) |
