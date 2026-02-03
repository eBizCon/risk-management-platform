# Skills & Workflows Übersicht

## Quelle der Wahrheit

Die Inhalte dieser Übersicht sind aus den im Repo vorhandenen Definitionen abgeleitet:

- **Workflows**: `.windsurf/workflows/*.md`
- **Skills**: `.windsurf/skills/*/SKILL.md`

## Beziehung (Input → Output)

```text
Anforderung / Use Case
  -> (Workflow) /create-and-plan-user-story  (siehe .windsurf/workflows/create-and-plan-user-story.md)
       -> (Skill) create-user-story
       -> (Skill) refine-user-story
       -> Approval Gate: "User Story freigeben? (JA / Änderungen)"
       -> (Skill) plan-user-story-implementation-blueprint
       -> Approval Gate: "Implementierungsplan freigeben? (JA / Änderungen)"
       -> optional: Dokumentation als Markdown erzeugen
       -> optional: Taskliste als JSON erzeugen

Vorhandene JSON Taskliste
  -> (Skill) execute-tasklist
       -> iterativ (Skill) implement-task-from-tasklist
       -> persistiert Fortschritt in <tasks_file>.progress.json
       -> führt Checks aus task.checks aus
```

## Entscheidungsleitfaden (Wann nutze ich was?)

- **Du hast nur eine grobe Idee / Use Case**
  - Nutze: `/create-and-plan-user-story`
- **Du hast schon eine gute User Story und willst „nur“ die technische Planung + Tasks**
  - Nutze: `/plan-user-story`
- **Du hast bereits eine JSON Taskliste und willst sie umsetzen**
  - Nutze: Skill `execute-tasklist`
- **Du willst gezielt einen einzelnen Schritt machen (ohne Orchestrierung)**
  - Nutze: den passenden Skill direkt (siehe Tabellen unten)
- **Du willst einen Bug verstehen/dokumentieren, aber noch nicht fixen**
  - Nutze: `analyse-bug`
- **Du willst Tests reproduzierbar ausführen**
  - Nutze: `run-e2e`

## Workflows

| Workflow | Zweck | Input | Ruft Skills auf |
|----------|-------|-------|-----------------|
| `/create-and-plan-user-story` | Kompletter Prozess: Von Idee zu User Story zu Planung zu Taskliste (inkl. Approval Gates) | Freitext-Anforderung | create-user-story → refine-user-story → plan-user-story-implementation-blueprint → create-tasklist-from-implementation-plan |
| `/plan-user-story` | Nur Planung einer bereits vorhandenen User Story | Ausgearbeitete User Story | plan-user-story-implementation-blueprint → create-tasklist-from-implementation-plan |

**Artefakte** (optional, wenn User zustimmt):
- Blueprint: `backlog/implementations/<user-story-name>/<user-story-name>.md`
- Taskliste: `backlog/implementations/<user-story-name>/<user-story-name>-tasks.json`

## Skills

| Skill | Zweck | Input | Output |
|-------|-------|-------|--------|
| `create-user-story` | Erstellt User Story aus Anforderung | Anforderungsbeschreibung | User Story (INVEST, Gherkin ACs) |
| `refine-user-story` | Validiert und verbessert bestehende User Story | User Story | Refinierte User Story + Hinweise |
| `plan-user-story-implementation-blueprint` | Erstellt technischen Umsetzungsplan | User Story | Blueprint (Dateien, Methoden, Tests) |
| `create-tasklist-from-implementation-plan` | Zerlegt Blueprint in Tasks | Blueprint (Markdown) | JSON-Taskliste |
| `execute-tasklist` | Führt Tasks aus einer JSON-Taskliste aus | JSON-Taskliste | Code-Änderungen + Progress-Tracking |
| `implement-task-from-tasklist` | Setzt einen einzelnen Task um | Task + Taskliste | Code-Änderungen + Bericht |
| `analyse-bug` | Strukturierte Bug-Analyse (ohne Fix) | Bug-Beschreibung | backlog/bugfixes/name-bug-analyse.md |
| `run-e2e` | E2E-Tests ausführen | - | Testergebnisse |
| `git-review` | Review staged git changes | Staged Changes | Review-Datei unter reviews/ |
| `skill-creator` | Erstellt/aktualisiert Skills für Windsurf | Skill-Anforderung | .skill Datei (Paket) |

## Skill-Hinweise (repo-spezifisch)

### `execute-tasklist` (Skill)

Quelle: `.windsurf/skills/execute-tasklist/SKILL.md`

- **Input**: Pfad zu einer JSON Taskliste (z.B. `backlog/implementations/<name>/<name>-tasks.json`)
- **Wichtiges Verhalten**:
  - Legt/aktualisiert automatisch eine Progress-Datei: `<tasks_file>.progress.json`
  - Unterstützt User-Kommandos: `JA`, `ALL`, `NEIN`, `<Task-ID>`, `BLOCK <ID>`, `SKIP <ID>`, `STOP`
  - Ruft pro Task den Skill `implement-task-from-tasklist` auf

### `run-e2e`

Quelle: `.windsurf/skills/run-e2e/SKILL.md`

- Führt laut Skill-Workflow aus:
  - `npm run test:e2e:ci`

### `git-review`

Quelle: `.windsurf/skills/git-review/SKILL.md`

- Nutze diesen Skill, wenn du staged Änderungen reviewen willst.
- Der Skill erzeugt eine Review-Datei unter `reviews/` (Dateiname `review-YYYY-MM-DD-HHmmss.md`).

---

## Ablauf: Vollständiger Feature-Lifecycle

```text
1. Anforderung / Use Case (Input)
2. create-user-story (Skill) → Erstellt User Story
3. refine-user-story (Skill) → Validiert und verbessert
   └── Approval Gate: „User Story freigeben?"
4. plan-user-story-implementation-blueprint (Skill) → Technischer Plan
   └── Approval Gate: „Implementierungsplan freigeben?"
5. create-tasklist-from-implementation-plan (Skill) → JSON Taskliste
6. JSON Taskliste gespeichert unter backlog/implementations/<name>/<name>-tasks.json
7. execute-tasklist (Skill) → Führt Tasks aus via implement-task-from-tasklist
   └── Progress-Tracking in <tasks_file>.progress.json
```

---

## Häufige Patterns

- **Wenn du merkst, dass du Annahmen triffst**
  - Geh einen Schritt zurück und nutze den Workflow `/create-and-plan-user-story` (durch die Approval Gates fängt er Unklarheiten früh ab).
- **Wenn du „zu große“ Tasks bekommst**
  - Passe das Blueprint an (kleiner schneiden) und lasse die Taskliste neu erzeugen.
- **Wenn du nur schnell einen einzelnen Task implementieren willst**
  - Nutze `implement-task-from-tasklist` direkt mit einem ausgewählten Task (statt alle Tasks zu starten).
