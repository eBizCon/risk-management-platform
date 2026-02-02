---
description: Create and plan a user story
---

## Zweck
Strukturierter Workflow von einem initialen Input bis zu einer freigegebenen Umsetzungsplanung – mit klaren Feedback-Loops und Freigabe-Gates. Arbeite Schritt für Schritt. Lasse keinen Schritt aus. Starte bei Schritt 1

## Schritte
1. Rufe skill create-user-story auf
2. Rufe skill refine-user-story
3. Approval gate loop:
   - Frage den User explizit:
     - „User Story freigeben? (JA / Änderungen)“
   - Bei Änderungen:
     - Feedback einarbeiten
     - Zurück zu Schritt 1
   - Bei JA:
     - User Story ist final → weiter zu Phase 2
    Loop-Regel:  
    Diese Phase wird so lange wiederholt, bis die User Story final freigegeben ist.
4. Rufe skill plan-user-story-implementation-blueprint auf
5. Approval gate loop:  
   - Frage den User explizit:
     - „Implementierungsplan freigeben? (JA / Änderungen)“
   - Bei Änderungen:
     - Feedback einarbeiten
     - Zurück zu Schritt 4
   - Bei JA:
     - Planung ist final → OK für Implementierung
    Loop-Regel:  
    Diese Phase wird so lange wiederholt, bis der Implementierungsplan freigegeben ist.
6. Document plan 
  - Ask the user if they want to document the plan
  - If yes, ask the user to provide a file name and location for the document. The default location is backlog/implementation/<user-story-name>/<user-story-name>.md
  - Create the document with the plan
7. Ask the user if they want to create a task list for the plan
  - If yes, ask the user to provide a file name and location for the task list. The default location is backlog/implementation/<user-story-name>/<user-story-name>-tasks.json
  - if no, continue without creating a task list
  - if yes, call skill create-tasklist-from-implementation-plan with the plan and save the task list to the given location
