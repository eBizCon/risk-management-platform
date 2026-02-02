---
name: analyse-bug
description: Bug analysieren und Bug-Analyse-Dokument erstellen. Verwende diesen Skill wenn der Nutzer einen Bug analysieren, verstehen oder dokumentieren möchte. Führt einen strukturierten Diskurs mit dem Entwickler und erstellt eine standardisierte Bug-Analyse als Markdown-Datei.
---

# Analyse Bug

## Zweck
Dieser Skill nimmt eine Bugbeschreibung entgegen und führt einen strukturierten Diskurs mit dem Entwickler, um den Bug sauber zu verstehen.

Das Ergebnis ist eine standardisierte Bug-Analyse als Markdown-Datei unter:

backlog/bugfixes/<bugname>-bug-analyse.md

Keine Codeänderungen, keine Fix-Implementierung.


## Input Contract
Pflicht:
- bug_description

Optional:
- expected_behavior
- actual_behavior
- steps_to_reproduce
- logs_or_errors
- environment


## Hard Rules
- Es dürfen keine Codeänderungen vorgenommen werden
- Es dürfen keine Fixes implementiert werden
- Der Skill arbeitet interaktiv im Diskurs mit dem Entwickler
- Maximal 5 Rückfragen pro Diskurs-Runde
- Jede Hypothese braucht einen Prüfplan
- Output ist immer eine Markdown-Datei im Backlog
- Berücksichtige die Windsurf Rules in `.windsurf/rules/` (Code-Style, Testing, Backend-Architektur) bei der Analyse


## Diskurs-Driven Workflow
Der Skill ist bewusst nicht "one-shot", sondern arbeitet in Iterationen.

### Phase 1 — Intake und Rückfragen (Diskurs Gate)
Wenn wichtige Informationen fehlen, muss der Skill zuerst Rückfragen stellen.

Der Skill fragt gezielt nach:
- Ist der Bug reproduzierbar?
- Wo tritt er auf (Prod/Stage/Local)?
- Was ist Expected vs Actual?
- Gibt es Logs oder Fehlermeldungen?
- Welche User-Flows sind betroffen?

Regel:
- Stelle maximal 5 Fragen
- Stoppe danach und warte auf Entwicklerantwort

Beispiel-Fragen:
1. Kannst du den Bug aktuell lokal reproduzieren?
2. In welcher Umgebung tritt er auf (Prod, Stage, Local)?
3. Was ist das erwartete Verhalten statt des aktuellen?
4. Gibt es einen Stacktrace oder konkrete Fehlermeldungen?
5. Passiert es immer oder nur unter bestimmten Bedingungen?

Erst nach Antworten geht es weiter.


### Phase 2 — Repro Plan bestätigen (Diskurs Gate)
Sobald genug Informationen vorhanden sind, erstellt der Skill einen Repro-Plan.
Dann muss er explizit im Chat fragen:
"Bestätigst du, dass diese Repro-Schritte korrekt sind?"
Wenn Entwickler nicht bestätigt:
- Plan anpassen
- keine Analyse abschließen

### Phase 3 — Hypothesen diskutieren (Diskurs Gate)
Der Skill formuliert maximal 4 Root-Cause-Hypothesen.
Zu jeder Hypothese:
- Warum plausibel
- Wie prüfen
- Welche Komponente vermutlich betroffen ist

Dann fragt der Skill:
"Welche Hypothese sollen wir als erstes validieren?"
Stop, bis Entwickler entscheidet.

### Phase 4 — Analyse dokumentieren
Erst nach bestätigtem Kontext erzeugt der Skill die finale Analyse-Datei.
Pfad:
backlog/bugfixes/<bugname>-bug-analyse.md

## Output Contract
Der Skill liefert:
1. Eine Markdown-Datei mit Analyse
2. Eine kurze Chat-Zusammenfassung
3. Offene Blocker-Fragen oder Next Steps


## Markdown Template
Verwende das Template aus [assets/bug-analysis-template.md](assets/bug-analysis-template.md)