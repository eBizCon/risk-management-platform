Analysiere dieses Projekt und erstelle Windsurf Rules (.windsurf/rules/*.md) basierend auf dem erkannten Tech-Stack und der bestehenden Architektur.



 **Vorgehen:**
 1. Untersuche `package.json`, Config-Dateien und die Ordnerstruktur
 2. Identifiziere: Framework, Sprache, ORM/DB, Styling, Test-Tools
 3. Erkenne bestehende Architektur-Patterns (z.B. Repository-Pattern, Service-Layer)
 4. Leite Best Practices für den erkannten Stack ab

 **Erstelle Rules für:**
 - **Backend-Architektur** (falls vorhanden): Schichten-Trennung, Daten-Zugriff, Validierung
 - **Code-Style**: TypeScript/JS-Konventionen, Testbarkeit (data-testid), Naming
 - **Datenbank** (falls ORM erkannt): Schema-Organisation, Type-Safety
 - **Styling** (falls CSS-Framework erkannt): Utility-First vs. Custom, Design-Tokens
 - **Framework-spezifisch**: Aktuelle Syntax und Patterns des erkannten Frameworks
 - **Testing**: E2E- und Unit-Test-Konventionen passend zu den erkannten Tools

 **Format:**
 - Verwende `trigger: always_on` für grundlegende Regeln
 - Verwende `trigger: model_decision` mit `description` für kontextabhängige Regeln
 - Halte jede Rule fokussiert und prägnant
