---
description: Execute tasks from a JSON task list with persisted progress (git-commit-friendly)
---

## Zweck
Strukturierter Workflow zum Abarbeiten einer JSON-Taskliste (z. B. Output von `split-implementation-blueprint-into-tasks`).  
Der User kann entweder den nächsten offenen Task oder eine konkrete Task-ID auswählen.  
Der Fortschritt wird in einer separaten JSON-Datei persistiert, die ins Git eingecheckt werden kann.

Erweiterung (Batch-Modus):
- Der User kann per Eingabe anweisen: **alle offenen (pending) Tasks ohne weiteres Approval ausführen**.

## Inputs
- **tasks_file**  
  Pfad zur JSON-Taskliste, z. B.  
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

## User-Eingaben (Kommandos)

Der Workflow akzeptiert folgende Eingaben:

- **JA**  
  Führt den nächsten `pending` Task aus.
- **NEIN**  
  Erlaubt Auswahl per Task-ID oder Kommandos (BLOCK/SKIP/STOP).
- **ALL**  
  Führt **alle** `pending` Tasks in Reihenfolge aus `tasks_file` aus, ohne weiteres User-Approval pro Task.
- **Task-ID**  
  Führt die angegebene Task aus, sofern Status nicht `completed` ist.
- **BLOCK <ID>**  
  Markiert Task als `blocked` (mit Begründung), ohne Ausführung.
- **SKIP <ID>**  
  Markiert Task als `skipped` (mit Begründung), ohne Ausführung.
- **STOP**  
  Beendet den Workflow.

Hinweis:
- Im Modus **ALL** werden `blocked`, `skipped`, `completed` Tasks übersprungen.
- Tasks mit Status `failed` werden nicht automatisch erneut ausgeführt (außer der User wählt die Task explizit per ID).

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
   - Schreibe zusätzlich ein `history`-Event:
     - `action = "resume"` (wenn Progress-Datei existierte), sonst nur `init`.

2. **Auswahl-Modus (User-Eingabe)**
   - Ermittle `next_pending`: die erste Task in der Reihenfolge aus `tasks_file`, deren Status `pending` ist.
   - Frage den User interaktiv:

     „Modus wählen:
     - `JA` = nächsten offenen Task ausführen
     - `ALL` = alle offenen Tasks ausführen (ohne weiteres Approval)
     - `NEIN` = Task-ID oder Kommandos (`BLOCK <ID>`, `SKIP <ID>`, `STOP`)“

   - Verhalten:
     - Bei **JA** → setze `execution_mode = "single"` und wähle `next_pending`.
     - Bei **ALL** → setze `execution_mode = "all_pending"` und gehe zu Schritt 3 mit einer Task-Schleife.
     - Bei **NEIN** → frage:
       - „Gib eine Task-ID an, oder einen Befehl:
         - `BLOCK <ID>`
         - `SKIP <ID>`
         - `STOP`“
       - Bei Eingabe einer Task-ID:
         - Wähle diesen Task, sofern sein Status nicht `completed` ist.
         - setze `execution_mode = "single"`.
       - Bei `BLOCK <ID>`:
         - Frage nach kurzer Begründung.
         - Setze Status auf `blocked`, aktualisiere `notes` und `history` (action=`block`), schreibe Progress-Datei, gehe zurück zu Schritt 2.
       - Bei `SKIP <ID>`:
         - Frage nach kurzer Begründung.
         - Setze Status auf `skipped`, aktualisiere `notes` und `history` (action=`skip`), schreibe Progress-Datei, gehe zurück zu Schritt 2.
       - Bei `STOP`:
         - Schreibe einen finalen Stand in die Progress-Datei und beende den Workflow.

3. **Start-Tracking (Single oder All)**
   - Wenn `execution_mode = "single"`:
     - Starte nur den gewählten Task (siehe Schritt 3a–5).
   - Wenn `execution_mode = "all_pending"`:
     - Erzeuge eine Liste `pending_tasks_in_order`:
       - alle Tasks aus `tasks_file` in Reihenfolge, deren Progress-Status `pending` ist.
     - Wenn keine `pending` Tasks vorhanden sind:
       - Gib Zusammenfassung aus und beende (Schritt 6).
     - Sonst:
       - Iteriere über `pending_tasks_in_order` und führe Schritte 3a–5 für jede Task aus,
         ohne weitere User-Interaktion zwischen den Tasks.

3a. **Start-Tracking für gewählten Task**
   - Für den aktuellen Task:
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
   - Der Workflow ruft für die Umsetzung **immer** den Skill `implement-task-from-tasklist`
     - Übergib dem Skill als Input (readonly):
       - `task`: das vollständige Task-Objekt des aktuell ausgewählten Tasks (inkl. `id`, `title`, `implementation_details`, `artifacts`, `acceptance_criteria`, `checks`, `constraints`, `notes`).
       - `tasklist`: die komplette Taskliste aus `tasks_file` (Array aller Task-Objekte, unverändert).

5. **Verifikation**
   - Führe nacheinander alle in `task.checks` genannten Checks aus (z. B. `npm test`, `npm run lint`, `npm run test:e2e`, etc.), sofern im Task spezifiziert.
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
     - Schreibe einen finalen Zustand in die Progress-Datei:
       - optional `history`-Event `action="update"` mit Details "final summary".
     - Beende den Workflow.
   - Wenn noch `pending`-Tasks existieren:
     - Wenn `execution_mode = "all_pending"`:
       - Der Batch-Lauf ist beendet (alle zu Beginn pending waren wurden versucht).
       - Zurück zu Schritt 2 (User entscheidet, ob erneut ALL laufen soll oder gezielt).
     - Sonst:
       - Gehe zurück zu Schritt 2.

7. **Wiederaufnahme**
   - Der Workflow ist jederzeit wiederaufnehmbar:
     - Er liest die vorhandene Progress-Datei ein.
     - Nutzt `summary`, `tasks` und `history`, um den nächsten sinnvollen Task zu bestimmen.
     - Bereits `completed`-Tasks werden nicht erneut gestartet.
   - Empfehlung:
     - Wenn ein Task `failed` ist, sollte der User ihn explizit per Task-ID erneut starten, um unbeabsichtigte Retry-Loops zu vermeiden.

## JSON-Schema der Progress-Datei

Die automatisch angelegte Progress-Datei `<tasks_file>.progress.json` hat folgendes Schema (vereinfacht als JSON Schema):

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
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