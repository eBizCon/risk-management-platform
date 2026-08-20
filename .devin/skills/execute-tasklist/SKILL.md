---
name: execute-tasklist
description: Führt Tasks aus einer JSON Task-Liste mit persistiertem Progress-Tracking aus. Verwende diesen Skill wenn der Nutzer eine Task-Liste abarbeiten möchte (z.B. von create-tasklist-from-implementation-plan), offene Tasks ausführen, Fortschritt in einem git-freundlichen Format tracken oder eine unterbrochene Task-Ausführung fortsetzen möchte. Unterstützt Single-Task, Batch (ALL) und manuelle Auswahl.
---

# Execute Tasklist

Führt Tasks aus einer JSON Task-Liste mit git-freundlicher Progress-Persistierung aus.

## Input

- **tasks_file**: Pfad zur JSON Task-Liste, z.B. `backlog/implementations/<story>/<story>-tasks.json`

## Progress File Konvention

- **Pfad**: `<tasks_file>.progress.json`
- **Automatisch erstellt** falls nicht vorhanden, mit allen Tasks auf `pending`

## Status-Werte

| Status | Beschreibung |
|--------|--------------|
| `pending` | Noch nicht gestartet |
| `in_progress` | Wird gerade ausgeführt |
| `completed` | Erfolgreich abgeschlossen |
| `failed` | Fehlgeschlagen (mit Begründung) |
| `blocked` | Kann nicht fortfahren (Dependency/Blocker) |
| `skipped` | Bewusst übersprungen |

## Benutzer-Kommandos

| Kommando | Aktion |
|----------|--------|
| `JA` | Nächsten offenen Task ausführen |
| `ALL` | Alle offenen Tasks ohne weiteres Approval ausführen |
| `NEIN` | Manuelle Auswahl (Task-ID oder Kommandos) |
| `<Task-ID>` | Spezifischen Task ausführen (falls nicht completed) |
| `BLOCK <ID>` | Als blocked markieren (fragt nach Begründung) |
| `SKIP <ID>` | Als skipped markieren (fragt nach Begründung) |
| `STOP` | Workflow beenden |

## Workflow

### 1. Init/Resume

1. `tasks_file` einlesen
2. Progress-File-Pfad bestimmen: `<tasks_file>.progress.json`
3. Falls Progress-File **nicht existiert**:
   - Alle Tasks mit `status: "pending"`, `attempts: 0` initialisieren
   - `meta` setzen (source_tasks_file, createdAt, lastUpdated, version)
   - `summary` setzen (Anzahl pro Status)
   - `history`-Eintrag hinzufügen: `action: "init"`
4. Falls Progress-File **existiert**:
   - Mit tasks_file synchronisieren (neue Tasks als pending, entfernte als skipped)
   - Eindeutige Task-IDs validieren
   - summary und meta.lastUpdated aktualisieren
   - `history`-Eintrag hinzufügen: `action: "resume"`

### 2. Auswahl-Modus

Nutzer fragen:
```
Modus wählen:
- JA = nächsten offenen Task ausführen
- ALL = alle offenen Tasks ausführen (ohne weiteres Approval)
- NEIN = Task-ID oder Kommandos (BLOCK <ID>, SKIP <ID>, STOP)
```

- **JA** → `execution_mode = "single"`, nächsten pending Task auswählen
- **ALL** → `execution_mode = "all_pending"`, alle pending Tasks iterieren
- **NEIN** → nach Task-ID oder Kommando fragen
  - `BLOCK <ID>` / `SKIP <ID>`: Status setzen, Begründung in notes, history-Eintrag, zurück zu Schritt 2
  - `STOP`: Finalen Stand schreiben, Workflow beenden

### 3. Task ausführen

Für jeden ausgewählten Task:

1. **Tracking starten**:
   - `status: "in_progress"` setzen
   - `attempts` erhöhen
   - `startedAt` setzen
   - history hinzufügen: `action: "start"`
   - Progress-File schreiben

2. **Implementierung**:
   - Skill `implement-task-from-tasklist` aufrufen mit:
     - `task`: aktuelles Task-Objekt
     - `tasklist`: komplette Task-Liste (readonly)
   - Task's `goal`, `implementation_details`, `artifacts`, `scope`, `out_of_scope`, `acceptance_criteria`, `constraints` befolgen
   - `global_constraints` vom Root-Level und `user_story`-Kontext berücksichtigen

3. **Verifikation**:
   - Alle Checks aus `task.checks` ausführen (z.B. `npm test`, `npm run lint`)
   - Bei Erfolg:
     - `status: "completed"`, `finishedAt` setzen
     - `artifacts_written` aktualisieren
     - history hinzufügen: `action: "complete"`
   - Bei Fehler:
     - `status: "failed"`, `finishedAt`, `failure_reason` setzen
     - history hinzufügen: `action: "fail"`
   - summary aktualisieren, Progress-File schreiben

### 4. Ende-Bedingung

- Falls keine pending Tasks mehr: Summary ausgeben, Workflow beenden
- Falls pending Tasks existieren:
  - `all_pending`-Modus: Batch abgeschlossen, zurück zu Schritt 2
  - `single`-Modus: zurück zu Schritt 2

### 5. Resume

Workflow ist jederzeit fortsetzbar:
- Liest existierendes Progress-File
- Nutzt summary/tasks/history um nächsten Task zu bestimmen
- Completed Tasks werden nie erneut ausgeführt
- Failed Tasks erfordern explizite Task-ID-Auswahl zum Retry

## Progress File Schema

Siehe [references/progress-schema.json](references/progress-schema.json) für das vollständige JSON Schema.

Kernstruktur:
```json
{
  "meta": { "source_tasks_file", "createdAt", "lastUpdated", "version" },
  "summary": { "total", "pending", "in_progress", "completed", "failed", "blocked", "skipped" },
  "tasks": [{ "id", "title", "status", "attempts", "startedAt?", "finishedAt?", "failure_reason?", "artifacts_written?", "notes?" }],
  "history": [{ "ts", "taskId", "action", "details?" }]
}
```

## Zu befolgende Regeln

- **Backend Architecture**: Transport-Layer (Routes) vs Services vs Repositories; DB-Zugriff nur in Repositories; Zod-Validierung in API-Routes
- **E2E Tests**: Stabile `data-testid`-Attribute verwenden (kebab-case, semantisch)
