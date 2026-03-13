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
