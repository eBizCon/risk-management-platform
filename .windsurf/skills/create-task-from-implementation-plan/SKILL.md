---
name: create-tasklist-from-implementation-plan
description: Takes the output of "plan-user-story-implementation-blueprint" (implementation blueprint in markdown) and produces a strict JSON task list. Each task contains all relevant blueprint details (incl. user story + constraints if present) so a separate executor skill can implement tasks iteratively without needing the original blueprint.
---

# Split Implementation Blueprint → Tasks (JSON only)

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
- Wenn keine eindeutig ableitbaren Constraints vorhanden sind: `global_constraints: []`.

Du darfst Constraints zusammenfassen, aber keine erfinden.

---

## Output (STRICT) — JSON ONLY
Antworte ausschließlich mit validem JSON in exakt diesem Schema:

```json
{
  "source_skill": "plan-user-story-implementation-blueprint",
  "user_story": {
    "title": "…",
    "as_a": "…",
    "i_want": "…",
    "so_that": "…"
  },
  "global_constraints": [
    "…"
  ],
  "tasks": [
    {
      "id": "T-001",
      "title": "Kurzer Task-Titel",

      "user_story": {
        "title": "…",
        "as_a": "…",
        "i_want": "…",
        "so_that": "…"
      },

      "constraints": [
        "global_constraints (kopiert)",
        "task-spezifische Constraints (nur wenn im Blueprint genannt)"
      ],

      "goal": "Überprüfbares Ergebnis in 1 Satz",

      "blueprint_references": [
        "Abschnitt 4: <Datei/Änderungspunkt>",
        "Abschnitt 6: <Testfall>",
        "Abschnitt 5: <Contract/Migration>"
      ],

      "implementation_details": [
        "Konkrete Details/Steps aus dem Blueprint (nahe am Wortlaut, keine neuen Features)."
      ],

      "scope": [
        "Was wird umgesetzt (max 5 bullets, klar abgegrenzt)"
      ],
      "out_of_scope": [
        "Was explizit nicht Teil dieses Tasks ist"
      ],

      "artifacts": [
        "Dateipfade/Ordner",
        "Klassen/Methoden",
        "Contracts/DTOs",
        "Migrationen"
      ],

      "acceptance_criteria": [
        "Messbare Done-Bedingung 1",
        "Messbare Done-Bedingung 2"
      ],

      "checks": [
        "Konkrete Commands oder Checks aus dem Blueprint (z. B. mvn test, gradle test, dotnet test, npm test, lint). Wenn nicht genannt: minimal plausible Checks als Text, ohne neue Tools zu erfinden."
      ],

      "notes": [
        "Nur falls nötig: Annahmen/Risiken aus Blueprint Abschnitt 2/7, die für die Umsetzung relevant sind."
      ]
    }
  ]
}
