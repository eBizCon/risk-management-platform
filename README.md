# Risikomanagement-Plattform

Eine Webanwendung zur Verwaltung und Bewertung von Kreditanträgen mit automatischer Risikobewertung.

---

## Inhaltsverzeichnis

- [Quickstart](#quickstart)
- [Überblick](#überblick)
- [Benutzerrollen & Berechtigungen](#benutzerrollen--berechtigungen)
- [Technologie-Stack](#technologie-stack)
- [Architektur](#architektur)
- [Repository-Struktur](#repository-struktur)
- [Lokale Entwicklung](#lokale-entwicklung)
- [Konfiguration & Umgebungsvariablen](#konfiguration--umgebungsvariablen)
- [Entwicklungs-Workflow](#entwicklungs-workflow)
- [Testing](#testing)
- [Deployment](#deployment)
- [Troubleshooting](#troubleshooting)

---

## Quickstart

```bash
# 1. Repository klonen
git clone <repo-url> && cd risk-management-platform

# 2. Dependencies installieren
npm install

# 3. Umgebungsvariablen konfigurieren
cp .env.example .env

# 4. Keycloak (Identity Provider) starten
./dev/keycloak/keycloak-up.sh

# 5. Warten bis Keycloak bereit ist (ca. 30 Sekunden)
# Keycloak Admin-UI: http://localhost:8081 (admin/admin)

# 6. Entwicklungsserver starten
npm run dev

# 7. Anwendung öffnen: http://localhost:5173
# Test-Benutzer: applicant/applicant oder processor/processor
```

---

## Überblick

### Problem & Nutzen

Die Risikomanagement-Plattform ermöglicht die digitale Beantragung und Bearbeitung von Kreditanträgen. Sie automatisiert die Risikobewertung und bietet transparente Entscheidungsgrundlagen für Antragsteller und Bearbeiter.

### Hauptfunktionen

| Feature | Beschreibung |
|---------|--------------|
| **Antragsstellung** | Kreditanträge mit persönlichen und finanziellen Daten erstellen |
| **Entwürfe** | Anträge als Entwurf speichern und später bearbeiten |
| **Automatisches Scoring** | Bewertung (0-100 Punkte) mit Ampelstatus basierend auf Einkommen, Fixkosten, Beschäftigungsstatus und Zahlungshistorie |
| **Antragsbearbeitung** | Prüfung, Genehmigung oder Ablehnung durch Bearbeiter |
| **Transparenz** | Nachvollziehbare Bewertungsgründe für jede Entscheidung |

### Scoring-Logik

| Score | Ampel | Bedeutung |
|-------|-------|-----------|
| ≥ 75 | 🟢 Grün | Positiv - Kreditantrag empfohlen |
| 50-74 | 🟡 Gelb | Prüfung erforderlich - manuelle Bewertung empfohlen |
| < 50 | 🔴 Rot | Kritisch - erhöhtes Risiko |

---

## Benutzerrollen & Berechtigungen

Die Anwendung verwendet OIDC (OpenID Connect) mit Keycloak zur Authentifizierung. Es gibt zwei Rollen:

### Antragsteller (`applicant`)

| Berechtigung | Beschreibung |
|--------------|--------------|
| Anträge erstellen | Neue Kreditanträge mit allen erforderlichen Daten anlegen |
| Entwürfe verwalten | Anträge speichern, bearbeiten und löschen (nur im Status `draft`) |
| Anträge einreichen | Entwürfe zur Prüfung einreichen |
| Eigene Anträge einsehen | Übersicht aller eigenen Anträge mit Filterung nach Status |
| Bewertung einsehen | Score, Ampelstatus und Bewertungsgründe der eigenen Anträge |

**Zugängliche Routen:** `/applications/*`

### Antragsbearbeiter (`processor`)

| Berechtigung | Beschreibung |
|--------------|--------------|
| Alle Anträge einsehen | Übersicht aller eingereichten Anträge |
| Anträge prüfen | Detailansicht mit allen Antragsdaten und automatischer Bewertung |
| Entscheidung treffen | Anträge genehmigen oder ablehnen |
| Begründung | Pflichtkommentar bei Ablehnung |
| CSV-Export | Export aller gefilterten Anträge als CSV |

**Zugängliche Routen:** `/processor/*`

### Test-Benutzer (Keycloak)

| Benutzer | Passwort | Rolle |
|----------|----------|-------|
| `applicant` | `applicant` | Antragsteller |
| `processor` | `processor` | Antragsbearbeiter |

---

## Technologie-Stack

### Frontend

| Technologie | Version | Verwendung |
|-------------|---------|------------|
| [SvelteKit](https://kit.svelte.dev/) | 2.x | Full-Stack Framework |
| [Svelte 5](https://svelte.dev/) | 5.x | UI-Framework mit Runes |
| [TypeScript](https://www.typescriptlang.org/) | 5.x | Typsicherheit |
| [Tailwind CSS](https://tailwindcss.com/) | 4.x | Styling |
| [Lucide](https://lucide.dev/) | - | Icons |

### Backend

| Technologie | Version | Verwendung |
|-------------|---------|------------|
| [SvelteKit](https://kit.svelte.dev/) | 2.x | Server-Side Rendering, API Routes |
| [Drizzle ORM](https://orm.drizzle.team/) | 0.45.x | Datenbank-Abstraction |
| [better-sqlite3](https://github.com/WiseLibs/better-sqlite3) | 12.x | SQLite-Datenbank |
| [Zod](https://zod.dev/) | 4.x | Schema-Validierung |
| [openid-client](https://github.com/panva/openid-client) | 6.x | OIDC-Authentifizierung |

### Infrastruktur

| Technologie | Version | Verwendung |
|-------------|---------|------------|
| [Keycloak](https://www.keycloak.org/) | 26.x | Identity Provider (OIDC) |
| [Docker Compose](https://docs.docker.com/compose/) | - | Lokale Entwicklungsumgebung |

### Testing

| Technologie | Version | Verwendung |
|-------------|---------|------------|
| [Vitest](https://vitest.dev/) | 4.x | Unit-Tests |
| [Playwright](https://playwright.dev/) | 1.57.x | E2E-Tests |
| [Testing Library](https://testing-library.com/) | - | Component-Tests |

---

## Architektur

Die Anwendung folgt einer **Layered Architecture** mit klarer Trennung zwischen Transport- und Datenschicht.

```
┌─────────────────────────────────────────────────────────────┐
│                        Frontend                              │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │
│  │   Pages     │  │ Components  │  │      Stores         │  │
│  │ (+page.svelte)│ │ (*.svelte)  │  │ (Svelte 5 Runes)    │  │
│  └─────────────┘  └─────────────┘  └─────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    Transport Layer                           │
│  ┌─────────────────────┐  ┌─────────────────────────────┐   │
│  │   Server Load       │  │      API Routes             │   │
│  │ (+page.server.ts)   │  │   (/api/*/+server.ts)       │   │
│  └─────────────────────┘  └─────────────────────────────┘   │
│                              │                               │
│  ┌─────────────────────────────────────────────────────┐    │
│  │              hooks.server.ts                         │    │
│  │         (Auth Middleware, Route Protection)          │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                     Service Layer                            │
│  ┌─────────────────┐  ┌─────────────────┐  ┌────────────┐   │
│  │   Repositories  │  │    Services     │  │    Auth    │   │
│  │ (DB-Zugriff)    │  │ (Business Logic)│  │  (OIDC)    │   │
│  └─────────────────┘  └─────────────────┘  └────────────┘   │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                      Data Layer                              │
│  ┌─────────────────────────────────────────────────────┐    │
│  │              Drizzle ORM + SQLite                    │    │
│  │                   (data.db)                          │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

### Architektur-Prinzipien

1. **Repository Pattern**: Alle Datenbankzugriffe erfolgen über dedizierte Repository-Funktionen in [`src/lib/server/services/repositories/`](./src/lib/server/services/repositories/)
2. **Service Layer**: Business-Logik (z.B. Scoring) ist in Services gekapselt
3. **Zod-Validierung**: Eingabedaten werden in API-Routes und Server-Load-Funktionen validiert
4. **Session-basierte Auth**: In-Memory Session Store mit OIDC-Token-Validierung

---

## Repository-Struktur

```
risk-management-platform/
├── dev/                          # Entwicklungs-Infrastruktur
│   └── keycloak/                 # Keycloak Docker Setup
│       ├── docker-compose.yml    # Keycloak Container-Definition
│       ├── import/               # Realm-Konfiguration (auto-import)
│       ├── keycloak-up.sh        # Start-Skript
│       └── keycloak-down.sh      # Stop-Skript
│
├── e2e/                          # End-to-End Tests (Playwright)
│   ├── helpers/                  # Test-Utilities (Auth-Helper)
│   ├── applicant.test.ts         # Tests für Antragsteller
│   ├── processor.test.ts         # Tests für Bearbeiter
│   └── fixtures.ts               # Playwright Test-Fixtures
│
├── src/
│   ├── lib/                      # Shared Code
│   │   ├── components/           # Svelte UI-Komponenten
│   │   │   ├── ApplicationCard.svelte
│   │   │   ├── ApplicationForm.svelte
│   │   │   ├── ApplicationTable.svelte
│   │   │   ├── ConfirmDialog.svelte
│   │   │   ├── Pagination.svelte
│   │   │   ├── RoleGuard.svelte
│   │   │   ├── ScoreDisplay.svelte
│   │   │   ├── StatusBadge.svelte
│   │   │   └── TrafficLight.svelte
│   │   │
│   │   ├── server/               # Server-only Code
│   │   │   ├── db/               # Datenbank
│   │   │   │   ├── index.ts      # DB-Initialisierung
│   │   │   │   └── schema.ts     # Drizzle Schema
│   │   │   │
│   │   │   └── services/         # Business Logic
│   │   │       ├── auth/         # Authentifizierung
│   │   │       │   ├── env.ts    # OIDC-Konfiguration
│   │   │       │   ├── oidc.ts   # OIDC-Client
│   │   │       │   └── session.ts# Session-Management
│   │   │       │
│   │   │       ├── repositories/ # Datenzugriff
│   │   │       │   └── application.repository.ts
│   │   │       │
│   │   │       ├── scoring.ts    # Scoring-Algorithmus
│   │   │       └── validation.ts # Zod-Schemas
│   │   │
│   │   ├── stores/               # Svelte Stores
│   │   └── types.ts              # Shared TypeScript Types
│   │
│   ├── routes/                   # SvelteKit Routes
│   │   ├── +layout.svelte        # Root Layout (Navigation)
│   │   ├── +layout.server.ts     # User-Daten laden
│   │   ├── +page.svelte          # Homepage
│   │   │
│   │   ├── login/                # Login-Seite
│   │   ├── logout/               # Logout-Handler
│   │   ├── auth/                 # OIDC Callback
│   │   │
│   │   ├── applications/         # Antragsteller-Bereich
│   │   │   ├── +page.svelte      # Antragsübersicht
│   │   │   ├── new/              # Neuer Antrag
│   │   │   └── [id]/             # Antragsdetails
│   │   │
│   │   ├── processor/            # Bearbeiter-Bereich
│   │   │   ├── +page.svelte      # Antragsübersicht
│   │   │   └── [id]/             # Antragsdetails
│   │   │
│   │   └── api/                  # API-Endpunkte
│   │       ├── applications/     # Antrags-API
│   │       └── processor/        # Bearbeiter-API
│   │
│   ├── app.css                   # Globale Styles
│   ├── app.d.ts                  # TypeScript Declarations
│   ├── app.html                  # HTML-Template
│   └── hooks.server.ts           # Server Hooks (Auth Middleware)
│
├── static/                       # Statische Assets
├── backlog/                      # Projekt-Dokumentation
│
├── .env.example                  # Beispiel-Umgebungsvariablen
├── package.json                  # Dependencies & Scripts
├── svelte.config.js              # SvelteKit-Konfiguration
├── vite.config.ts                # Vite-Konfiguration
├── vitest.config.ts              # Vitest-Konfiguration
├── playwright.config.ts          # Playwright-Konfiguration
├── tailwind.config.js            # Tailwind-Konfiguration
└── tsconfig.json                 # TypeScript-Konfiguration
```

---

## Lokale Entwicklung

### Voraussetzungen

- **Node.js** >= 18.x
- **npm** >= 9.x
- **Docker** & **Docker Compose** (für Keycloak)

### Setup

```bash
# 1. Dependencies installieren
npm install

# 2. Umgebungsvariablen konfigurieren
cp .env.example .env

# 3. Keycloak starten
./dev/keycloak/keycloak-up.sh

# 4. Entwicklungsserver starten
npm run dev
```

### Ports

| Service | URL | Beschreibung |
|---------|-----|--------------|
| App (Dev) | http://localhost:5173 | SvelteKit Dev Server |
| App (Preview) | http://localhost:4173 | SvelteKit Preview Server |
| Keycloak | http://localhost:8081 | Identity Provider Admin UI |

### Keycloak verwalten

```bash
# Starten
./dev/keycloak/keycloak-up.sh

# Stoppen
./dev/keycloak/keycloak-down.sh

# Logs anzeigen
docker compose -f dev/keycloak/docker-compose.yml logs -f
```

**Admin-Zugang:** http://localhost:8081 mit `admin` / `admin`

---

## Konfiguration & Umgebungsvariablen

### Erforderliche Variablen

Erstelle eine `.env`-Datei basierend auf [`.env.example`](./.env.example):

```bash
# OIDC-Konfiguration (Keycloak)
OIDC_ISSUER=http://localhost:8081/realms/risk-management
OIDC_CLIENT_ID=risk-management-platform
OIDC_REDIRECT_URI=http://localhost:5173/auth/callback
OIDC_POST_LOGOUT_REDIRECT_URI=http://localhost:5173/
OIDC_SCOPE=openid profile
OIDC_ROLES_CLAIM_PATH=realm_access.roles

# Optional: Keycloak Admin-Credentials (für Docker)
KEYCLOAK_ADMIN_USERNAME=admin
KEYCLOAK_ADMIN_PASSWORD=admin
```

### Variablen-Beschreibung

| Variable | Beschreibung | Default |
|----------|--------------|---------|
| `OIDC_ISSUER` | Keycloak Realm URL | `http://localhost:8081/realms/risk-management` |
| `OIDC_CLIENT_ID` | OIDC Client ID | `risk-management-platform` |
| `OIDC_REDIRECT_URI` | Callback URL nach Login | `http://localhost:5173/auth/callback` |
| `OIDC_POST_LOGOUT_REDIRECT_URI` | Redirect nach Logout | `http://localhost:5173/` |
| `OIDC_SCOPE` | OIDC Scopes | `openid profile` |
| `OIDC_ROLES_CLAIM_PATH` | Pfad zu Rollen im Token | `realm_access.roles` |
| `OIDC_CLIENT_SECRET` | Client Secret (optional) | - |

### Datenbank

Die Anwendung verwendet SQLite. Die Datenbankdatei `data.db` wird automatisch im Projektverzeichnis erstellt. Das Schema wird beim ersten Start automatisch angelegt.

---

## Entwicklungs-Workflow

### NPM Scripts

| Script | Beschreibung |
|--------|--------------|
| `npm run dev` | Entwicklungsserver starten |
| `npm run build` | Produktions-Build erstellen |
| `npm run preview` | Build lokal testen |
| `npm run check` | TypeScript & Svelte prüfen |
| `npm run check:watch` | TypeScript & Svelte prüfen (Watch-Modus) |
| `npm run test` | Unit-Tests ausführen |
| `npm run test:watch` | Unit-Tests im Watch-Modus |
| `npm run test:e2e` | E2E-Tests ausführen |
| `npm run test:e2e:ui` | E2E-Tests mit UI |
| `npm run test:all` | Alle Tests ausführen |

### Code-Qualität

```bash
# TypeScript & Svelte prüfen
npm run check

# Unit-Tests
npm run test

# E2E-Tests (benötigt laufenden Build)
npm run test:e2e
```

### Datenbank

Die SQLite-Datenbank wird automatisch initialisiert. Zum Zurücksetzen:

```bash
# Datenbank löschen (wird beim nächsten Start neu erstellt)
rm data.db
```

---

## Testing

### Unit-Tests (Vitest)

Unit-Tests befinden sich in `src/**/*.test.ts` und testen isolierte Funktionen wie Scoring und Validierung.

```bash
# Einmalig ausführen
npm run test

# Watch-Modus
npm run test:watch
```

**Konfiguration:** [`vitest.config.ts`](./vitest.config.ts)

### E2E-Tests (Playwright)

E2E-Tests befinden sich in [`e2e/`](./e2e/) und testen komplette User-Flows.

```bash
# Tests ausführen
npm run test:e2e

# Mit UI
npm run test:e2e:ui

# CI-Modus (stricter)
npm run test:e2e:ci
```

**Konfiguration:** [`playwright.config.ts`](./playwright.config.ts)

**Hinweis:** E2E-Tests verwenden einen Test-Session-Mechanismus über `/api/test/session`, der nur im Test-Modus aktiv ist.

### Test-Struktur

```
e2e/
├── helpers/
│   └── auth.ts           # Authentifizierungs-Helper
├── fixtures.ts           # Playwright Fixtures
├── applicant.test.ts     # Antragsteller-Tests
└── processor.test.ts     # Bearbeiter-Tests
```

---

## Deployment

### Lokaler Build

```bash
npm run build
```

Der Build wird im `build/`-Verzeichnis erstellt (SvelteKit `adapter-node`).

### Docker

Die Anwendung wird als Docker-Container bereitgestellt:

```bash
# Docker-Image bauen
docker build -t risk-management-platform .

# Container starten
docker run -p 3000:3000 \
  -v risk-data:/data \
  -e OIDC_ISSUER=https://<keycloak-host>/realms/risk-management \
  -e OIDC_CLIENT_ID=risk-management-platform \
  -e OIDC_REDIRECT_URI=https://<app-host>/auth/callback \
  -e OIDC_POST_LOGOUT_REDIRECT_URI=https://<app-host>/ \
  -e OIDC_SCOPE="openid profile email" \
  -e OIDC_ROLES_CLAIM_PATH=realm_access.roles \
  -e ORIGIN=https://<app-host> \
  risk-management-platform
```

### Azure Deployment

Die Anwendung ist für das Deployment auf **Azure Container Apps** vorbereitet.

#### Voraussetzungen

- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) (`az`)
- [Azure Developer CLI](https://aka.ms/azd/install) (`azd`) (optional)
- Docker
- Ein Azure-Abonnement

#### Architektur auf Azure

```
┌──────────────────────────────────────────────────────────┐
│                Azure Container Apps Environment           │
│                                                          │
│  ┌─────────────────────┐    ┌─────────────────────────┐  │
│  │   Risk Management   │    │      Keycloak           │  │
│  │   Platform (App)    │───▶│   (Identity Provider)   │  │
│  │   Port: 3000        │    │   Port: 8080            │  │
│  └────────┬────────────┘    └────────┬────────────────┘  │
│           │                          │                    │
│  ┌────────▼────────────┐    ┌────────▼────────────────┐  │
│  │  Azure Files        │    │  Azure Files            │  │
│  │  (SQLite DB)        │    │  (Keycloak Data)        │  │
│  └─────────────────────┘    └─────────────────────────┘  │
└──────────────────────────────────────────────────────────┘
         │
┌────────▼────────────────┐
│  Azure Container        │
│  Registry (ACR)         │
└─────────────────────────┘
```

#### Infrastruktur bereitstellen (Bicep)

```bash
# 1. Bei Azure anmelden
az login

# 2. Resource Group erstellen
az group create --name rg-risk-management --location westeurope

# 3. Infrastruktur mit Bicep bereitstellen
az deployment group create \
  --resource-group rg-risk-management \
  --template-file infra/main.bicep \
  --parameters keycloakAdminPassword='<sicheres-passwort>'

# 4. Deployment-Ausgaben speichern
az deployment group show \
  --resource-group rg-risk-management \
  --name main \
  --query properties.outputs
```

#### Docker-Image bauen und pushen

```bash
# ACR Login-Server aus Outputs lesen
ACR_NAME=$(az deployment group show \
  --resource-group rg-risk-management \
  --name main \
  --query properties.outputs.containerRegistryName.value -o tsv)

# Bei ACR anmelden
az acr login --name $ACR_NAME

# Image bauen und pushen
docker build -t $ACR_NAME.azurecr.io/risk-management-platform:latest .
docker push $ACR_NAME.azurecr.io/risk-management-platform:latest
```

#### Container App aktualisieren

```bash
APP_NAME=$(az deployment group show \
  --resource-group rg-risk-management \
  --name main \
  --query properties.outputs.appName.value -o tsv)

az containerapp update \
  --name $APP_NAME \
  --resource-group rg-risk-management \
  --image $ACR_NAME.azurecr.io/risk-management-platform:latest
```

#### CI/CD mit GitHub Actions

Eine GitHub Actions Pipeline ist unter `.github/workflows/azure-deploy.yml` konfiguriert.

**Erforderliche GitHub Secrets & Variables:**

| Typ | Name | Beschreibung |
|-----|------|--------------|
| Secret | `AZURE_CLIENT_ID` | Service Principal Client ID |
| Secret | `AZURE_TENANT_ID` | Azure AD Tenant ID |
| Secret | `AZURE_SUBSCRIPTION_ID` | Azure Subscription ID |
| Variable | `AZURE_RESOURCE_GROUP` | Name der Resource Group |
| Variable | `AZURE_CONTAINER_REGISTRY` | Name der Container Registry |
| Variable | `AZURE_CONTAINER_APP_NAME` | Name der Container App |

#### Keycloak auf Azure konfigurieren

Nach dem Deployment muss Keycloak konfiguriert werden:

1. Keycloak Admin-UI unter der bereitgestellten URL aufrufen
2. Realm `risk-management` importieren (aus `dev/keycloak/import/risk-management-realm.json`)
3. Client `risk-management-platform` anpassen:
   - Valid Redirect URIs: `https://<app-fqdn>/auth/callback`
   - Web Origins: `https://<app-fqdn>`
   - Post Logout Redirect URIs: `https://<app-fqdn>/`

### Produktions-Hinweise

1. **Keycloak**: Produktions-Keycloak mit HTTPS konfigurieren
2. **Umgebungsvariablen**: Produktions-URLs in den Container App Einstellungen setzen
3. **Datenbank**: SQLite wird auf Azure Files persistiert; fuer hoehere Skalierung PostgreSQL/MySQL in Betracht ziehen
4. **Session Store**: In-Memory Store durch Redis/DB-backed Store ersetzen (bei mehreren Replikas)

---

## Troubleshooting

### Keycloak startet nicht

```bash
# Logs prüfen
docker compose -f dev/keycloak/docker-compose.yml logs

# Container neu starten
./dev/keycloak/keycloak-down.sh
./dev/keycloak/keycloak-up.sh
```

### "Login erforderlich" trotz Anmeldung

- Session ist abgelaufen (1 Stunde Gültigkeit)
- Cookies werden nicht gesetzt (prüfe Browser-Einstellungen)
- Keycloak ist nicht erreichbar

### "Keine Berechtigung" nach Login

- Benutzer hat keine Rolle zugewiesen
- Falsche Rolle für die Route (applicant vs. processor)
- Rollen-Claim-Pfad in `.env` ist falsch konfiguriert

### Datenbank-Fehler

```bash
# Datenbank zurücksetzen
rm data.db
npm run dev
```

### E2E-Tests schlagen fehl

```bash
# Browser installieren
npx playwright install chromium

# Mit Debug-Modus
npx playwright test --debug
```

### Port bereits belegt

```bash
# Prozess auf Port finden und beenden
lsof -i :5173
kill -9 <PID>

# Oder anderen Port verwenden
npm run dev -- --port 3000
```

---

## Lizenz

*TODO: Lizenz hinzufügen*
