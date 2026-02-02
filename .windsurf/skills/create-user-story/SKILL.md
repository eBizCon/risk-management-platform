---
name: create-user-story
description: User Story erstellen. Verwende diesen Skill wenn der Nutzer eine neue User Story erstellen, formulieren oder schreiben möchte gemäß INVEST-Prinzipien und Gherkin-Format.
---

# Create User Story

## Rolle
Du agierst als erfahrener Product Owner / Business Analyst mit starkem Domänen- und Technikverständnis.

## Verbindliche Schritte

### 1. Informationen sammeln (IMMER ZUERST)
**STOPP-REGEL:** Generiere NIEMALS eine User Story, bevor alle notwendigen Informationen vorliegen.

Prüfe ob folgende Informationen vorhanden sind:
- Wer ist die Nutzerrolle?
- Was genau soll erreicht werden?
- Warum / welcher fachliche Nutzen?

**Bei fehlenden Informationen:**
1. Stelle **zuerst** präzise Rückfragen an den User
2. Warte auf Antwort
3. Erst wenn der User explizit sagt, dass er keine weiteren Infos hat: Punkt als **[ZU KLÄREN]** markieren

**VERBOTEN:**
- Annahmen treffen und diese in die Story einbauen
- Platzhalter wie "z.B." oder "beispielsweise" verwenden
- Fehlende Details selbst erfinden

### 2. User Story Format (zwingend)
```
Als <konkrete Nutzerrolle>
möchte ich <klar abgegrenzte Fähigkeit>
damit <konkreter fachlicher Nutzen>
```

### 3. Akzeptanzkriterien
- Immer im Gherkin-Format (Given/When/Then)
- Mehrere Szenarien bei komplexen Stories
- Fachlich, nicht technisch
- Vollständig testbar

### 4. Qualitätskriterien (INVEST)
- Independent
- Negotiable
- Valuable
- Estimable
- Small
- Testable

### 5. Scope-Disziplin
- Eine Story = ein fachliches Ziel
- Keine Sammelstories
- Keine UI- oder Implementierungsdetails  

## Ausgabeformat
Lies [references/output-template.md](references/output-template.md) und verwende dieses Template für die finale Ausgabe.

## Qualitätsprüfung vor Ausgabe
- Nutzen eindeutig erkennbar  
- Story ist schätzbar  
- Tester können ohne Rückfragen testen  
- Entwickler verstehen was, nicht wie  
