# Advanced Use Cases: Kreditantrags-Management

Diese Liste ergänzt die bestehenden Use Cases (`use-cases.md`) um fachlich tiefergehende Szenarien, die für eine realistische Kreditantrags-Plattform relevant sind.

---

## C) Kreditdetails & Konditionen

### 25) Kreditbetrag und Laufzeit erfassen

- **Ziel**: Antrag um `Kreditbetrag` (Loan Amount) und `Laufzeit` (z.B. 12, 24, 48, 60 Monate) erweitern.
- **Umsetzungsidee**: Neue Value Objects `LoanAmount` und `LoanTerm`, Erweiterung von `Application.Create()`, Formular-Felder im Frontend, Scoring berücksichtigt neue Felder.
- **Ergebnis**: Anträge bilden den tatsächlichen Kreditwunsch vollständig ab.

### 26) Zinssatz berechnen und anzeigen

- **Ziel**: Basierend auf Score, Laufzeit und Betrag einen individuellen Zinssatz berechnen und dem Applicant anzeigen.
- **Umsetzungsidee**: Domain Service `InterestRateCalculator`, Ergebnis als Value Object `InterestRate`, Anzeige im Antrags-Detail.
- **Ergebnis**: Antragsteller sieht sofort die voraussichtlichen Konditionen.

### 27) Kreditart auswählen

- **Ziel**: Antrag um `Kreditart` erweitern (Ratenkredit, Autokredit, Immobilienfinanzierung, Umschuldung).
- **Umsetzungsidee**: Neues Value Object `LoanType` als Enumeration, unterschiedliche Validierungsregeln je Typ, UI-Dropdown.
- **Ergebnis**: Differenzierte Antragsstrecken je nach Kreditprodukt.

### 28) Tilgungsplan-Vorschau generieren

- **Ziel**: Nach Antragserstellung eine Tilgungsplan-Vorschau (monatliche Rate, Zinsanteil, Tilgungsanteil, Restschuld) anzeigen.
- **Umsetzungsidee**: Domain Service `AmortizationScheduleService`, Read Model für die Vorschau, UI-Tabelle oder Chart.
- **Ergebnis**: Transparenz über die Gesamtkosten des Kredits.

---

## D) Antragsteller-Profil & Personendaten

### 29) Erweiterte Personendaten erfassen

- **Ziel**: Antrag um Adresse, Geburtsdatum, Familienstand und Anzahl Unterhaltsberechtigte erweitern.
- **Umsetzungsidee**: Value Objects `Address`, `DateOfBirth`, `MaritalStatus`, `NumberOfDependents`. Formular wird mehrstufig (Wizard).
- **Ergebnis**: Realistischere Bonitätsprüfung, vollständigere Antragsdaten.

### 30) Wohnsituation erfassen

- **Ziel**: Erfassen ob der Antragsteller zur Miete wohnt oder Eigentum besitzt, inkl. monatlicher Mietkosten.
- **Umsetzungsidee**: Value Object `HousingSituation` (Rent/Own/Other), optionales Feld `MonthlyRent` als `Money`. Scoring berücksichtigt Wohnsituation.
- **Ergebnis**: Differenziertere Risikoeinschätzung.

### 31) Beschäftigungsdauer und Arbeitgeber erfassen

- **Ziel**: Ergänzung um Arbeitgeber-Name und Dauer der aktuellen Beschäftigung.
- **Umsetzungsidee**: Value Object `EmploymentDetails` (Employer, EmployedSince). Scoring gewichtet lange Beschäftigungsdauer positiv.
- **Ergebnis**: Stabilität der Einkommenssituation wird besser bewertet.

### 32) Bestehende Verbindlichkeiten erfassen

- **Ziel**: Antragsteller gibt bestehende Kredite/Verbindlichkeiten an (Betrag, monatliche Rate, Restlaufzeit).
- **Umsetzungsidee**: Child Entity `ExistingLiability` am Application-Aggregate, Summe fließt in Scoring ein.
- **Ergebnis**: Gesamtverschuldung wird sichtbar und im Scoring berücksichtigt.

---

## E) Externe Bonitätsprüfung

### 33) SCHUFA-Einwilligung einholen

- **Ziel**: Vor Einreichung muss der Antragsteller der externen Bonitätsabfrage zustimmen (Checkbox + Timestamp).
- **Umsetzungsidee**: Boolesches Feld `SchufahConsentGiven` + `ConsentGivenAt` am Aggregate, Guard Clause in `Submit()`.
- **Ergebnis**: Compliance-konforme Datenverarbeitung.

### 34) Externe Bonitätsabfrage integrieren

- **Ziel**: Nach Einreichung wird automatisch eine externe Bonitätsauskunft abgefragt (z.B. SCHUFA, Creditreform).
- **Umsetzungsidee**: Anti-Corruption Layer mit Interface `ICreditCheckService`, Domain Event `ApplicationSubmitted` triggert die Abfrage, Ergebnis als Value Object `ExternalCreditScore` am Aggregate.
- **Ergebnis**: Internes + externes Scoring für fundierte Entscheidung.

### 35) KYC-Identitätsprüfung

- **Ziel**: Know-Your-Customer Prüfung vor finaler Genehmigung (Ausweisdokument, Adressnachweis).
- **Umsetzungsidee**: Neuer Status `AwaitingIdentityVerification`, Upload-Möglichkeit für Ausweisdokument, Processor bestätigt Identität.
- **Ergebnis**: Regulatorische Anforderungen werden abgedeckt.

---

## F) Antrag zurückziehen / Widerruf

### 36) Antragsteller zieht eingereichten Antrag zurück

- **Ziel**: Antragsteller kann einen Antrag im Status `Submitted` oder `Resubmitted` selbst zurückziehen.
- **Umsetzungsidee**: Neuer Status `Withdrawn`, Methode `Withdraw()` am Aggregate mit Guard Clause, Domain Event `ApplicationWithdrawnEvent`, Button im Antrags-Detail.
- **Ergebnis**: Antragsteller behält Kontrolle über seinen Antrag.

### 37) Widerrufsfrist nach Genehmigung

- **Ziel**: Nach Genehmigung hat der Antragsteller 14 Tage Widerrufsfrist (gemäß Verbraucherkreditrichtlinie).
- **Umsetzungsidee**: `WithdrawalDeadline` berechnet aus `ProcessedAt + 14 Tage`, Status `Approved` erlaubt `Revoke()` innerhalb der Frist, danach automatischer Übergang zu `Final`.
- **Ergebnis**: Rechtskonforme Widerrufsmöglichkeit.

### 38) Stornierung durch Sachbearbeiter

- **Ziel**: Processor kann einen Antrag stornieren (z.B. bei Betrugsverdacht) mit Pflicht-Begründung.
- **Umsetzungsidee**: Neuer Status `Cancelled`, Methode `Cancel(string reason)` am Aggregate, nur aus `Submitted`/`Resubmitted`/`NeedsInformation` möglich.
- **Ergebnis**: Schutz vor Missbrauch, klare Abgrenzung zu Reject.

---

## G) Vier-Augen-Prinzip & Bearbeiterzuweisung

### 39) Zweite Genehmigung bei hohem Risiko oder Betrag

- **Ziel**: Ab einem bestimmten Kreditbetrag oder bei Yellow/Red-Scoring muss ein zweiter Processor genehmigen.
- **Umsetzungsidee**: Domain Policy `FourEyesPrinciplePolicy`, neuer Status `AwaitingSecondApproval`, zweites Approval-Feld `SecondApprovedBy`.
- **Ergebnis**: Risikominimierung bei kritischen Anträgen.

### 40) Eskalation bei Fristüberschreitung

- **Ziel**: Wenn ein Antrag länger als X Tage unbearbeitet bleibt, wird er automatisch eskaliert.
- **Umsetzungsidee**: Background Job prüft `SubmittedAt` gegen SLA-Threshold, setzt Flag `IsEscalated`, Benachrichtigung an Teamleiter.
- **Ergebnis**: Keine Anträge bleiben unbemerkt liegen.

---

## H) Vertrag & Auszahlung (Post-Approval)

### 41) Vertragserstellung nach Genehmigung

- **Ziel**: Nach Genehmigung wird automatisch ein Kreditvertrag generiert (PDF mit Konditionen, Tilgungsplan, Bedingungen).
- **Umsetzungsidee**: Eigener Bounded Context `ContractManagement`, Domain Event `ApplicationDecidedEvent` (approved) triggert Vertragserstellung, PDF-Rendering.
- **Ergebnis**: Durchgängiger Prozess von Antrag bis Vertrag.

### 42) Digitale Signatur

- **Ziel**: Antragsteller und Bank unterzeichnen den Vertrag digital.
- **Umsetzungsidee**: Integration mit Signatur-Provider (z.B. DocuSign, qualifizierte elektronische Signatur), neuer Status `AwaitingSignature` → `Signed`.
- **Ergebnis**: Papierloser Abschluss.

### 43) Auszahlung anstoßen

- **Ziel**: Nach beidseitiger Unterschrift wird die Auszahlung des Kreditbetrags initiiert.
- **Umsetzungsidee**: Neuer Status `Disbursed`, Integration mit Zahlungssystem, Domain Event `LoanDisbursedEvent`.
- **Ergebnis**: End-to-End-Prozess vom Antrag bis zur Auszahlung.

---

## I) Benachrichtigungen

### 44) E-Mail bei Statusänderung

- **Ziel**: Antragsteller erhält E-Mail bei jeder relevanten Statusänderung (Submitted, Approved, Rejected, NeedsInformation).
- **Umsetzungsidee**: Domain Event Handler für `ApplicationSubmittedEvent`, `ApplicationDecidedEvent`, `InquiryCreatedEvent` → E-Mail-Service via Interface `INotificationService`.
- **Ergebnis**: Proaktive Information ohne manuelles Nachschauen.

### 45) Erinnerung bei unbeantworteter Rückfrage

- **Ziel**: Wenn eine Rückfrage nach X Tagen nicht beantwortet wird, erhält der Antragsteller eine Erinnerung.
- **Umsetzungsidee**: Background Job prüft offene Inquiries mit `CreatedAt` > Threshold, sendet Erinnerungs-E-Mail.
- **Ergebnis**: Weniger vergessene Rückfragen, schnellere Bearbeitung.

### 46) Processor-Benachrichtigung bei Wiedereinreichung

- **Ziel**: Wenn ein Antragsteller eine Rückfrage beantwortet (Status → Resubmitted), wird der zuständige Processor benachrichtigt.
- **Umsetzungsidee**: Event Handler für Status-Wechsel zu `Resubmitted`, Benachrichtigung an `ProcessorEmail` aus der Inquiry.
- **Ergebnis**: Nahtlose Weiterbearbeitung ohne manuelle Queue-Prüfung.

---

## J) Fristen & SLAs

### 47) Bearbeitungsfrist für Sachbearbeiter definieren

- **Ziel**: Jeder eingereichte Antrag hat eine Bearbeitungsfrist (z.B. 48h für Standard, 24h für Eskalation).
- **Umsetzungsidee**: Value Object `ProcessingDeadline`, berechnet bei Submit, sichtbar in Processor-Liste, farbliche Hervorhebung bei Fristablauf.
- **Ergebnis**: Transparente SLAs, messbare Bearbeitungszeit.

### 48) Automatischer Ablauf unvollständiger Anträge

- **Ziel**: Drafts, die länger als 30 Tage nicht eingereicht werden, werden automatisch als `Expired` markiert.
- **Umsetzungsidee**: Background Job, neuer Status `Expired`, Benachrichtigung vor Ablauf (z.B. 7 Tage vorher).
- **Ergebnis**: Saubere Datenbasis, keine verwaisten Entwürfe.

---

## K) Audit Trail & Compliance

### 49) Vollständige Änderungshistorie

- **Ziel**: Jede Statusänderung, jeder Kommentar, jede Bearbeitung wird unveränderlich protokolliert.
- **Umsetzungsidee**: Entity `AuditEntry` (Timestamp, Actor, Action, OldValue, NewValue), Event Sourcing Light oder Event Handler, die bei jedem Domain Event einen Audit-Eintrag schreiben.
- **Ergebnis**: Lückenlose Nachvollziehbarkeit für Compliance und Revision.

### 50) Datenexport für Aufsichtsbehörde

- **Ziel**: Strukturierter Export aller Antragsdaten inkl. Scoring-Begründungen und Entscheidungen.
- **Umsetzungsidee**: Dedizierter Export-Endpoint mit festem Schema (z.B. XML/JSON), Zugangsbeschränkung auf Admin-Rolle.
- **Ergebnis**: Regulatorische Anforderungen (z.B. BaFin) werden erfüllt.

---

## L) DDD-Schulungs-Use-Cases: Domain Events, Policies & Specifications

Diese Sektion bewertet Use Cases speziell danach, welche **neuen DDD-Patterns** sie für die Schulung einführen. Basis: Was bereits demonstriert wird vs. was in `domain-policy.md` und `backend-ddd.md` als nächste Konzepte vorgesehen ist.

### Bereits demonstrierte DDD-Patterns

| Pattern | Status | Wo |
|---|---|---|
| Aggregates + Child Entities | ✅ | `Application` + `ApplicationInquiry` |
| Value Objects | ✅ | `Money`, `EmailAddress`, `TrafficLight`, etc. |
| Strongly Typed IDs | ✅ | `ApplicationId`, `InquiryId` |
| Domain Services | ✅ | `ScoringService` |
| Guard Clauses | ✅ | `Create()`, `Submit()`, `Approve()`, etc. |
| Domain Events (raised) | ✅ | 4 Events, aber **keine Handler** |
| CQRS + Dispatcher | ✅ | Commands, Queries, custom Dispatcher |
| Result Pattern | ✅ | Durchgängig |

### Noch NICHT demonstrierte Patterns

| Pattern | Aus den Regeln | Priorität für Schulung |
|---|---|---|
| **Domain Policies** (`IDomainPolicy<TEvent>`) | `domain-policy.md` + `backend-ddd.md` | Hoch — zentrales neues Konzept |
| **Domain Event Handler** (echte Reaktion) | Regel: *"Every event SHOULD have at least one handler"* | Hoch — aktuell sind alle Events "dead code" |
| **Specification Pattern** | `backend-ddd.md` beschrieben | Mittel — wiederverwendbare Geschäftsregeln |
| **Pipeline Behaviors** | `domain-policy.md` beschrieben | Niedrig — eher technisch als fachlich |

---

### 51) ⭐ EMPFOHLEN: Auto-Decision Policy + In-App-Benachrichtigung

**Fachliche Regel**: *"Wenn ein Antrag eingereicht wird UND die Ampel Grün ist (Score ≥ 80), wird er automatisch genehmigt. Bei Rot (Score < 30) automatisch abgelehnt. Bei Gelb entscheidet ein Sachbearbeiter. Nach jeder Entscheidung wird der Antragsteller per In-App-Notification benachrichtigt."*

- **Ziel**: Automatische Entscheidung bei eindeutigen Fällen + Benachrichtigung als Side-Effect.
- **Umsetzungsidee**:
  - **Domain Policy** `AutoDecisionPolicy : IDomainPolicy<ApplicationSubmittedEvent>` — läuft VOR Save, gleiche Transaktion, ruft `application.Approve()` oder `application.Reject()` auf.
  - **Specification** `AutoApprovableSpecification` / `AutoRejectableSpecification` — kapselt die Entscheidungslogik als wiederverwendbaren Ausdruck.
  - **Domain Event Handler** `CreateNotificationOnDecisionHandler : IDomainEventHandler<ApplicationDecidedEvent>` — erstellt Notification NACH Save (Fire-and-Forget Side-Effect).
  - **Neue Entity** `Notification` (eigenes kleines Aggregate mit `NotificationType`, `NotificationStatus`, `ReadAt`).
  - **Dispatcher-Erweiterung**: `ExecutePoliciesAsync()` als neues Konzept neben `PublishDomainEventsAsync()`.
- **Ergebnis**: Kompletter Pipeline-Fluss aus `domain-policy.md` wird demonstriert.

**Demonstrierte Pipeline im Handler:**
```
SubmitApplicationHandler:
  1. application.Submit()                    ← Business Logic (existiert)
  2. dispatcher.ExecutePoliciesAsync()       ← Domain Policy (NEU, vor Save)
  3. SaveChangesAsync()                      ← Persistence
  4. dispatcher.PublishDomainEventsAsync()    ← Event Handler (NEU, nach Save)
     → CreateNotificationOnDecisionHandler   ← Side-Effect: Notification erstellen
```

**Neue DDD-Patterns**: Domain Policy, Domain Event Handler, Specification, Dispatcher-Erweiterung, zweites Aggregate.

**Schulungsbewertung**:
- ✅ **4 neue Patterns** in einem Use Case
- ✅ Direkt an bestehender `Submit()`-Logik aufsetzbar
- ✅ Unterschied Policy vs. Event Handler wird am lebenden Code sichtbar
- ✅ Frontend-Ergebnis sofort sichtbar (Notification-Badge)
- ✅ Moderate Komplexität — in 2–3h umsetzbar
- ⚠️ Aggregate `Notification` ist sehr klein — manche könnten es als "Overkill" empfinden

---

### 52) Alternative A: Audit Trail als Event Handler

**Fachliche Regel**: *"Jede Statusänderung eines Antrags wird unveränderlich in einer Änderungshistorie protokolliert (wer, wann, was, alter/neuer Wert)."*

- **Ziel**: Domain Event Handler schreiben Audit-Einträge für alle existierenden Events.
- **Umsetzungsidee**:
  - **Domain Event Handler** für `ApplicationSubmittedEvent`, `ApplicationDecidedEvent`, `ApplicationDeletedEvent`, `InquiryCreatedEvent`.
  - **Neue Entity** `AuditEntry` (Timestamp, ActorEmail, Action, OldValue, NewValue, ApplicationId).
  - Query `GetAuditTrailByApplication` für die Detailansicht.
- **Ergebnis**: Alle bestehenden Events bekommen endlich Handler — kein "dead code" mehr.

**Schulungsbewertung**:
- ✅ Alle 4 existierenden Events werden "lebendig"
- ✅ Einfaches, verständliches Konzept (Audit = Logging)
- ✅ Zeigt gut, dass Events **nach Save** laufen (Fire-and-Forget)
- ❌ **Keine Domain Policy** — zeigt nur After-Save Event Handler
- ❌ Kein Unterschied Policy vs. Event sichtbar
- ❌ Fachlich wenig spannend (Logging ist nicht "Business-Logik")

**Fazit**: Guter Einstieg in Event Handler, aber **zeigt nur die Hälfte** des Konzepts (Events ja, Policies nein).

---

### 53) Alternative B: Antrag zurückziehen (Withdraw)

**Fachliche Regel**: *"Ein Antragsteller kann einen eingereichten Antrag selbst zurückziehen, solange er noch nicht bearbeitet wurde."*

- **Ziel**: Neuer Status `Withdrawn`, Methode `Withdraw()`, Domain Event `ApplicationWithdrawnEvent`, Event Handler für Benachrichtigung.
- **Umsetzungsidee**:
  - Neuer Status in `ApplicationStatus` Enumeration.
  - Guard Clause: Nur aus `Submitted`/`Resubmitted` möglich.
  - Domain Event + Handler (z.B. Processor-Benachrichtigung).
  - Frontend: Button im Antrags-Detail.
- **Ergebnis**: Antragsteller hat mehr Kontrolle.

**Schulungsbewertung**:
- ✅ Saubere Status-Transition mit Guard Clause
- ✅ Neues Domain Event + Handler
- ✅ Einfach zu verstehen, schnell umsetzbar (~1h)
- ❌ **Keine Policy** — nur neuer Status + Event
- ❌ Kein neues DDD-Pattern — nur Erweiterung bestehender Patterns
- ❌ Fachlich korrekt, aber didaktisch wenig Mehrwert

**Fazit**: Fachlich sinnvoll, aber **führt kein neues DDD-Konzept ein**. Eher geeignet als Aufwärmübung.

---

### 54) Alternative C: Vier-Augen-Prinzip (Second Approval)

**Fachliche Regel**: *"Anträge mit Ampel Gelb oder Rot benötigen die Genehmigung von zwei verschiedenen Sachbearbeitern."*

- **Ziel**: Domain Policy prüft nach `Approve()`, ob ein zweiter Approval nötig ist.
- **Umsetzungsidee**:
  - **Domain Policy** `FourEyesPrinciplePolicy : IDomainPolicy<ApplicationDecidedEvent>` — prüft vor Save ob Zweigenehmigung erforderlich.
  - Neuer Status `AwaitingSecondApproval`.
  - Neue Felder: `FirstApprovedBy`, `SecondApprovedBy`.
  - **Specification** `RequiresSecondApprovalSpecification` (TrafficLight != Green).
- **Ergebnis**: Risikominimierung bei kritischen Anträgen.

**Schulungsbewertung**:
- ✅ Zeigt Domain Policy perfekt (transaktional, vor Save)
- ✅ Zeigt Specification Pattern
- ✅ Fachlich anspruchsvoll und realistisch
- ❌ **Hohe Komplexität** — Status-Maschine wird deutlich komplexer
- ❌ Braucht mindestens 2 Processor-Accounts zum Testen
- ❌ Frontend-Aufwand hoch (neue Buttons, Status-Anzeige, Berechtigung)
- ❌ Für 4h-Schulung vermutlich **zu umfangreich**

**Fazit**: Fachlich und didaktisch sehr gut, aber **Aufwand zu hoch** für eine Schulung. Besser als Folge-Use-Case nach #51.

---

### 55) Alternative D: Externe Bonitätsabfrage (Anti-Corruption Layer)

**Fachliche Regel**: *"Nach Einreichung wird automatisch eine externe Bonitätsauskunft abgefragt. Der externe Score fließt in die Gesamtbewertung ein."*

- **Ziel**: ACL-Pattern für externe Systemintegration demonstrieren.
- **Umsetzungsidee**:
  - Interface `ICreditCheckService` in der Domain.
  - **Domain Event Handler** für `ApplicationSubmittedEvent` triggert die Abfrage.
  - **ACL-Adapter** in Infrastructure übersetzt externes Modell → Value Object `ExternalCreditScore`.
  - Fake-Implementation für die Schulung (simuliert API-Call).
- **Ergebnis**: Clean Architecture Schichttrennung wird sichtbar.

**Schulungsbewertung**:
- ✅ Zeigt Anti-Corruption Layer (ACL) — wichtiges strategisches DDD-Pattern
- ✅ Zeigt Domain Event Handler als Prozess-Trigger
- ✅ Interface in Domain, Implementation in Infrastructure = Dependency Inversion
- ❌ **Keine Domain Policy** — nur Event Handler + ACL
- ❌ Braucht Fake-Service → Teilnehmer sehen keinen "echten" Effekt
- ❌ Asynchrone Abfrage passt schlecht zu After-Save Events (eigentlich Outbox Pattern nötig)
- ❌ Kann zu Diskussionen über Eventual Consistency führen, die den Rahmen sprengen

**Fazit**: Zeigt ein wichtiges strategisches Pattern, aber **lenkt von taktischem DDD ab** (Events/Policies). Besser als separates Modul.

---

### Vergleichsmatrix für die Schulung

| Use Case | Domain Policy | Event Handler | Specification | ACL | Neue Aggregate | Komplexität | Empfehlung |
|---|:---:|:---:|:---:|:---:|:---:|---|---|
| **#51 Auto-Decision + Notification** | ✅ | ✅ | ✅ | — | ✅ Notification | Mittel | ⭐ **Top-Empfehlung** |
| #52 Audit Trail | — | ✅ | — | — | ✅ AuditEntry | Niedrig | Guter Einstieg, zeigt aber nur Events |
| #53 Withdraw | — | ✅ | — | — | — | Niedrig | Aufwärmübung, kein neues Pattern |
| #54 Vier-Augen-Prinzip | ✅ | ✅ | ✅ | — | — | Hoch | Zu komplex für 4h Schulung |
| #55 Externe Bonitätsabfrage | — | ✅ | — | ✅ | — | Mittel-Hoch | Strategisches DDD, anderer Fokus |

**Empfohlene Reihenfolge für Schulung:**
1. **#53 Withdraw** als Aufwärmübung (30 min) — neuer Status, Guard Clause, Event
2. **#51 Auto-Decision + Notification** als Hauptübung (2–3h) — Policy, Event Handler, Specification, Dispatcher-Erweiterung
3. **#52 Audit Trail** als Bonus wenn Zeit bleibt (30 min) — alle Events bekommen Handler

---

## Priorisierungsempfehlung

| Prio | Use Cases | Begründung |
|------|-----------|------------|
| 🔴 Hoch | #25 Kreditbetrag + Laufzeit, #36 Antrag zurückziehen | Fundamentale fachliche Lücken |
| 🔴 Hoch | #32 Bestehende Verbindlichkeiten, #33 SCHUFA-Einwilligung | Scoring ohne Gesamtbild ist unvollständig |
| 🟡 Mittel | #44 E-Mail-Benachrichtigungen, #45 Erinnerungen | Event-Infrastruktur existiert bereits |
| 🟡 Mittel | #39 Vier-Augen-Prinzip, #47 Bearbeitungsfristen | Wichtig bei mehreren Processors |
| 🟡 Mittel | #29 Erweiterte Personendaten, #49 Audit Trail | Realismus und Compliance |
| 🟢 Niedrig | #34 Externe Bonitätsabfrage, #35 KYC | Kann als Interface vorbereitet werden |
| 🟢 Niedrig | #41–#43 Vertrag & Auszahlung | Eigener Bounded Context, spätere Phase |
| 🟢 Niedrig | #26 Zinssatz, #28 Tilgungsplan | Nice-to-have, erhöht Realismus |
