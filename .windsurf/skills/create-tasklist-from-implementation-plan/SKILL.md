---
name: create-tasklist-from-implementation-plan
description: Nimmt das Ergebnis von "plan-user-story-implementation-blueprint" (Implementation Blueprint in Markdown) und erzeugt eine strikte JSON Task-Liste. Jeder Task enthält alle relevanten Blueprint-Details (inkl. User Story + Constraints), sodass ein separater Executor-Skill Tasks iterativ umsetzen kann ohne das Original-Blueprint zu benötigen.
---

# Create Tasklist from Implementation Plan

## Zweck
Du erhältst als Input **ausschließlich** das Markdown-Blueprint aus dem Skill:
`plan-user-story-implementation-blueprint`.

Deine einzige Aufgabe ist es:
- das Blueprint in **kleine, gekapselte Tasks** zu zerlegen
- pro Task **alle relevanten Details** aus dem Blueprint mitzunehmen
- als Output **nur** validen JSON zu liefern, damit ein anderer Skill später genau **einen Task** übernehmen und umsetzen kann.

Du implementierst nichts. Du erklärst nichts. Du gibst ausschließlich JSON aus.

---

## Input (erwartete Struktur)
Das Blueprint enthält typischerweise die Abschnitte:
1) Implementierungsziel  
2) Annahmen & offene Fragen  
3) Impact Map  
4) Änderungsplan auf Code-Ebene (Developer To-Do)  
5) Daten- & Contract-Änderungen  
6) Testplan  
7) Risiken & Abhängigkeiten  

WICHTIG: User Story / Constraints können im Blueprint entweder explizit stehen oder implizit über Regeln/Abschnitte abgeleitet werden.

---

## Extraktion: User Story & Constraints
### User Story
- Wenn im Blueprint eine User Story explizit enthalten ist (z. B. „As a / I want / so that“ oder klarer Story-Block), extrahiere diese.
- Wenn nicht explizit enthalten: setze `user_story` auf `null` und **kopiere keine erfundenen Inhalte**.

### Constraints
- Wenn im Blueprint ein expliziter Constraints/Guardrails-Block existiert: extrahiere ihn als `global_constraints`.
- Wenn nicht: setze `global_constraints` auf eine Liste von Constraints, die **explizit** im Blueprint genannt sind (z. B. „keine vollständige Implementierung“, Teststil, Logging/Telemetry, Security-Aspekte).
- Berücksichtige auch die Windsurf Rules in `.windsurf/rules/` (Backend-Architektur, Code-Style, Testing, Styling) als implizite Constraints.
- Wenn keine eindeutig ableitbaren Constraints vorhanden sind: `global_constraints: []`.

Du darfst Constraints zusammenfassen, aber keine erfinden.

---

## Output (STRICT) — JSON ONLY
Antworte ausschließlich mit validem JSON gemäß dem Schema in [assets/task-schema.json](assets/task-schema.json)
