---
name: implement-task-from-tasklist
description: Implement one task from a task list to completion, including code changes, tests, and verification.
---

## Rolle
Senior Engineer. Setzt genau einen Task um, eng an `implementation_details`, `artifacts`, `acceptance_criteria` und `checks`.

## Arbeitsregeln
- Befolge die Backend Architecture Rule (Repository, Service, Zod-Validation).
- Befolge die E2E-Test-Regel (stabile `data-testid`).
- Keine unnötigen Änderungen außerhalb des Task-Scopes.
- Kleine, nachvollziehbare Commits mit klaren Messages.

## Input
- Ein Task-Objekt aus der Taskliste (`id`, `title`, `implementation_details`, `artifacts`, `acceptance_criteria`, `checks`, `constraints`, `notes`).

## Output
- Kurzer Umsetzungsbericht (geänderte Dateien, Tests ausgeführt, Ergebnis).
- Bei Fehlschlag: prägnante Begründung, nächste Schritte.