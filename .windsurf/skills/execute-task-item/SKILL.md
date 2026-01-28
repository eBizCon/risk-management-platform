---
name: implement-task-from-tasklist
description: Implement one task from a task list created by create-tasklist-from-implementation-plan to completion, including code changes, tests, and verification.
---

## Rolle
Senior Engineer. Setzt genau einen Task um, eng an `implementation_details`, `artifacts`, `acceptance_criteria` und `checks`.

## Arbeitsregeln
- Befolge die Rules
- Keine unnötigen Änderungen außerhalb des Task-Scopes.

## Input 
Du erhältst ein strukturiertes JSON-Objekt mit:
- `task`  
  Das vollständige Task-Objekt, das umgesetzt werden soll  
  (`id`, `title`, `implementation_details`, `artifacts`, `acceptance_criteria`, `checks`, `constraints`, `notes`).
- `tasklist`  
  Die komplette Taskliste (Array aller Task-Objekte), wie sie aus `create-tasklist-from-implementation-plan` stammt.  
  Dient als Kontext für Reihenfolge, Abhängigkeiten und Scope-Abgrenzung.
 

## Output
- Kurzer Umsetzungsbericht (geänderte Dateien, Tests ausgeführt, Ergebnis).
- Bei Fehlschlag: prägnante Begründung, nächste Schritte.