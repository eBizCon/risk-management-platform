---
name: plan-user-story-implementation-blueprint
description: Erstellt einen entwicklungsreifen Implementation Blueprint für eine User Story, inklusive konkreter Code-Änderungen (Dateien/Klassen/Methoden), Data Contracts, Testplan und Execution Steps – ohne vollständigen Production Code.
---

# Plan User Story Implementation Blueprint

## Rolle
Du agierst als erfahrener Softwarearchitekt / Senior Engineer, der ein developer-ready Umsetzungs-Blueprint erstellt: konkret, file- und code-nah, aber ohne vollständige Implementierung.

## Ziel
Erstelle eine detaillierte, umsetzungsreife Planung, sodass ein Entwickler ohne weitere Rückfragen weiß:
- welche Dateien/Komponenten angepasst werden
- welche neuen Klassen/Methoden entstehen
- wie Datenflüsse aussehen
- welche Tests zu schreiben/anpassen sind
- wie alles lokal/CI verifiziert wird

## Input
- Ausgearbeitete User Story

## Verbindliche Arbeitsregeln
1. Ausgangspunkt: User Story & ACs
   - Analysiere Ziel, Nutzen und Akzeptanzkriterien
   - Extrahiere funktionale Anforderungen, Edge Cases, Non-Functionals (Performance, Security, Logging)

2. Codebasis-Realität
   - Nenne die betroffenen Layer/Module und deren Verantwortlichkeiten
   - Identifiziere konkrete Touchpoints (z. B. Controller, Service, DB, Messaging, UI)
   - Respektiere bestehende Patterns/Standards (Naming, DI, Error Handling, Teststil)
   - Lese und befolge die Windsurf Rules in `.windsurf/rules/` (Backend-Architektur, Code-Style, Testing, Styling)

3. Detaillierte Planung (code-nah)
   - Plane Änderungen auf Datei-/Klassen-/Methoden-Ebene
   - Erlaube:
     - konkrete Klassen- und Methodennamen
     - Methodensignaturen
     - DTOs/Contracts (Felder, Typen)
     - Pseudocode / Schrittlogik (keine vollständige Implementierung)
   - Vermeide:
     - komplette Produktionscode-Blöcke
     - lange Implementierungen (max. kurze Snippets nur zur Illustration)

4. Daten- & Contract-Planung
   - Definiere Datenmodelle (DB Tabellen/Spalten oder Entities) falls nötig
   - Plane Migrationsschritte (Schema-Änderungen)
   - Plane API-Verträge (Request/Response, Fehlercodes)

5. Fehlerfälle & Observability
   - Definiere Validierungen, Fehlermeldungen, Statuscodes
   - Logging/Tracing-Maßnahmen (wo und was)
   - Sicherheitsaspekte (AuthZ/AuthN, Input Sanitization)

6. Tests sind zwingend
   - Leite Tests direkt aus Akzeptanzkriterien ab
   - Plane Unit/Integration/Contract/E2E je nach Bedarf
   - Tests müssen dem bestehenden Repo-Teststil folgen (Framework, Patterns, Ordner, Naming)
   - Definiere konkrete Testfälle (Given/When/Then) und betroffene Test-Dateien

7. Verifikation (Execution & Fix Loop)
   - Nenne die konkreten Commands zum Ausführen (z. B. unit, integration, e2e)
   - Wenn Tests fehlschlagen: Ursache analysieren, minimal fixen, erneut ausführen bis grün (als Plan)

8. Abgrenzung (neu, praxisnah)
   - Du schreibst keine vollständige Implementierung
   - Du lieferst aber eine vollständige Änderungsplanung inkl. Signaturen, Pseudocode und Dateiliste

## Ausgabeformat (zwingend)
Verwende das Template aus [assets/output-template.md](assets/output-template.md)
