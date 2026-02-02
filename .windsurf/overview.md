# Skills & Workflows Übersicht

## Zusammenhang

Workflows orchestrieren mehrere Skills zu einem strukturierten Prozess.
Skills sind einzelne, spezialisierte Aufgaben.

---

## Workflows

| Workflow | Zweck | Ruft Skills auf |
|----------|-------|-----------------|
| `/create-an-plan-user-story` | Kompletter Prozess: Von Idee zu User Story zu Planung zu Taskliste | create-user-story, refine-user-story, plan-user-story-implementation-blueprint, create-tasklist-from-implementation-plan |
| `/plan-user-story` | Nur Planung einer bereits vorhandenen User Story | plan-user-story-implementation-blueprint, create-tasklist-from-implementation-plan |
| `/execute-task-list` | Abarbeiten einer JSON-Taskliste mit Fortschrittsverfolgung | implement-task-from-tasklist (pro Task) |

---

## Skills

| Skill | Zweck | Input | Output |
|-------|-------|-------|--------|
| `create-user-story` | Erstellt User Story aus Anforderung | Anforderungsbeschreibung | User Story (INVEST, Gherkin ACs) |
| `refine-user-story` | Validiert und verbessert bestehende User Story | User Story | Refinierte User Story + Hinweise |
| `plan-user-story-implementation-blueprint` | Erstellt technischen Umsetzungsplan | User Story | Blueprint (Dateien, Methoden, Tests) |
| `create-tasklist-from-implementation-plan` | Zerlegt Blueprint in Tasks | Blueprint (Markdown) | JSON-Taskliste |
| `implement-task-from-tasklist` | Setzt einen einzelnen Task um | Task + Taskliste | Code-Änderungen + Bericht |
| `analyse-bug` | Strukturierte Bug-Analyse (ohne Fix) | Bug-Beschreibung | backlog/bugfixes/name-bug-analyse.md |
| `run-e2e` | E2E-Tests ausführen | - | Testergebnisse |

---

## Ablauf: Vollständiger Feature-Lifecycle

1. **Anforderung / Use Case** (Input)
2. **create-user-story** (Skill) - Erstellt User Story
3. **refine-user-story** (Skill) - Validiert und verbessert (Approval Gate)
4. **plan-user-story-implementation-blueprint** (Skill) - Technischer Plan
5. **create-tasklist-from-implementation-plan** (Skill) - JSON Taskliste
6. **JSON Taskliste** gespeichert unter backlog/implementations/name/name-tasks.json
7. **/execute-task-list** (Workflow) - Führt Tasks aus via implement-task-from-tasklist

---

## Standalone Skills

| Skill | Wann verwenden |
|-------|----------------|
| `analyse-bug` | Bug analysieren ohne sofort zu fixen - erzeugt Analyse-Dokument |
| `run-e2e` | E2E-Tests manuell ausführen |
