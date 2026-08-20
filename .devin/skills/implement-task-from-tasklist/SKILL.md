---
name: implement-task-from-tasklist
description: Implementiert einen Task aus einer Task-Liste (erstellt von create-tasklist-from-implementation-plan) vollständig, inklusive Code-Änderungen, Tests und Verifikation.
---

# Implement Task from Tasklist

## Rolle
Senior Engineer. Setzt genau einen Task um, eng an `implementation_details`, `artifacts`, `acceptance_criteria` und `checks`.

## Arbeitsregeln
- Befolge die Windsurf Rules in `.windsurf/rules/` (Backend-Architektur, Code-Style, Testing, Styling)
- Keine unnötigen Änderungen außerhalb des Task-Scopes.

## Input 
Du erhältst ein strukturiertes JSON-Objekt mit:
- `task`  
  Das vollständige Task-Objekt, das umgesetzt werden soll  
  (`id`, `title`, `implementation_details`, `artifacts`, `acceptance_criteria`, `checks`, `constraints`, `notes`).
- `tasklist`  
  Die komplette Taskliste (Array aller Task-Objekte), wie sie aus `create-tasklist-from-implementation-plan` stammt.  
  Dient als Kontext für Reihenfolge, Abhängigkeiten und Scope-Abgrenzung.
 

## Workflow
1. **Task analysieren**: Lies `implementation_details`, `artifacts`, `acceptance_criteria` und `constraints`
2. **Implementieren**: Führe Code-Änderungen gemäß `artifacts` und `implementation_details` durch
3. **Tests ausführen**: Führe die in `checks` definierten Commands aus
4. **Verifizieren**: Prüfe ob alle `acceptance_criteria` erfüllt sind
5. **Bericht erstellen**: Dokumentiere geänderte Dateien und Testergebnisse

## Output
- Kurzer Umsetzungsbericht (geänderte Dateien, Tests ausgeführt, Ergebnis).
- Bei Fehlschlag: prägnante Begründung, nächste Schritte.