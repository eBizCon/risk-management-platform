---
description: Execute tasks from a JSON task list with persisted progress (git-commit-friendly)
---

## Zweck
Strukturierter Workflow zum Abarbeiten einer JSON-Taskliste (z. B. Output von `split-implementation-blueprint-into-tasks`).  
Der User kann entweder den nächsten offenen Task oder eine konkrete Task-ID auswählen.  
Der Fortschritt wird in einer separaten JSON-Datei persistiert, die ins Git eingecheckt werden kann.

## Inputs
- **tasks_file**  
  Pfad zur JSON-Taskliste, z. B.  
  `backlog/implementations/<user-story-name>/<user-story-name>-tasks.json`

## Konvention: Progress-Datei
- **Automatische Anlage**  
  Die Fortschrittsdatei wird automatisch angelegt, wenn sie nicht existiert.
- **Pfad-Konvention**  
  `<tasks_file>.progress.json`  
  Beispiel:  
  `backlog/implementations/foo/foo-tasks.json`  
  → `backlog/implementations/foo/foo-tasks.json.progress.json`

## Status-Enum
Folgende Statuswerte sind erlaubt:

- **pending**  
  Task ist noch nicht begonnen.
- **in_progress**  
  Task wird aktuell bearbeitet.
- **completed**  
  Task ist erfolgreich abgeschlossen.
- **failed**  
  Task wurde versucht, aber ist aktuell fehlgeschlagen (mit Begründung).
- **blocked**  
  Task kann aktuell nicht bearbeitet werden (Abhängigkeit/Blocker, mit Begründung).
- **skipped**  
  Task wurde bewusst übersprungen (mit Begründung).

## Schritte

1. **Init/Resume**
   - Lese `tasks_file`.
   - Bestimme den Pfad der Progress-Datei: `<tasks_file>.progress.json`.
   - Wenn die Progress-Datei **nicht existiert**:
     - Initialisiere für jede Task-ID aus `tasks_file` einen Eintrag mit:
       - `status = "pending"`
       - `attempts = 0`
     - Setze `meta` (inkl. `source_tasks_file`, `createdAt`, `lastUpdated`, `version`).
     - Setze `summary` (Counts pro Status).
     - Setze `history` mit einem ersten `init`-Event.
     - Schreibe die Datei an `<tasks_file>.progress.json`.
   - Wenn die Progress-Datei **existiert**:
     - Lese sie ein.
     - Synchronisiere mit `tasks_file`:
       - Neue Tasks in `tasks_file`, die noch nicht in der Progress-Datei existieren → als `pending` hinzufügen.
       - Tasks, die nicht mehr in `tasks_file` vorkommen → optional auf `skipped` setzen und in `notes` markieren.
     - Validiere:
       - Jede `tasks[*].id` existiert genau einmal.
     - Aktualisiere `summary` und `meta.lastUpdated`.
     - Schreibe die Datei zurück.

2. **Auswahl des Tasks**
   - Ermittle `next_pending`: die erste Task in der Reihenfolge aus `tasks_file`, deren Status `pending` ist.
   - Frage den User interaktiv:
     - „Nächsten offenen Task ausführen? (JA / NEIN)“
       - Bei JA → wähle `next_pending`.
       - Bei NEIN → frage:
         - „Gib eine Task-ID an, oder einen Befehl:  
           - `BLOCK <ID>` zum Blockieren  
           - `SKIP <ID>` zum Überspringen  
           - `STOP` zum Beenden“
   - Verhalten:
     - Bei Eingabe einer Task-ID:
       - Wähle diesen Task, sofern sein Status nicht `completed` ist.
     - Bei `BLOCK <ID>`:
       - Frage nach einer kurzen Begründung.
       - Setze Status auf `blocked`, aktualisiere `notes` und `history`, schreibe Progress-Datei, gehe zurück zu Schritt 2.
     - Bei `SKIP <ID>`:
       - Frage nach einer kurzen Begründung.
       - Setze Status auf `skipped`, aktualisiere `notes` und `history`, schreibe Progress-Datei, gehe zurück zu Schritt 2.
     - Bei `STOP`:
       - Schreibe einen finalen Stand in die Progress-Datei und beende den Workflow.

3. **Start-Tracking für gewählten Task**
   - Für den gewählten Task:
     - Setze `status = "in_progress"`.
     - Erhöhe `attempts` um `1`.
     - Setze `startedAt = now` (ISO-Zeitstempel).
   - Aktualisiere `summary` und `meta.lastUpdated`.
   - Füge einen Eintrag in `history` mit `action = "start"` hinzu.
   - Schreibe die Progress-Datei.

4. **Umsetzung des Tasks**
   - Nutze die Informationen aus dem Task-Objekt aus `tasks_file`:
     - `implementation_details`
     - `artifacts`
     - `scope`
     - `acceptance_criteria`
     - `checks`
     - `constraints`
     - `notes`
   - Implementiere ausschließlich im Scope des Tasks.
   - Beachte dabei explizit:
     - Backend Architecture Rule:
       - Transport-Layer (SvelteKit-Routes) vs. Services vs. Repositories.
       - DB-Zugriffe nur in Repositories.
       - Zod-Validation in API-Routes/Form-Actions.
     - E2E-Test-Regel:
       - Verwende stabile `data-testid`-Attribute (kebab-case, sprechend) für relevante UI-Elemente.
   - Optional:
     - Verwende den Skill `implement-task-from-blueprint`, um einen einzelnen Task strukturiert umzusetzen.

5. **Verifikation**
   - Führe nacheinander alle in `task.checks` genannten Checks aus (z. B. `npm test`, `npm run lint`, `npm run test:e2e`, etc.), sofern im Task spezifiziert.
   - Dokumentiere Ergebnis:
     - Wenn alle relevanten Checks erfolgreich:
       - Setze `status = "completed"`.
       - Setze `finishedAt = now`.
       - Aktualisiere `artifacts_written` (Liste geänderter/neu angelegter Dateien).
       - Füge Eintrag in `history` mit `action = "complete"` hinzu.
     - Wenn ein Check fehlschlägt oder der Task fachlich nicht erfüllt werden kann:
       - Setze `status = "failed"`.
       - Setze `finishedAt = now`.
       - Setze `failure_reason` auf eine kurze, präzise Begründung (inkl. relevanter Fehlermeldung/Tests).
       - Füge Eintrag in `history` mit `action = "fail"` hinzu.
   - Aktualisiere `summary` und `meta.lastUpdated`.
   - Schreibe die Progress-Datei.

6. **Ende-Bedingung**
   - Prüfe, ob es noch `pending`-Tasks gibt.
   - Wenn **keine** `pending`-Tasks mehr vorhanden sind:
     - Gib eine Zusammenfassung aus:
       - Anzahl `completed`, `failed`, `blocked`, `skipped`.
     - Schreibe einen finalen Zustand in die Progress-Datei.
     - Beende den Workflow.
   - Wenn noch `pending`-Tasks existieren:
     - Gehe zurück zu Schritt 2.

7. **Wiederaufnahme**
   - Der Workflow ist jederzeit wiederaufnehmbar:
     - Er liest die vorhandene Progress-Datei ein.
     - Nutzt `summary`, `tasks` und `history`, um den nächsten sinnvollen Task zu bestimmen.
     - Bereits `completed`-Tasks werden nicht erneut gestartet.

## JSON-Schema der Progress-Datei

Die automatisch angelegte Progress-Datei `<tasks_file>.progress.json` hat folgendes Schema (vereinfacht als JSON Schema):

```json
{
  "$schema": "[http://json-schema.org/draft-07/schema#](http://json-schema.org/draft-07/schema#)",
  "title": "TaskListProgress",
  "type": "object",
  "required": ["meta", "summary", "tasks", "history"],
  "properties": {
    "meta": {
      "type": "object",
      "required": ["source_tasks_file", "createdAt", "lastUpdated", "version"],
      "properties": {
        "source_tasks_file": { "type": "string" },
        "createdAt": { "type": "string", "format": "date-time" },
        "lastUpdated": { "type": "string", "format": "date-time" },
        "version": { "type": "integer", "minimum": 1 }
      },
      "additionalProperties": false
    },
    "summary": {
      "type": "object",
      "required": [
        "total",
        "pending",
        "in_progress",
        "completed",
        "failed",
        "blocked",
        "skipped"
      ],
      "properties": {
        "total": { "type": "integer", "minimum": 0 },
        "pending": { "type": "integer", "minimum": 0 },
        "in_progress": { "type": "integer", "minimum": 0 },
        "completed": { "type": "integer", "minimum": 0 },
        "failed": { "type": "integer", "minimum": 0 },
        "blocked": { "type": "integer", "minimum": 0 },
        "skipped": { "type": "integer", "minimum": 0 }
      },
      "additionalProperties": false
    },
    "tasks": {
      "type": "array",
      "items": {
        "type": "object",
        "required": ["id", "title", "status", "attempts"],
        "properties": {
          "id": { "type": "string" },
          "title": { "type": "string" },
          "status": {
            "type": "string",
            "enum": [
              "pending",
              "in_progress",
              "completed",
              "failed",
              "blocked",
              "skipped"
            ]
          },
          "attempts": { "type": "integer", "minimum": 0 },
          "startedAt": { "type": ["string", "null"], "format": "date-time" },
          "finishedAt": { "type": ["string", "null"], "format": "date-time" },
          "failure_reason": { "type": ["string", "null"] },
          "artifacts_written": {
            "type": "array",
            "items": { "type": "string" }
          },
          "notes": {
            "type": "array",
            "items": { "type": "string" }
          }
        },
        "additionalProperties": false
      }
    },
    "history": {
      "type": "array",
      "items": {
        "type": "object",
        "required": ["ts", "taskId", "action"],
        "properties": {
          "ts": { "type": "string", "format": "date-time" },
          "taskId": { "type": "string" },
          "action": {
            "type": "string",
            "enum": [
              "init",
              "start",
              "complete",
              "fail",
              "block",
              "skip",
              "resume",
              "update"
            ]
          },
          "details": { "type": "string" }
        },
        "additionalProperties": false
      }
    }
  },
  "additionalProperties": false
}