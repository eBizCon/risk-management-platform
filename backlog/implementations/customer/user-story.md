# User Story: Kundenverwaltung

## User Story

Als **Applicant (Berater)**
möchte ich **Kunden anlegen, bearbeiten und archivieren sowie Kreditanträge für einen bestimmten Kunden erstellen**
damit **Kreditanträge einem konkreten Kreditnehmer zugeordnet sind und ich meinen Kundenstamm strukturiert verwalten kann**

---

## Kontext

Der Applicant ist ein Bankberater/Vermittler. Der Kunde (Customer) ist die Person, für die ein Kreditantrag gestellt wird. Ein Applicant verwaltet mehrere Kunden und erstellt Anträge für diese. Der Processor kann Kundendaten im Kontext eines Antrags einsehen, aber nicht ändern.

```
Applicant (Berater, OIDC) ──verwaltet──▶ Customer (Kreditnehmer)
                           ──erstellt───▶ Application ──für──▶ Customer
```

---

## Akzeptanzkriterien

### Szenario 1: Kunde anlegen

```gherkin
Given ich bin als Applicant eingeloggt
  And ich befinde mich auf der Kundenverwaltungsseite
When ich einen neuen Kunden mit Name, E-Mail, Adresse, Telefon, Geburtsdatum, Wohnsituation, Familienstand, Arbeitgeber und Beschäftigungsdauer anlege
Then wird der Kunde mit Status "Active" gespeichert
  And der Kunde erscheint in meiner Kundenliste
  And der Kunde ist nur für mich sichtbar (nicht für andere Applicants)
```

### Szenario 2: Kunde bearbeiten

```gherkin
Given ich bin als Applicant eingeloggt
  And ich habe einen aktiven Kunden
When ich die Stammdaten des Kunden ändere
Then werden die Änderungen gespeichert
  And die aktualisierten Daten sind sofort sichtbar
```

### Szenario 3: Kunde archivieren (ohne offene Anträge)

```gherkin
Given ich bin als Applicant eingeloggt
  And ich habe einen aktiven Kunden
  And der Kunde hat keine Anträge im Status Draft, Submitted, NeedsInformation oder Resubmitted
When ich den Kunden archiviere
Then wird der Kunde als "Archived" markiert
  And der Kunde erscheint nicht mehr in der Standard-Kundenliste
```

### Szenario 4: Kunde archivieren (mit offenen Anträgen)

```gherkin
Given ich bin als Applicant eingeloggt
  And ich habe einen aktiven Kunden
  And der Kunde hat mindestens einen Antrag im Status Draft, Submitted, NeedsInformation oder Resubmitted
When ich versuche den Kunden zu archivieren
Then wird die Archivierung abgelehnt
  And ich erhalte einen Hinweis, dass noch offene Anträge existieren
```

### Szenario 5: Kreditantrag für einen Kunden erstellen

```gherkin
Given ich bin als Applicant eingeloggt
  And ich habe mindestens einen aktiven Kunden
When ich einen neuen Kreditantrag erstelle
Then muss ich einen meiner aktiven Kunden auswählen
  And der Antrag wird diesem Kunden zugeordnet
  And die Kundenreferenz ist im Antrag sichtbar
```

### Szenario 6: Kein Antrag für archivierten Kunden

```gherkin
Given ich bin als Applicant eingeloggt
  And ich habe einen archivierten Kunden
When ich einen neuen Kreditantrag erstelle
Then ist der archivierte Kunde nicht in der Kundenauswahl verfügbar
```

### Szenario 7: Processor sieht Kundendaten (lesend)

```gherkin
Given ich bin als Processor eingeloggt
  And ich betrachte einen eingereichten Antrag
When ich die Details des Antrags öffne
Then sehe ich die Stammdaten des zugehörigen Kunden
  And ich kann die Kundendaten nicht bearbeiten
```

---

## Business Rules

- Ein Kunde gehört immer genau einem Applicant (dem Ersteller).
- Ein Applicant sieht nur seine eigenen Kunden.
- Ein Kunde kann mehrere Kreditanträge haben.
- Ein Kreditantrag gehört immer genau einem Kunden.
- Archivierung ist nur möglich, wenn der Kunde keine offenen Anträge hat (Status: Draft, Submitted, NeedsInformation, Resubmitted gelten als "offen").
- Anträge für abgeschlossene (Approved/Rejected) Kunden bleiben erhalten.
- Archivierte Kunden können nicht für neue Anträge ausgewählt werden.
- Der Processor hat ausschließlich Lesezugriff auf Kundendaten.

## Kundendaten (Felder)

| Feld | Pflicht | Beschreibung |
|------|---------|-------------|
| Name | Ja | Vollständiger Name des Kreditnehmers |
| E-Mail | Ja | Kontakt-E-Mail des Kunden |
| Adresse | Ja | Straße, PLZ, Ort |
| Telefon | Nein | Telefonnummer |
| Geburtsdatum | Ja | Für Altersberechnung / Bonitätsprüfung |
| Wohnsituation | Ja | Miete / Eigentum / Sonstiges |
| Familienstand | Ja | Ledig / Verheiratet / Geschieden / Verwitwet |
| Arbeitgeber | Nein | Name des Arbeitgebers |
| Beschäftigungsdauer | Nein | Seit wann beim aktuellen Arbeitgeber |
| Status | Automatisch | Active / Archived |

---

## Abhängigkeiten

- Bestehende Anträge (`Application`) müssen um eine `CustomerId` erweitert werden (DB-Migration).
- Das Antragsformular muss um eine Kundenauswahl erweitert werden.
- Die Processor-Detailansicht muss Kundendaten anzeigen.
- Bestehende Anträge ohne Customer brauchen eine Migrationsstrategie.

## Offene Klärungspunkte [ZU KLÄREN]

- Migrationsstrategie für bestehende Anträge: Sollen bestehende Anträge (ohne Customer) einen Dummy-Kunden bekommen, oder bleibt CustomerId nullable für Legacy-Daten?
- Soll der Name im Antrag (`Application.Name`) durch den Kundennamen ersetzt werden, oder bleibt er als separates Feld erhalten?
- Soll die Kundenliste paginiert/filterbar sein?
