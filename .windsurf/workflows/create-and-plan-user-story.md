---
description: Create and plan a user story
---

## Zweck

Strukturierter Workflow von einem initialen Input bis zu einer freigegebenen Umsetzungsplanung – mit klaren Feedback-Loops und Freigabe-Gates. Arbeite Schritt für Schritt. Lasse keinen Schritt aus. Starte bei Schritt 1.

## Schritte

1. Rufe skill create-user-story auf
2. Rufe skill refine-user-story auf
3. Approval Gate (User Story):
   - Frage den User: „User Story freigeben"
   - Bei Ja: weiter zu Schritt 4
   - Bei Änderungwünschen: Feedback einarbeiten und zurück zu Schritt 2
4. Rufe skill plan-user-story-implementation-blueprint auf
5. Approval Gate (Implementierungsplan):
   - Frage den User: „Implementierungsplan freigeben"
   - Bei JA: weiter zu Schritt 6
   - Bei Änderungwünschen: Feedback einarbeiten und zurück zu Schritt 4
6. Dokumentation:
   - Frage den User, ob der Plan dokumentiert werden soll
   - Bei JA: Frage nach Dateiname/Pfad (Default: `backlog/implementations/<user-story-name>/<user-story-name>.md`)
   - Erstelle das Dokument
7. Task-Liste:
   - Frage den User, ob eine Task-Liste erstellt werden soll
   - Bei JA: Frage nach Dateiname/Pfad (Default: `backlog/implementations/<user-story-name>/<user-story-name>-tasks.json`)
   - Rufe skill create-tasklist-from-implementation-plan auf und speichere die Liste
