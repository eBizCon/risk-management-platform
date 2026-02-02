 # Use Cases für eine Windsurf-Schulung (4h)
 
 Diese Liste sammelt mögliche Use Cases für dieses Demo-Projekt (gemischte Gruppe). Jeder Use Case ist bewusst so formuliert, dass sofort klar ist, was implementiert werden soll (keine ausführliche User Story).
 ---
 
 ## A) Use Cases auf Basis vorhandener Funktionen (Enhancements)
 
 ### 1) Applicant: Anträge nach Datum sortieren
 - **Ziel**: In `/applications` eine Sortierung ergänzen: `Neueste zuerst` / `Älteste zuerst`.
 - **Umsetzungsidee**: Sortierung als Query-Param (z.B. `?sort=createdAt_desc`) und im Repository `orderBy` erweitern.
 - **Ergebnis**: Sortierung bleibt beim Reload erhalten, Liste ist stabil sortiert.
 
 ### 2) Applicant: Draft-Löschen mit echtem ConfirmDialog (statt Browser-`confirm()`)
 - **Ziel**: In `/applications` beim Löschen eines Drafts den bestehenden `ConfirmDialog.svelte` verwenden.
 - **Umsetzungsidee**: UI-Refactor: `confirm()` entfernen, Dialog öffnen, bei Confirm den DELETE Call ausführen.
 - **Ergebnis**: Konsistentes Look & Feel, stabilere E2E-Interaktion (über `data-testid`).
 
 ### 3) Processor: Filterauswahl bereinigen (z.B. Draft nicht anzeigen)
 - **Ziel**: In `/processor` die Filteroptionen so anpassen, dass nur fachlich sinnvolle Status auswählbar sind.
 - **Umsetzungsidee**: UI-Dropdown anpassen und optional Status-Whitelist serverseitig erzwingen.
 - **Ergebnis**: Klarere UX, weniger “unerwartete” Filterzustände.
 
 ### 4) Applicant: Pagination für die eigene Antragsliste
 - **Ziel**: `/applications` paginieren (analog zu `/processor`) inkl. `page` Query-Param.
 - **Umsetzungsidee**: Repository-Funktion `getApplicationsByUserPaginated`, Load liefert `pagination` Meta, UI rendert `Pagination.svelte`.
 - **Ergebnis**: Liste bleibt performant und navigierbar bei vielen Einträgen.
 
 ### 5) Processor: Zusätzlich nach Score/Ampel filtern
 - **Ziel**: In `/processor` neben Status auch nach Ampel (`green/yellow/red`) oder Score-Spanne filtern.
 - **Umsetzungsidee**: Query-Param + Repo-Where-Clause erweitern, Filter kombinierbar machen.
 - **Ergebnis**: Kritische Fälle sind schneller auffindbar.

 ### 6) Processor: Bei Reject einen festen Ablehnungsgrund auswählen
 - **Ziel**: Reject bekommt ein Dropdown mit “Reason Codes” (plus optionaler Freitext-Kommentar).
 - **Umsetzungsidee**: `processorDecisionSchema` erweitern, UI im Processor-Detail anpassen, Speicherung je nach Zeit in DB-Feld oder als strukturierte Daten.
 - **Ergebnis**: Ablehnungen werden auswertbar und konsistenter.
 
 ### 7) Processor: CSV-Export der aktuellen Liste [x]
 - **Ziel**: Export-Button in `/processor`, der die aktuelle Filter-/Sortieransicht als CSV herunterlädt.
 - **Umsetzungsidee**: neue API Route (Download) + Query-Validierung + CSV Rendering serverseitig.
 - **Ergebnis**: CSV Download funktioniert, Daten sind reproduzierbar.

 #### Beispiel Prompt:
 Als Processsor möchte ich die aktuelle gefilterte und sortierte Ansicht in der tabelle unter "/processor" als csv datei herunterladen können. Für den Export sollen die Filter und Sortierungen berücksichtigt werden. 
 
 ### 8) Processor: Bulk Actions (mehrere Anträge genehmigen/ablehnen)
 - **Ziel**: In der Processor-Liste mehrere Anträge auswählen und gesammelt “Approve/Reject” durchführen.
 - **Umsetzungsidee**: Checkboxen + Bulk-Endpoint + Validierung + Statuswechsel serverseitig.
 - **Ergebnis**: Schnellere Bearbeitung bei hoher Last.
 
 ### 9) Scoring: Bewertungsgründe verständlicher darstellen
 - **Ziel**: Score-/Ampelgründe (vorhanden als `scoringReasons`) im UI besser visualisieren.
 - **Umsetzungsidee**: `ScoreDisplay`/Detail-Ansicht erweitern (z.B. Kategorien, Badges).
 - **Ergebnis**: Entscheidungen werden nachvollziehbarer.
 
 ### 10) Validation UX: Fehlermeldungen konsistent und nutzerfreundlich
 - **Ziel**: Validierungsfehler pro Feld + optional Summary verbessern (insbesondere Business Rules).
 - **Umsetzungsidee**: Frontend-Rendering der Errors konsolidieren, Server-seitig bleibt Zod der Source of Truth.
 - **Ergebnis**: Weniger Friktion beim Ausfüllen.
 
 ### 11) Auth UX: 401/403 Fehlerseiten verbessern
 - **Ziel**: Bei fehlendem Login/fehlender Rolle bessere Hinweise und klare Call-to-Action (Login-Link, ggf. Return-To).
 - **Umsetzungsidee**: Fehlerdarstellung vereinheitlichen (z.B. `+error.svelte`) und optional Return-To über Cookie/Query.
 - **Ergebnis**: Weniger Verwirrung, saubere Trennung zwischen API (401/403 JSON) und Pages.
 
 ### 12) E2E: Tests stabilisieren und beschleunigen
 - **Ziel**: Stabilere Selectors (`data-testid`), besseres Test-Setup (z.B. schnelleres Seed/Builder Pattern).
 - **Umsetzungsidee**: Helper für “create application” (API statt UI) + konsequente TestIDs an Primäraktionen.
 - **Ergebnis**: Weniger flaky Tests, schnellere Feedback-Loops.
 
 ### 13) Observability: Strukturierte Server-Logs für wichtige Events
 - **Ziel**: Logging für `create/submit/process` + Auth-Events (Login/Logout) mit Korrelations-ID.
 - **Umsetzungsidee**: zentraler Logger/Helper, konsistente Log-Fields.
 - **Ergebnis**: Debugging in Demo/Schulung einfacher.

 ---
 
 ## B) Use Cases für neue / aktuell nicht vorhandene Funktionen
 
 ### 14) Processor: Dashboard mit Kennzahlen (Kacheln + “Letzte 7 Tage”)
 - **Ziel**: Neue Seite `/dashboard` oder `/analytics` mit:
   - Total, submitted/approved/rejected/draft
   - Durchschnittlicher Score
   - Ampel-Verteilung
   - Neue Anträge pro Tag (letzte 7 Tage; Tabelle reicht als MVP)
 - **Umsetzungsidee**: Aggregationsqueries im Repository + UI-Kacheln.
 - **Ergebnis**: Management-Übersicht für schnelle Lageeinschätzung.
 
 ### 15) Applicant: Persönliches Dashboard (“Meine Drafts / Offen / Letzte Updates”)
 - **Ziel**: Übersicht für den Applicant mit Quick-Links auf relevante Anträge.
 - **Umsetzungsidee**: Repo-Funktionen mit `limit`/`sort`, UI mit kleinen Listen.
 - **Ergebnis**: Schnellzugriff auf den eigenen Arbeitsstand.
 
 ### 16) Processor: Arbeitsliste (Queue) nur für submitted, sortiert nach Risiko
 - **Ziel**: Neue Ansicht `/processor/queue` mit Fokus auf “was ist als Nächstes zu tun?”.
 - **Umsetzungsidee**: Filter fix auf `submitted`, Sortierung “risk first” (Score asc oder rot zuerst).
 - **Ergebnis**: Effizientere Abarbeitung.
 
 ### 17) Processor: Antrag “mir zuweisen” (Assignment)
 - **Ziel**: Anträge können einem Processor zugeordnet werden (`assignedTo`, `assignedAt`).
 - **Umsetzungsidee**: DB-Feld(er) + Button im Detail + optionaler Filter “nur meine”.
 - **Ergebnis**: Klarheit, wer woran arbeitet.
 
 ### 18) Applicant: In-App Benachrichtigung nach Entscheidung
 - **Ziel**: Notification Center (UI) + Benachrichtigung bei approve/reject inkl. Kommentar.
 - **Umsetzungsidee**: neue Entity `notifications`, Erzeugung beim Prozessieren, UI Liste/Badge.
 - **Ergebnis**: Applicant sieht Updates ohne manuelles Nachschauen.
 
 ### 19) Processor: Hinweis “Neu seit letztem Login”
 - **Ziel**: Badge/Counter “neu” für eingereichte Anträge seit einem Zeitpunkt.
 - **Umsetzungsidee**: Tracking (DB oder Session) + Repo Count Query.
 - **Ergebnis**: Schneller Überblick über neue Arbeit.
 
 ### 20) Applicant: Dokumente zum Antrag hochladen
 - **Ziel**: Upload (z.B. Gehaltsnachweis) beim Antrag, Anzeige/Download im Processor-Detail.
 - **Umsetzungsidee**: Storage-Konzept (Demo: lokal/FS) + neue API Route + UI.
 - **Ergebnis**: Realistischere Antragsstrecke.
 
 ### 21) Scoring: Gründe standardisieren (Reason Keys statt Freitext)
 - **Ziel**: Scoring-Reasons als strukturierte Keys/Kategorien speichern und darstellen.
 - **Umsetzungsidee**: Service `scoring.ts` erweitert Rückgabestruktur, UI rendert strukturiert.
 - **Ergebnis**: Bessere Auswertbarkeit und konsistentere UI.
 
 ### 22) Admin: Scoring-Grenzwerte konfigurierbar machen
 - **Ziel**: Admin-UI für Score-Thresholds ohne Code-Change.
 - **Umsetzungsidee**: neue Rolle/Authorization, Config in DB, Scoring-Service liest Config.
 - **Ergebnis**: “Business Configuration” als Demo.
 
 ### 23) Processor: Suche nach Name oder ID in der Liste
 - **Ziel**: Suchfeld in `/processor`, Query-Param `q=...`, Filterung im Repo.
 - **Umsetzungsidee**: `LIKE` auf Name + direkter ID-Match, Debounce optional.
 - **Ergebnis**: Schneller Zugriff auf einzelne Fälle.

 ### 24) Nutzer: Anwendung auf mobilen Geräten nutzbar machen (Responsive UI)
 - **Ziel**: Die Anwendung soll auf mobilen Geräten (min. 360px Breite) gut bedienbar sein.
 - **Umsetzungsidee**:
   - Navigation/Layout für Mobile optimieren (z.B. Menü einklappen, Abstände anpassen).
   - Tabellen (ApplicationTable) mobilfreundlich darstellen (z.B. Card-Layout oder horizontales Scrollen).
   - Formulare responsiv prüfen (Inputs/Buttons full width, sinnvolle Reihenfolge).
 - **Ergebnis**: Keine abgeschnittenen Inhalte, Buttons gut klickbar, keine horizontale Scroll-Hölle (außer wo bewusst).
