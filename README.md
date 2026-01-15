# Risikomanagement-Plattform

Eine Webanwendung zur Verwaltung und Bewertung von Kreditanträgen mit automatischer Risikobewertung.

## Funktionen

### Für Antragsteller
- Kreditanträge erstellen mit persönlichen und finanziellen Daten
- Entwürfe speichern und später bearbeiten
- Anträge einreichen zur Prüfung
- Automatische Bewertung mit Score (0-100) und Ampelstatus einsehen
- Übersicht aller eigenen Anträge mit Filterung nach Status

### Für Antragsbearbeiter
- Übersicht aller eingereichten Anträge
- Detaillierte Prüfung von Antragsdaten und automatischer Bewertung
- Anträge genehmigen oder ablehnen (mit Pflichtbegründung bei Ablehnung)
- Historie bearbeiteter Anträge nachverfolgen

### Automatisches Scoring
- Bewertung basierend auf Einkommen, Fixkosten, Beschäftigungsstatus und Zahlungshistorie
- Score von 0-100 Punkten mit Ampelstatus (grün >= 75, gelb >= 50, rot < 50)
- Nachvollziehbare Entscheidungsgründe

## Technologie-Stack

- **Frontend**: SvelteKit, TypeScript, Tailwind CSS v4, Lucide Icons
- **Backend**: Drizzle ORM, SQLite
- **Validierung**: Zod

## Installation

```bash
npm install
```

## Entwicklung

```bash
npm run dev
```

## Build

```bash
npm run build
```

## Debug-Modus

Die Anwendung enthält einen Debug-Modus zum Wechseln zwischen den Rollen Antragsteller und Antragsbearbeiter (unten rechts im Bildschirm).
