---
description: Create and plan a user story
---

## Zweck

Strukturierter Workflow von ein User Story bis zu einer freigegebenen Umsetzungsplanung – mit klaren Feedback-Loops und Freigabe-Gates. Arbeite Schritt für Schritt. Lasse keinen Schritt aus. Starte bei Schritt 1

## Input

- Ausgearbeitete User Story

## Schritte

1. Rufe skill plan-user-story-implementation-blueprint auf
2. Approval Gate (Implementierungsplan):
   - Frage den User: „Implementierungsplan freigeben"
   - Bei JA: weiter zu Schritt 6
   - Bei Änderungswünschen: Feedback einarbeiten und zurück zu Schritt 4
3. Dokumentation:
   - Frage den User, ob der Plan dokumentiert werden soll
   - Bei JA: Frage nach Dateiname/Pfad (Default: `backlog/implementations/<user-story-name>/<user-story-name>.md`)
   - Erstelle das Dokument
4. Task-Liste:
   - Frage den User, ob eine Task-Liste erstellt werden soll
   - Bei JA: Frage nach Dateiname/Pfad (Default: `backlog/implementations/<user-story-name>/<user-story-name>-tasks.json`)
   - Rufe skill create-tasklist-from-implementation-plan auf und speichere die Liste
