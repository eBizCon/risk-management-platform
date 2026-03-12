# DDD User Stories

Diese User Stories dienen als Grundlage für eine Domain Driven Design Schulung.
Sie bauen aufeinander auf und transformieren das Projekt schrittweise von einem Transaction-Script-Ansatz zu einem Rich Domain Model.

---

## US-DDD-01: Application Aggregate mit expliziter State Machine

**Als** Entwickler des Risk-Management-Systems
**möchte ich** das Application-Modell als echtes Aggregate Root mit explizitem Zustandsautomaten modellieren,
**damit** ungültige Zustandsübergänge zur Compile- und Laufzeit verhindert werden und die Geschäftslogik im Domain Model statt in Services lebt.

### Kontext

Aktuell ist `Application` ein reiner Datentyp (anemic model). Zustandsübergänge (z.B. `draft → submitted`, `submitted → approved`) werden implizit durch direkte DB-Updates in verschiedenen API-Routen durchgeführt. Es gibt keine zentrale Stelle, die alle erlaubten Transitionen definiert und durchsetzt.

### Akzeptanzkriterien (Gherkin)

```gherkin
Feature: Application State Machine

  Scenario: Draft kann eingereicht werden
    Given ein Application Aggregate im Status "draft"
    When submit() aufgerufen wird
    Then ist der Status "submitted"
    And submittedAt ist gesetzt

  Scenario: Eingereicht kann genehmigt werden
    Given ein Application Aggregate im Status "submitted"
    When approve(comment) aufgerufen wird
    Then ist der Status "approved"
    And processedAt ist gesetzt
    And processorComment enthält den Kommentar

  Scenario: Eingereicht kann abgelehnt werden
    Given ein Application Aggregate im Status "submitted"
    When reject(comment) aufgerufen wird
    Then ist der Status "rejected"
    And processedAt ist gesetzt

  Scenario: Ungültige Transition wird verhindert
    Given ein Application Aggregate im Status "draft"
    When approve(comment) aufgerufen wird
    Then wird ein InvalidStateTransitionError geworfen
    And der Status bleibt "draft"

  Scenario: Resubmitted kann genehmigt werden
    Given ein Application Aggregate im Status "resubmitted"
    When approve(comment) aufgerufen wird
    Then ist der Status "approved"

  Scenario: Needs-Information-Transition
    Given ein Application Aggregate im Status "submitted"
    When requestInformation(inquiryText, processorEmail) aufgerufen wird
    Then ist der Status "needs_information"

  Scenario: Antwort auf Rückfrage
    Given ein Application Aggregate im Status "needs_information"
    When answerInquiry(responseText) aufgerufen wird
    Then ist der Status "resubmitted"
```

### Erlaubte Zustandsübergänge

```
draft → submitted
submitted → approved | rejected | needs_information
needs_information → resubmitted
resubmitted → approved | rejected | needs_information
```

### Technische Hinweise

- Neues Modul: `src/lib/server/domain/application.aggregate.ts`
- Methoden auf dem Aggregate: `submit()`, `approve(comment)`, `reject(comment)`, `requestInformation(text, processorEmail)`, `answerInquiry(responseText)`
- Jede Methode validiert die erlaubte Transition und wirft `InvalidStateTransitionError` bei Verstoß
- Repository übernimmt die Persistierung, das Aggregate enthält die Geschäftslogik
- Bestehende Services und API-Routen werden refactored, um das Aggregate zu nutzen
- Bestehende Tests und E2E-Tests müssen weiterhin grün sein

### DDD-Lernziele

- **Aggregate Root**: Konsistenzgrenze um zusammengehörige Daten
- **Invarianten**: Geschäftsregeln, die immer gelten müssen
- **Rich Domain Model vs. Anemic Model**: Verhalten gehört ins Modell
- **Tell, Don't Ask**: Statt Status abzufragen und extern zu entscheiden, dem Aggregate sagen was passieren soll

### Geschätzter Aufwand

90 Minuten (inkl. Refactoring bestehender Services)

---

## US-DDD-02: Domain Events bei Zustandswechsel

**Als** Entwickler des Risk-Management-Systems
**möchte ich** bei jeder Zustandsänderung eines Antrags ein Domain Event emittieren,
**damit** andere Teile des Systems lose gekoppelt auf Geschäftsereignisse reagieren können, ohne direkte Abhängigkeiten zu erzeugen.

### Kontext

Aktuell werden Seiteneffekte (z.B. Scoring bei Submit) direkt in den API-Routen oder Services ausgeführt. Das führt zu enger Kopplung. Domain Events ermöglichen eine saubere Trennung: Das Aggregate meldet *was passiert ist*, Event Handler entscheiden *was daraufhin geschieht*.

### Voraussetzung

US-DDD-01 (Application Aggregate) ist implementiert.

### Akzeptanzkriterien (Gherkin)

```gherkin
Feature: Domain Events

  Scenario: Submit emittiert ApplicationSubmitted Event
    Given ein Application Aggregate im Status "draft"
    When submit() aufgerufen wird
    Then enthält die Event-Liste des Aggregates ein "ApplicationSubmitted" Event
    And das Event enthält applicationId, submittedAt und applicantEmail

  Scenario: Approve emittiert ApplicationApproved Event
    Given ein Application Aggregate im Status "submitted"
    When approve(comment) aufgerufen wird
    Then enthält die Event-Liste des Aggregates ein "ApplicationApproved" Event
    And das Event enthält applicationId, processedAt und processorComment

  Scenario: Reject emittiert ApplicationRejected Event
    Given ein Application Aggregate im Status "submitted"
    When reject(comment) aufgerufen wird
    Then enthält die Event-Liste des Aggregates ein "ApplicationRejected" Event

  Scenario: InquiryRequested Event
    Given ein Application Aggregate im Status "submitted"
    When requestInformation(text, processorEmail) aufgerufen wird
    Then enthält die Event-Liste des Aggregates ein "InquiryRequested" Event

  Scenario: Events werden nach Persistierung dispatched
    Given ein Application Aggregate mit pending Events
    When das Repository das Aggregate speichert
    Then werden alle pending Events über den EventBus dispatched
    And die Event-Liste des Aggregates ist danach leer

  Scenario: Scoring wird durch ApplicationSubmitted Event getriggert
    Given ein registrierter Handler für "ApplicationSubmitted"
    When ein ApplicationSubmitted Event dispatched wird
    Then wird der Scoring-Service aufgerufen
    And Score, TrafficLight und Reasons werden am Antrag gespeichert
```

### Domain Events

| Event | Ausgelöst durch | Payload |
|-------|-----------------|---------|
| `ApplicationSubmitted` | `submit()` | `{ applicationId, submittedAt, applicantEmail }` |
| `ApplicationApproved` | `approve()` | `{ applicationId, processedAt, processorComment }` |
| `ApplicationRejected` | `reject()` | `{ applicationId, processedAt, processorComment }` |
| `InquiryRequested` | `requestInformation()` | `{ applicationId, inquiryText, processorEmail }` |
| `InquiryAnswered` | `answerInquiry()` | `{ applicationId, responseText }` |

### Technische Hinweise

- Neues Modul: `src/lib/server/domain/events/` mit Event-Typen und einem einfachen synchronen EventBus
- Aggregate sammelt Events intern (`domainEvents: DomainEvent[]`)
- Repository dispatched Events nach erfolgreicher Persistierung
- Erster konkreter Handler: Scoring bei `ApplicationSubmitted` (Migration des bestehenden Scoring-Aufrufs)
- In-Process, synchron – kein Message Broker nötig für die Schulung

### DDD-Lernziele

- **Domain Events**: Fachliche Ereignisse als First-Class Citizens
- **Event-Driven Architecture**: Lose Kopplung durch Events statt direkte Aufrufe
- **Mapping zu Event Storming**: Events im Code entsprechen den orangenen Sticky Notes
- **Side-Effect-Trennung**: Aggregate entscheidet, Handler reagiert

### Geschätzter Aufwand

60 Minuten (inkl. Refactoring des Scoring-Aufrufs)

---

## US-DDD-03: RiskScore als Value Object

**Als** Entwickler des Risk-Management-Systems
**möchte ich** das Scoring-Ergebnis als unveränderliches Value Object `RiskScore` modellieren,
**damit** Score, Ampel und Gründe als zusammengehörige fachliche Einheit behandelt werden und die Scoring-Logik im Domain Model gekapselt ist.

### Kontext

Aktuell werden `score` (number), `trafficLight` (string) und `scoringReasons` (string) als separate primitive Felder in der `applications`-Tabelle gespeichert. `scoringReasons` ist ein serialisierter String. Die `calculateScore()`-Funktion gibt ein loses Interface `ScoringResult` zurück. Es fehlt die fachliche Modellierung als zusammengehöriges Konzept.

### Voraussetzung

US-DDD-01 (Application Aggregate) ist implementiert.

### Akzeptanzkriterien (Gherkin)

```gherkin
Feature: RiskScore Value Object

  Scenario: RiskScore wird aus Scoring-Parametern erstellt
    Given Einkommen 5000, Fixkosten 2000, Wunschrate 500, angestellt, keine Zahlungsausfälle
    When RiskScore.calculate() aufgerufen wird
    Then wird ein RiskScore mit score >= 75, trafficLight "green" und mindestens 4 Gründen erstellt

  Scenario: RiskScore ist unveränderlich
    Given ein erstellter RiskScore
    When versucht wird, score oder trafficLight zu ändern
    Then schlägt dies fehl (readonly properties)

  Scenario: Zwei RiskScores mit gleichen Werten sind gleich
    Given zwei RiskScore-Instanzen mit identischem score, trafficLight und reasons
    When equals() aufgerufen wird
    Then gibt die Methode true zurück

  Scenario: RiskScore bestimmt TrafficLight automatisch
    Given ein Score von 80
    When ein RiskScore erstellt wird
    Then ist trafficLight "green"

    Given ein Score von 60
    When ein RiskScore erstellt wird
    Then ist trafficLight "yellow"

    Given ein Score von 30
    When ein RiskScore erstellt wird
    Then ist trafficLight "red"

  Scenario: RiskScore kann serialisiert und deserialisiert werden
    Given ein RiskScore mit score 75, trafficLight "green" und 3 Gründen
    When toJSON() und fromJSON() aufgerufen werden
    Then ist das Ergebnis gleich dem Original

  Scenario: Application Aggregate nutzt RiskScore
    Given ein Application Aggregate
    When applyScore(riskScore) aufgerufen wird
    Then wird der RiskScore am Aggregate gesetzt
    And score, trafficLight und reasons sind über das Aggregate abrufbar
```

### Technische Hinweise

- Neues Modul: `src/lib/server/domain/value-objects/risk-score.ts`
- `RiskScore` ist eine Klasse mit `readonly` Properties: `score`, `trafficLight`, `reasons`
- Factory-Methode `RiskScore.calculate(income, fixedCosts, desiredRate, employmentStatus, hasPaymentDefault)` kapselt die bestehende Scoring-Logik
- Methoden: `equals(other)`, `toJSON()`, `static fromJSON(data)`
- `calculateScore()` in `scoring.ts` wird zu `RiskScore.calculate()` migriert
- Repository serialisiert/deserialisiert beim Speichern/Laden
- Bestehende UI-Darstellung bleibt kompatibel (Score + Ampel + Gründe)

### DDD-Lernziele

- **Value Object**: Unveränderlich, Identität über Werte (nicht über ID)
- **Equality by Value**: Zwei Objekte mit gleichen Werten sind gleich
- **Self-Validating**: Ein RiskScore kann nicht in einem ungültigen Zustand existieren
- **Encapsulation**: Scoring-Logik gehört zum Fachkonzept, nicht in einen externen Service
- **Serialization**: Value Objects müssen für die Persistierung umgewandelt werden

### Geschätzter Aufwand

45 Minuten (inkl. Migration der bestehenden Scoring-Logik)

---

## Reihenfolge und Abhängigkeiten

```
US-DDD-01: Application Aggregate (Grundlage)
    ├── US-DDD-02: Domain Events (baut auf Aggregate auf)
    └── US-DDD-03: RiskScore Value Object (baut auf Aggregate auf)
```

**Gesamtaufwand**: ~195 Minuten (+ 45 min Buffer/Diskussion = 4h Schulung)

## Ergänzende Diskussionsthemen für die Schulung

- **Event Storming → Code**: Wie bilden sich die orangenen Stickies auf Domain Events ab?
- **Bounded Context**: Wo würde man bei Wachstum des Systems schneiden?
- **Anti-Corruption Layer**: Was passiert, wenn ein externes Scoring-System angebunden wird?
- **CQRS-Ansatz**: Read Models für die Processor-Übersicht vs. Write Model im Aggregate
