# Risikomanagement-Plattform

Eine verteilte Webanwendung zur Verwaltung und Bewertung von Kreditanträgen mit automatischer Risikobewertung. Die Plattform basiert auf einer Microservice-Architektur mit einem .NET Backend, einem SvelteKit Frontend (BFF) und asynchroner Kommunikation über RabbitMQ.

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
- [Weiterführende Dokumentation](#weiterführende-dokumentation)
- [Troubleshooting](#troubleshooting)

---

## Quickstart

### Option A: Mit .NET Aspire (empfohlen)

```bash
# 1. Repository klonen
git clone <repo-url> && cd risk-management-platform

# 2. Frontend-Dependencies installieren
cd src/frontend && npm install && cd ../..

# 3. Aspire AppHost starten (startet automatisch alle Backend-Services)
cd src/backend && dotnet run --project AppHost

# 4. Aspire Dashboard öffnen (zeigt alle Service-URLs)
# Das Dashboard wird automatisch im Browser geöffnet

# 5. Frontend starten (in einem separaten Terminal)
cd src/frontend && npm run dev

# 6. Anwendung öffnen: http://localhost:5173
# Test-Benutzer: applicant/applicant oder processor/processor
```

> **Hinweis:** Aspire startet automatisch PostgreSQL, Keycloak, RabbitMQ und beide Backend-APIs.
> Die Service-URLs werden dynamisch zugewiesen und im Aspire Dashboard angezeigt.

### Option B: Mit Docker Compose + manuellen Services

```bash
# 1. Repository klonen
git clone <repo-url> && cd risk-management-platform

# 2. Alle Services mit einem Skript starten
./dev/start-dev.sh

# 3. Anwendung öffnen: http://localhost:5173
# Test-Benutzer: applicant/applicant oder processor/processor
```

> **Hinweis:** `start-dev.sh` startet Docker Services (Keycloak, PostgreSQL, RabbitMQ),
> das .NET Backend (dotnet watch) und das SvelteKit Frontend (vite dev).

---

## Überblick

### Problem & Nutzen

Die Risikomanagement-Plattform ermöglicht die digitale Beantragung und Bearbeitung von Kreditanträgen. Sie automatisiert die Risikobewertung und bietet transparente Entscheidungsgrundlagen für Antragsteller und Bearbeiter.

### Hauptfunktionen

| Feature | Beschreibung |
|---------|--------------|
| **Kundenverwaltung** | Kundenstammdaten anlegen, bearbeiten, archivieren und aktivieren |
| **Antragsstellung** | Kreditanträge mit persönlichen und finanziellen Daten erstellen |
| **Entwürfe** | Anträge als Entwurf speichern und später bearbeiten |
| **Automatisches Scoring** | Bewertung (0-100 Punkte) mit Ampelstatus basierend auf Einkommen, Fixkosten, Beschäftigungsstatus und Zahlungshistorie |
| **Antragsbearbeitung** | Prüfung, Genehmigung oder Ablehnung durch Bearbeiter |
| **Rückfragen** | Bearbeiter können Rückfragen an Antragsteller stellen |
| **Scoring-Konfiguration** | Risikomanager können die Scoring-Parameter anpassen |
| **Dashboard** | Statistiken und Diagramme zu Antragsstatus |
| **Transparenz** | Nachvollziehbare Bewertungsgründe für jede Entscheidung |

### Scoring-Logik

| Score | Ampel | Bedeutung |
|-------|-------|-----------|
| >= 75 | Grün | Positiv - Kreditantrag empfohlen |
| 50-74 | Gelb | Prüfung erforderlich - manuelle Bewertung empfohlen |
| < 50 | Rot | Kritisch - erhöhtes Risiko |

---

## Benutzerrollen & Berechtigungen

Die Anwendung verwendet OIDC (OpenID Connect) mit Keycloak zur Authentifizierung. Es gibt drei Rollen:

### Antragsteller (`applicant`)

| Berechtigung | Beschreibung |
|--------------|--------------|
| Kunden anlegen | Neue Kunden mit Stammdaten erstellen |
| Anträge erstellen | Neue Kreditanträge mit allen erforderlichen Daten anlegen |
| Entwürfe verwalten | Anträge speichern, bearbeiten und löschen (nur im Status `draft`) |
| Anträge einreichen | Entwürfe zur Prüfung einreichen |
| Eigene Anträge einsehen | Übersicht aller eigenen Anträge mit Filterung nach Status |
| Bewertung einsehen | Score, Ampelstatus und Bewertungsgründe der eigenen Anträge |
| Rückfragen beantworten | Offene Rückfragen von Bearbeitern beantworten |

**Zugängliche Routen:** `/applications/*`, `/customers/*`

### Antragsbearbeiter (`processor`)

| Berechtigung | Beschreibung |
|--------------|--------------|
| Alle Anträge einsehen | Übersicht aller eingereichten Anträge |
| Anträge prüfen | Detailansicht mit allen Antragsdaten und automatischer Bewertung |
| Entscheidung treffen | Anträge genehmigen oder ablehnen |
| Begründung | Pflichtkommentar bei Ablehnung |
| Rückfragen stellen | Zusätzliche Informationen vom Antragsteller anfordern |
| CSV-Export | Export aller gefilterten Anträge als CSV |

**Zugängliche Routen:** `/processor/*`, `/customers/*`

### Risikomanager (`risk_manager`)

| Berechtigung | Beschreibung |
|--------------|--------------|
| Scoring-Konfiguration | Scoring-Parameter (Schwellenwerte, Gewichtungen) anpassen |

**Zugängliche Routen:** `/risk-manager/*`

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
| [SvelteKit](https://kit.svelte.dev/) | 2.x | BFF (Backend for Frontend) mit Server-Side Rendering |
| [Svelte 5](https://svelte.dev/) | 5.x | UI-Framework mit Runes |
| [TypeScript](https://www.typescriptlang.org/) | 5.x | Typsicherheit |
| [Tailwind CSS](https://tailwindcss.com/) | 4.x | Styling |
| [Chart.js](https://www.chartjs.org/) | 4.x | Dashboard-Diagramme |
| [Lucide](https://lucide.dev/) | - | Icons |
| [openid-client](https://github.com/panva/openid-client) | 6.x | OIDC-Authentifizierung |

### Backend

| Technologie | Version | Verwendung |
|-------------|---------|------------|
| [.NET](https://dotnet.microsoft.com/) | 10.x | Runtime & SDK |
| [ASP.NET Core](https://docs.microsoft.com/aspnet/core) | 10.x | Web API Framework |
| [Entity Framework Core](https://docs.microsoft.com/ef/core) | 10.x | ORM & Datenbankzugriff |
| [MassTransit](https://masstransit.io/) | - | Message Bus & Saga Orchestration |
| [FluentValidation](https://docs.fluentvalidation.net/) | - | Eingabevalidierung |
| [PostgreSQL](https://www.postgresql.org/) | 17.x | Datenbank |
| [RabbitMQ](https://www.rabbitmq.com/) | 4.x | Message Broker |

### Infrastruktur

| Technologie | Version | Verwendung |
|-------------|---------|------------|
| [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/) | 13.x | Service-Orchestrierung & Dashboard |
| [Keycloak](https://www.keycloak.org/) | 26.x | Identity Provider (OIDC) |
| [Docker Compose](https://docs.docker.com/compose/) | - | Lokale Entwicklungsumgebung |

### Testing

| Technologie | Version | Verwendung |
|-------------|---------|------------|
| [Vitest](https://vitest.dev/) | 4.x | Unit-Tests (Frontend) |
| [Playwright](https://playwright.dev/) | 1.57.x | E2E-Tests |
| [Testing Library](https://testing-library.com/) | - | Component-Tests |
| [xUnit](https://xunit.net/) | - | Unit-Tests (Backend) |

---

## Architektur

Die Anwendung folgt einer **Distributed Architecture** mit Domain-Driven Design (DDD), CQRS und Event-Driven Messaging.

```
+-------------------------------------------------------------------+
|                      Frontend (SvelteKit BFF)                     |
|                                                                   |
|  +----------+  +----------+  +----------+  +---------------+     |
|  |  Pages   |  |Components|  |  Charts  |  | hooks.server  |     |
|  | (Svelte) |  | (Svelte) |  |(Chart.js)|  | (BFF Proxy)   |     |
|  +----------+  +----------+  +----------+  +-------+-------+     |
|                                                     |             |
|           OIDC Auth (openid-client, Session Cookie)  |             |
+-----------------------------------------------------+-------------+
                                                      |
                        HTTP (API Proxy + JWT/API-Key) |
                                                      |
          +-------------------------------------------+------+
          |                                           |      |
          v                                           v      |
+----------------------+              +----------------------+
|  Risk Management API |    HTTP      | Customer Mgmt API    |
|  (ASP.NET Core)      |<----------->| (ASP.NET Core)       |
|                      |   (ACL)     |                      |
|  CQRS Dispatcher     |             |  CQRS Dispatcher     |
|  MassTransit Saga    |             |  Outbox Pattern      |
|  EF Core + PG        |             |  EF Core + PG        |
+----------+-----------+              +----------------------+
           |                                    |
           | AMQP                               | AMQP (Outbox)
           v                                    v
+-------------------------------------------------------------+
|                    RabbitMQ (Message Broker)                 |
|                                                             |
|  Saga: ApplicationCreation (Create + Update)                |
|  Integration Events: Customer CRUD                          |
+-------------------------------------------------------------+
           |
           v
+---------------------+   +-------------------+
|     PostgreSQL      |   |     Keycloak      |
|  - risk-management  |   |  (OIDC Provider)  |
|  - customer-mgmt    |   |  JWT + Rollen     |
+---------------------+   +-------------------+
```

### Architektur-Prinzipien

1. **Domain-Driven Design (DDD)**: Aggregates, Value Objects, Domain Events, Bounded Contexts
2. **CQRS**: Commands und Queries über einen zentralen `IDispatcher` getrennt
3. **Saga Pattern**: Asynchrone Orchestrierung langlebiger Prozesse (Antragserstellung und -aktualisierung) via MassTransit State Machine
4. **Repository Pattern**: Alle Datenbankzugriffe über dedizierte Repository-Interfaces
5. **Anti-Corruption Layer (ACL)**: Service-zu-Service-Kommunikation über definierte Interfaces
6. **Outbox Pattern**: Zuverlässige Event-Publizierung im CustomerManagement BC
7. **BFF Pattern**: SvelteKit Frontend proxied API-Aufrufe an Backend-Services

Weiterführende Architektur-Dokumentation:

- [Application Lifecycle & Message Patterns](./docs/application-lifecycle.md) -- Detaillierter Antrags-Lifecycle, CQRS Commands/Queries, Domain Events, Saga-Ablauf
- [Context Map](./docs/context-map.md) -- Bounded Contexts, Beziehungen, Deployment-Topologie
- [Saga Pattern](./docs/saga-pattern.md) -- Technische Dokumentation der asynchronen Antragserstellung

---

## Repository-Struktur

```
risk-management-platform/
|-- dev/                                    # Entwicklungs-Infrastruktur
|   |-- docker-compose.yml                  # Keycloak + PostgreSQL + RabbitMQ
|   |-- keycloak/                           # Keycloak-Konfiguration
|   |-- init-db.sql                         # Initiale DB-Konfiguration
|   |-- services-up.sh                      # Docker-Infrastruktur starten
|   |-- services-down.sh                    # Docker-Infrastruktur stoppen
|   |-- start-dev.sh                        # Alles starten (Docker + Backend + Frontend)
|   +-- add-migration.sh                    # EF Core Migration Helper
|
|-- src/
|   |-- backend/                            # .NET Solution
|   |   |-- RiskManagementPlatform.slnx     # Solution File
|   |   |-- AppHost/                        # .NET Aspire AppHost
|   |   |-- ServiceDefaults/                # Aspire Service Defaults
|   |   |-- SharedKernel/                   # Gemeinsame Bausteine (CQRS, Results, Events)
|   |   |-- RiskManagement.Domain/          # Kerndomaene (Aggregates, Value Objects)
|   |   |-- RiskManagement.Application/     # Application Layer (Commands, Queries, Sagas)
|   |   |-- RiskManagement.Infrastructure/  # Infrastructure (EF Core, Consumers, Sagas)
|   |   |-- RiskManagement.Api/             # API Layer (Controllers, Middleware)
|   |   |-- RiskManagement.Api.Tests/       # Backend Unit Tests
|   |   |-- CustomerManagement.Domain/      # Kundenverwaltung Domain
|   |   |-- CustomerManagement.Application/ # Kundenverwaltung Application
|   |   |-- CustomerManagement.Infrastructure/ # Kundenverwaltung Infrastructure
|   |   +-- CustomerManagement.Api/         # Kundenverwaltung API
|   |
|   +-- frontend/                           # SvelteKit Frontend (BFF)
|       |-- src/
|       |   |-- lib/                        # Shared Code (Components, API, Types)
|       |   |-- routes/                     # SvelteKit Routes
|       |   +-- hooks.server.ts             # BFF Proxy & Auth Middleware
|       |-- e2e/                            # End-to-End Tests (Playwright)
|       +-- package.json                    # Frontend Dependencies & Scripts
|
|-- docs/                                   # Architektur-Dokumentation
|-- backlog/                                # Projekt-Dokumentation & Blueprints
|-- .windsurf/                              # AI Automation Framework
|-- .github/                                # GitHub Actions CI/CD
+-- AGENTS.md                               # AI Agent Konfiguration
```

---

## Lokale Entwicklung

### Voraussetzungen

- **Node.js** >= 18.x (fuer Frontend)
- **npm** >= 9.x
- **.NET SDK** >= 10.x (fuer Backend)
- **Docker** & **Docker Compose** (fuer Keycloak, PostgreSQL & RabbitMQ)

### Option A: Mit .NET Aspire (empfohlen)

Aspire orchestriert alle Backend-Services automatisch und bietet ein Dashboard zur Überwachung.

```bash
# 1. Frontend-Dependencies installieren
cd src/frontend && npm install && cd ../..

# 2. Aspire AppHost starten
cd src/backend && dotnet run --project AppHost

# 3. Frontend starten (separates Terminal)
cd src/frontend && npm run dev
```

Aspire startet automatisch:
- **PostgreSQL** (mit separaten Datenbanken fuer risk-management und customer-management)
- **Keycloak** (mit Realm-Import)
- **RabbitMQ** (mit Management UI)
- **Risk Management API** (mit DB-Migration und Seeding)
- **Customer Management API** (mit DB-Migration)

Das Aspire Dashboard zeigt alle Services, Logs und Health-Status.

### Option B: Mit Docker Compose

```bash
# 1. Frontend-Dependencies installieren
cd src/frontend && npm install && cd ../..

# 2. Alles starten (Docker + Backend + Frontend)
./dev/start-dev.sh
```

Oder manuell:

```bash
# 1. Docker-Infrastruktur starten
./dev/services-up.sh

# 2. Backend starten (separates Terminal)
cd src/backend/RiskManagement.Api && dotnet watch run --launch-profile http

# 3. Frontend starten (separates Terminal)
cd src/frontend && npm run dev
```

### Ports

| Service | URL | Beschreibung |
|---------|-----|--------------|
| Frontend (Dev) | http://localhost:5173 | SvelteKit Dev Server |
| Frontend (Preview) | http://localhost:4173 | SvelteKit Preview Server |
| Risk Management API | http://localhost:5227 | .NET Backend API |
| Customer Management API | http://localhost:5000 | .NET Backend API |
| Keycloak | http://localhost:8081 | Identity Provider Admin UI |
| PostgreSQL | localhost:5432 | Datenbank |
| RabbitMQ | localhost:5672 | Message Broker |
| RabbitMQ Management | http://localhost:15672 | RabbitMQ Admin UI |

> **Hinweis:** Bei Aspire werden die Backend-Ports dynamisch zugewiesen. Die tatsächlichen URLs sind im Aspire Dashboard sichtbar.

### Entwicklungsservices verwalten

```bash
# Docker-Services starten (Keycloak + PostgreSQL + RabbitMQ)
./dev/services-up.sh

# Docker-Services stoppen
./dev/services-down.sh

# Logs anzeigen
docker compose -f dev/docker-compose.yml logs -f
```

**Keycloak Admin:** http://localhost:8081 mit `admin` / `admin`
**RabbitMQ Management:** http://localhost:15672 mit `risk` / `risk`

---

## Konfiguration & Umgebungsvariablen

### Frontend (`src/frontend/.env`)

Erstelle eine `.env`-Datei basierend auf [`src/frontend/.env.example`](./src/frontend/.env.example):

```bash
# Session-Verschlüsselung
SESSION_SECRET=change-me-to-a-random-32-char-string!!

# OIDC-Konfiguration (Keycloak)
OIDC_ISSUER=http://localhost:8081/realms/risk-management
OIDC_CLIENT_ID=risk-management-platform
OIDC_CLIENT_SECRET=
OIDC_REDIRECT_URI=http://localhost:5173/auth/callback
OIDC_POST_LOGOUT_REDIRECT_URI=http://localhost:5173
OIDC_SCOPE=openid profile email
OIDC_ROLES_CLAIM_PATH=realm_access.roles

# Backend-Service-URLs (BFF Proxy)
RISK_MANAGEMENT_API_URL=http://localhost:5227
CUSTOMER_SERVICE_URL=http://localhost:5000

# Service-zu-Service API Key
SERVICE_API_KEY=dev-api-key-12345
```

### Variablen-Beschreibung

| Variable | Beschreibung | Default |
|----------|--------------|---------|
| `SESSION_SECRET` | Schlüssel fuer Session-Cookie-Verschlüsselung (AES-256-GCM) | - |
| `OIDC_ISSUER` | Keycloak Realm URL | `http://localhost:8081/realms/risk-management` |
| `OIDC_CLIENT_ID` | OIDC Client ID | `risk-management-platform` |
| `OIDC_CLIENT_SECRET` | Client Secret (optional) | - |
| `OIDC_REDIRECT_URI` | Callback URL nach Login | `http://localhost:5173/auth/callback` |
| `OIDC_POST_LOGOUT_REDIRECT_URI` | Redirect nach Logout | `http://localhost:5173` |
| `OIDC_SCOPE` | OIDC Scopes | `openid profile email` |
| `OIDC_ROLES_CLAIM_PATH` | Pfad zu Rollen im Token | `realm_access.roles` |
| `RISK_MANAGEMENT_API_URL` | URL der Risk Management API | `http://localhost:5227` |
| `CUSTOMER_SERVICE_URL` | URL der Customer Management API | `http://localhost:5000` |
| `SERVICE_API_KEY` | API Key fuer Service-zu-Service-Kommunikation | - |

### Backend-Konfiguration

Die Backend-APIs werden über `appsettings.json` und Umgebungsvariablen konfiguriert. Bei Aspire erfolgt die Konfiguration automatisch über das AppHost-Projekt.

| Variable | Beschreibung |
|----------|--------------|
| `ConnectionStrings:DefaultConnection` | PostgreSQL-Verbindungsstring |
| `ConnectionStrings:messaging` | RabbitMQ-Verbindungsstring |
| `CUSTOMER_SERVICE_URL` | URL der Customer Management API (fuer Risk API) |
| `APPLICATION_SERVICE_URL` | URL der Risk Management API (fuer Customer API) |
| `SERVICE_API_KEY` | API Key fuer Service-zu-Service-Authentifizierung |
| `OIDC_ISSUER` | Keycloak Realm URL |
| `OIDC_ROLES_CLAIM_PATH` | Pfad zu Rollen im JWT Token |

### Datenbank

Die Anwendung verwendet PostgreSQL als Datenbank mit zwei separaten Datenbanken:
- `risk-management` -- Kreditanträge, Scoring, Saga State
- `customer-management` -- Kundenstammdaten

Das Datenbankschema wird automatisch beim Start der Backend-APIs über EF Core Migrations angewendet (`dbContext.Database.MigrateAsync()`).

---

## Entwicklungs-Workflow

### Frontend NPM Scripts

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
| `npm run test:e2e:ci` | E2E-Tests im CI-Modus |
| `npm run test:all` | Alle Tests ausführen |
| `npm run lint` | ESLint ausführen |
| `npm run lint:fix` | ESLint mit Auto-Fix |
| `npm run format` | Prettier formatieren |
| `npm run format:check` | Prettier prüfen |

> **Hinweis:** Alle NPM-Befehle müssen im Verzeichnis `src/frontend/` ausgeführt werden.

### Backend .NET CLI

```bash
# Backend starten (mit Hot Reload)
cd src/backend/RiskManagement.Api && dotnet watch run --launch-profile http

# Aspire AppHost starten (alle Services)
cd src/backend && dotnet run --project AppHost

# Neue EF Core Migration erstellen
./dev/add-migration.sh <MigrationName>

# Backend Tests ausführen
cd src/backend && dotnet test
```

### Code-Qualität

```bash
# Frontend: TypeScript & Svelte prüfen
cd src/frontend && npm run check

# Frontend: Linting
cd src/frontend && npm run lint

# Frontend: Unit-Tests
cd src/frontend && npm run test

# Frontend: E2E-Tests (benötigt laufende Services)
cd src/frontend && npm run test:e2e:ci

# Backend: Tests
cd src/backend && dotnet test
```

---

## Testing

### Unit-Tests -- Frontend (Vitest)

Unit-Tests befinden sich in `src/frontend/src/**/*.test.ts` und testen isolierte Funktionen wie Scoring und Validierung.

```bash
cd src/frontend

# Einmalig ausführen
npm run test

# Watch-Modus
npm run test:watch
```

**Konfiguration:** [`src/frontend/vitest.config.ts`](./src/frontend/vitest.config.ts)

### Unit-Tests -- Backend (xUnit)

Backend-Tests befinden sich in `src/backend/RiskManagement.Api.Tests/`.

```bash
cd src/backend && dotnet test
```

### E2E-Tests (Playwright)

E2E-Tests befinden sich in [`src/frontend/e2e/`](./src/frontend/e2e/) und testen komplette User-Flows.

```bash
cd src/frontend

# Tests ausführen
npm run test:e2e

# Mit UI
npm run test:e2e:ui

# CI-Modus
npm run test:e2e:ci
```

**Konfiguration:** [`src/frontend/playwright.config.ts`](./src/frontend/playwright.config.ts)

**Hinweis:** E2E-Tests benötigen laufende Backend-Services (Keycloak, PostgreSQL, RabbitMQ, Backend APIs). Sie verwenden einen Test-Session-Mechanismus über `/api/test/session`, der nur im Test-Modus aktiv ist.

### Test-Struktur

```
src/frontend/e2e/
|-- helpers/
|   +-- auth.ts              # Authentifizierungs-Helper
|-- fixtures.ts              # Playwright Fixtures
|-- applicant.test.ts        # Antragsteller-Tests
+-- processor.test.ts        # Bearbeiter-Tests
```

---

## Deployment

### .NET Aspire (Entwicklung & Staging)

Aspire orchestriert alle Services und deren Abhängigkeiten:

```bash
cd src/backend && dotnet run --project AppHost
```

Das AppHost-Projekt (`src/backend/AppHost/Program.cs`) definiert:
- PostgreSQL mit zwei Datenbanken
- Keycloak mit Realm-Import
- RabbitMQ mit Management UI
- Risk Management API mit DB- und Messaging-Referenzen
- Customer Management API mit DB- und Messaging-Referenzen

### Frontend Build

```bash
cd src/frontend && npm run build
```

Die Anwendung verwendet `@sveltejs/adapter-node` fuer Node.js-basiertes Deployment.

### Produktions-Umgebung

Fuer Produktion müssen folgende Anpassungen vorgenommen werden:

1. **Keycloak**: Produktions-Keycloak mit HTTPS konfigurieren
2. **Umgebungsvariablen**: Produktions-URLs in `.env` / Umgebungsvariablen setzen
3. **Datenbanken**: Zwei separate PostgreSQL-Datenbanken konfigurieren
4. **RabbitMQ**: Produktions-RabbitMQ mit TLS und Authentifizierung
5. **Session Secret**: Sicheres, zufälliges Secret fuer Cookie-Verschlüsselung
6. **API Keys**: Sichere Service-zu-Service API Keys

---

## Weiterführende Dokumentation

| Dokument | Beschreibung |
|----------|--------------|
| [Application Lifecycle](./docs/application-lifecycle.md) | Detaillierter Antrags-Lifecycle, Status-Übergänge, CQRS Commands/Queries, Domain Events, Saga-Ablauf |
| [Context Map](./docs/context-map.md) | Bounded Contexts, Beziehungen zwischen Services, SharedKernel, Deployment-Topologie |
| [Saga Pattern](./docs/saga-pattern.md) | Technische Dokumentation der asynchronen Antragserstellung und -aktualisierung |

---

## Troubleshooting

### Keycloak startet nicht

```bash
# Logs prüfen
docker compose -f dev/docker-compose.yml logs keycloak

# Container neu starten
./dev/services-down.sh
./dev/services-up.sh
```

### Backend-API startet nicht

```bash
# Prüfe ob PostgreSQL und RabbitMQ laufen
docker compose -f dev/docker-compose.yml ps

# Backend-Logs anzeigen
cd src/backend/RiskManagement.Api && dotnet run --launch-profile http

# Aspire-Dashboard fuer alle Logs nutzen
cd src/backend && dotnet run --project AppHost
```

### RabbitMQ-Verbindungsfehler

```bash
# RabbitMQ-Status prüfen
docker compose -f dev/docker-compose.yml logs rabbitmq

# Management UI: http://localhost:15672 (risk/risk)
# Queues und Exchanges prüfen
```

### "Login erforderlich" trotz Anmeldung

- Session ist abgelaufen (prüfe `SESSION_SECRET`)
- Cookies werden nicht gesetzt (prüfe Browser-Einstellungen)
- Keycloak ist nicht erreichbar
- Backend-API gibt 401 zurück (Token abgelaufen)

### "Keine Berechtigung" nach Login

- Benutzer hat keine Rolle zugewiesen
- Falsche Rolle fuer die Route (applicant vs. processor vs. risk_manager)
- Rollen-Claim-Pfad in `.env` ist falsch konfiguriert

### Datenbank-Fehler

Die Datenbank-Migration wird automatisch beim Backend-Start ausgeführt. Falls Probleme auftreten:

```bash
# PostgreSQL-Logs prüfen
docker compose -f dev/docker-compose.yml logs postgres

# Datenbanken manuell prüfen
docker compose -f dev/docker-compose.yml exec postgres psql -U risk -d risk_management -c '\dt'
```

### E2E-Tests schlagen fehl

```bash
# Sicherstellen dass alle Services laufen
./dev/services-up.sh

# Browser installieren
cd src/frontend && npx playwright install chromium

# Mit Debug-Modus
cd src/frontend && npx playwright test --debug
```

### Port bereits belegt

```bash
# Prozess auf Port finden und beenden
lsof -i :5173
lsof -i :5227
lsof -i :5000
kill -9 <PID>

# Oder anderen Port verwenden
cd src/frontend && npm run dev -- --port 3000
```

---

## Lizenz

*TODO: Lizenz hinzufügen*
