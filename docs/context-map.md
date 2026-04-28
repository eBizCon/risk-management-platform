# Context Map — Risk Management Platform

## Überblick

Die Risk Management Platform besteht aus mehreren Bounded Contexts, die nach Domain-Driven Design (DDD) modelliert sind. Jeder Bounded Context hat eine eigene Domain, Application, Infrastructure und API-Schicht. Die Kommunikation zwischen den Contexts erfolgt über HTTP (synchron) und RabbitMQ (asynchron).

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                              Frontend (SvelteKit)                               │
│                                                                                 │
│   Applicant Views    Processor Views    Risk-Manager Views    Customer Views    │
│   /applications      /processor         /risk-manager         /customers        │
│                                                                                 │
└────────┬──────────────────┬──────────────────────────────────────┬──────────────┘
         │ HTTP             │ HTTP                                 │ HTTP
         │ (BFF Proxy)      │ (BFF Proxy)                          │ (BFF Proxy)
         ▼                  ▼                                      ▼
┌─────────────────────────────────┐              ┌──────────────────────────────┐
│                                 │    HTTP       │                              │
│   Risk Management               │◄────────────►│   Customer Management        │
│   Bounded Context               │   (ACL)      │   Bounded Context            │
│                                 │              │                              │
│   API: risk-api                 │              │   API: customer-api          │
│   DB:  risk-management (PG)     │              │   DB:  customer-management   │
│                                 │              │       (PG)                   │
└────────────┬────────────────────┘              └──────────────────────────────┘
             │
             │ AMQP (RabbitMQ)
             ▼
┌─────────────────────────────────┐
│                                 │
│   Messaging Infrastructure      │
│   (MassTransit + RabbitMQ)      │
│                                 │
│   Saga: ApplicationCreation     │
│   Consumers: 8 (Saga)           │
│   + 5 (Cross-BC sync)           │
│                                 │
└─────────────────────────────────┘


                    ┌───────────────────────────┐
                    │                           │
                    │   Shared Kernel            │
                    │                           │
                    │   Value Objects            │
                    │   Base Classes             │
                    │   Dispatcher (CQRS)        │
                    │   Middleware               │
                    │   Result Pattern           │
                    │                           │
                    └───────────────────────────┘

                    ┌───────────────────────────┐
                    │                           │
                    │   External Services        │
                    │                           │
                    │   SCHUFA (Mock)            │
                    │   Keycloak (OIDC)          │
                    │                           │
                    └───────────────────────────┘
```

## Bounded Contexts

### 1. Risk Management (Kerndomäne)

Der zentrale Bounded Context der Plattform. Verantwortet die Kreditantrags-Verwaltung, Risikobewertung und Scoring.

**Aggregates:**

| Aggregate | Root Entity | Beschreibung |
|---|---|---|
| `ApplicationAggregate` | `Application` | Kreditantrag mit Status-Lifecycle, Scoring, Rückfragen |
| `ScoringConfigAggregate` | `ScoringConfig` | Konfigurierbare Scoring-Parameter und Versionen |

**Domain Services:**

| Service | Beschreibung |
|---|---|
| `IScoringService` | Berechnet Risiko-Score basierend auf Finanzdaten und Konfiguration |
| `ICreditCheckService` | Bonitätsprüfung (implementiert als `MockSchufaProvider`) |

**Application Services:**

| Service | Beschreibung |
|---|---|
| `ICustomerProfileService` | Anti-Corruption Layer zum CustomerManagement BC |

**Domain Events:**

| Event | Auslöser |
|---|---|
| `ApplicationSubmittedEvent` | Antrag eingereicht |
| `ApplicationDecidedEvent` | Antrag genehmigt oder abgelehnt |
| `ApplicationDeletedEvent` | Antrag gelöscht |
| `InquiryCreatedEvent` | Rückfrage erstellt |

**Saga Events (Messaging):**

| Event | Beschreibung |
|---|---|
| `ApplicationCreationStarted` | Startet Saga für neue Antragserstellung (OperationType=Create) |
| `ApplicationUpdateStarted` | Startet Saga für Update/Submit bestehender Anträge (OperationType=Update) |
| `FetchCustomerProfile` | Fordert Kundenprofil vom CustomerManagement BC an |
| `CustomerProfileFetched` | Kundendaten abgerufen |
| `PerformCreditCheck` | Fordert Bonitätsprüfung an |
| `CreditCheckCompleted` | Bonitätsprüfung abgeschlossen |
| `FinalizeApplication` | Finalisiert neuen Antrag (Create-Pfad) |
| `FinalizeApplicationUpdate` | Aktualisiert bestehenden Antrag (Update-Pfad) |
| `MarkApplicationFailed` | Markiert Antrag als fehlgeschlagen in der Domain |
| `ApplicationCreationCompleted` | Antrag erfolgreich finalisiert |
| `ApplicationCreationFailed` | Fehler bei der Antragserstellung |

**Projektstruktur:**

```
RiskManagement.Domain/           Domain-Schicht (Aggregates, Events, Value Objects, Services)
RiskManagement.Application/      Application-Schicht (Commands, Queries, DTOs, Validation, Saga State)
RiskManagement.Infrastructure/   Infrastructure-Schicht (EF Core, HTTP Clients, Consumers, State Machine)
RiskManagement.Api/              API-Schicht (Controllers, Program.cs)
```

---

### 2. Customer Management (Unterstützende Domäne)

Verwaltet Kundenstammdaten. Eigenständiger Bounded Context mit eigener Datenbank.

**Aggregates:**

| Aggregate | Root Entity | Beschreibung |
|---|---|---|
| `CustomerAggregate` | `Customer` | Kundenstammdaten (Name, Adresse, Beschäftigungsstatus) |

**Domain Events:**

| Event | Auslöser |
|---|---|
| `CustomerCreatedEvent` | Neuer Kunde angelegt |
| `CustomerUpdatedEvent` | Kundendaten geändert |
| `CustomerActivatedEvent` | Kunde aktiviert |
| `CustomerArchivedEvent` | Kunde archiviert |
| `CustomerDeletedEvent` | Kunde gelöscht |

**Projektstruktur:**

```
CustomerManagement.Domain/           Domain-Schicht
CustomerManagement.Application/      Application-Schicht (Commands, Queries, DTOs, Validation)
CustomerManagement.Infrastructure/   Infrastructure-Schicht (EF Core, Persistence)
CustomerManagement.Api/              API-Schicht (Controllers, Program.cs)
```

---

### 3. Shared Kernel

Gemeinsam genutzte Bausteine, die in beiden Bounded Contexts verwendet werden.

| Komponente | Beschreibung |
|---|---|
| `AggregateRoot<TId>` | Basisklasse für Aggregate Roots mit Domain Event Support |
| `Entity<TId>` | Basisklasse für Entities |
| `ValueObject` | Basisklasse für Value Objects (Equality by value) |
| `IDispatcher` | CQRS Dispatcher (SendAsync, QueryAsync, PublishAsync, PublishDomainEventsAsync) |
| `ICommand<T>` / `IQuery<T>` | Marker-Interfaces für Commands und Queries |
| `ICommandHandler<TCommand, TResult>` | Command Handler Interface |
| `IQueryHandler<TQuery, TResult>` | Query Handler Interface |
| `IDomainEventHandler<TEvent>` | Domain Event Handler Interface |
| `Result<T>` | Result Pattern (Success, Failure, NotFound, Forbidden, ValidationFailure) |
| `EmailAddress` | Stark typisiertes Value Object |
| `InternalAuthMiddleware` | API-Key-basierte Service-zu-Service-Authentifizierung |
| `ApiKeyAuthMiddleware` | API-Key-Validierung für interne Endpoints |
| `ExceptionHandlingMiddleware` | Globale Exception-Behandlung mit strukturierten Fehlerantworten |
| `KeycloakRoleClaimsTransformer` | Transformiert Keycloak-OIDC-Rollen in ASP.NET Claims |
| `DomainEventDispatchInterceptor` | EF Core Interceptor für automatisches Dispatchen von Domain Events bei SaveChanges |
| `Customer*IntegrationEvent` | Integration Events (Created, Updated, Activated, Archived, Deleted) für Cross-BC-Sync |

---

### 4. Frontend (SvelteKit)

Single-Page Application als BFF (Backend for Frontend) mit Server-Side Rendering.

| Bereich | Route | Rolle |
|---|---|---|
| Antragsteller | `/applications` | `applicant` |
| Sachbearbeiter | `/processor` | `processor` |
| Risikomanager | `/risk-manager/scoring-config` | `risk_manager` |
| Kundenverwaltung | `/customers` | `applicant`, `processor` |
| Authentifizierung | `/login`, `/auth` | alle |

---

## Beziehungen zwischen Bounded Contexts

### Risk Management → Customer Management

**Beziehungstyp:** Customer/Supplier mit Anti-Corruption Layer (ACL)

```
Risk Management                          Customer Management
┌─────────────────────┐                  ┌─────────────────────┐
│                     │                  │                     │
│  ICustomerProfile-  │   HTTP GET       │  /api/internal/     │
│  Service            │─────────────────►│  customers/{id}     │
│  (Application Layer)│                  │  (Internal API)     │
│                     │                  │                     │
│  CustomerService-   │  Übersetzt       │                     │
│  Client             │  Response →      │                     │
│  (Infrastructure)   │  CustomerProfile │                     │
│                     │                  │                     │
└─────────────────────┘                  └─────────────────────┘
```

- **Supplier:** Customer Management stellt einen internen API-Endpunkt bereit (`/api/internal/customers/{id}`)
- **Customer:** Risk Management konsumiert diese API über `CustomerServiceClient`
- **ACL:** `ICustomerProfileService` übersetzt die externe Antwort in domänen-eigene Records (`CustomerProfile`, `CustomerAddress`)
- **Authentifizierung:** Service-zu-Service via `X-Api-Key` Header + `InternalAuthMiddleware`

### Customer Management → Risk Management

**Beziehungstyp:** Indirekte Referenz + Event Publishing (Outbox Pattern)

- Customer Management kennt die `APPLICATION_SERVICE_URL` des Risk Management API
- Wird verwendet, um ggf. antragsrelevante Informationen zu prüfen (z.B. vor Kundenlöschung via `IApplicationServiceClient`)
- Customer Management publiziert **Integration Events** (`CustomerCreated`, `CustomerUpdated`, `CustomerActivated`, `CustomerArchived`, `CustomerDeleted`) über RabbitMQ
- Verwendet das **Outbox Pattern** (`AddEntityFrameworkOutbox<CustomerDbContext>` + `UseBusOutbox()`), um sicherzustellen, dass Events nur bei erfolgreicher DB-Transaktion gesendet werden

### Shared Kernel ← beide Bounded Contexts

**Beziehungstyp:** Shared Kernel

Beide Bounded Contexts referenzieren das `SharedKernel`-Projekt für:
- Basisklassen (`AggregateRoot`, `Entity`, `ValueObject`)
- CQRS Dispatcher und Handler-Interfaces
- Result Pattern
- Gemeinsame Value Objects (`EmailAddress`)
- Middleware (`InternalAuthMiddleware`)

### Risk Management → SCHUFA (External Service)

**Beziehungstyp:** Anti-Corruption Layer

```
Risk Management
┌─────────────────────┐
│                     │
│  ICreditCheck-      │   Aktuell: MockSchufaProvider
│  Service            │──────────────────► (simuliert Antworten)
│  (Domain Service)   │
│                     │   Zukünftig: echte SCHUFA API
│                     │
└─────────────────────┘
```

- Die Domain definiert `ICreditCheckService` als Port
- Die Infrastructure implementiert `MockSchufaProvider` als Adapter
- Das Interface ist so gestaltet, dass es durch eine echte SCHUFA-Anbindung ersetzt werden kann

### Risk Management → RabbitMQ (Messaging)

**Beziehungstyp:** Infrastructure / Technical Concern

```
Risk Management
┌─────────────────────┐
│                     │
│  CreateApplication- │  Publish
│  Handler            │──────────► ApplicationCreationStarted
│                     │                     │
│  ApplicationCreation│                     ▼
│  StateMachine       │◄──── orchestriert Saga via RabbitMQ
│                     │
│  8 Consumers        │◄──── empfangen Commands von Saga
│  (inkl. Fault +     │
│   MarkFailed)       │
└─────────────────────┘
```

- MassTransit nutzt RabbitMQ als Transport
- Saga State wird in PostgreSQL persistiert (gleiche DB)
- Consumers laufen im selben Prozess wie die API

### Alle Services → Keycloak (Identity Provider)

**Beziehungstyp:** Conformist

- Beide APIs nutzen Keycloak als OIDC Identity Provider
- JWT-Token-Validierung über `JwtBearer` Middleware
- Rollen: `applicant`, `processor`, `risk_manager`
- Frontend nutzt Authorization Code Flow mit PKCE

## Deployment-Topologie (Aspire)

```
┌──────────────────────────────────────────────────┐
│                   AppHost (Aspire)                │
│                                                  │
│   ┌──────────┐  ┌──────────┐  ┌──────────────┐  │
│   │ risk-api │  │customer- │  │   Frontend   │  │
│   │          │  │  api     │  │  (SvelteKit) │  │
│   └────┬─────┘  └────┬─────┘  └──────────────┘  │
│        │             │                           │
│   ┌────▼─────────────▼─────┐  ┌──────────────┐  │
│   │   PostgreSQL           │  │  RabbitMQ    │  │
│   │   - risk-management    │  │  (messaging) │  │
│   │   - customer-management│  │              │  │
│   └────────────────────────┘  └──────────────┘  │
│                                                  │
│   ┌────────────────────────┐                     │
│   │   Keycloak             │                     │
│   │   (Identity Provider)  │                     │
│   └────────────────────────┘                     │
│                                                  │
└──────────────────────────────────────────────────┘
```

| Service | Port | Datenbank |
|---|---|---|
| `risk-api` | dynamisch (Aspire) | `risk-management` |
| `customer-api` | dynamisch (Aspire) | `customer-management` |
| PostgreSQL | 5432 | — |
| RabbitMQ | 5672 / 15672 (Management) | — |
| Keycloak | 8081 | eingebettet |

## Zusammenfassung der Beziehungsmuster

| Von | Nach | Muster | Transport |
|---|---|---|---|
| Risk Management | Customer Management | Customer/Supplier + ACL | HTTP (synchron) |
| Customer Management | Risk Management | Indirekte Referenz + Outbox Pattern | HTTP (synchron) + AMQP (Integration Events) |
| Risk Management | SCHUFA | ACL (Port/Adapter) | In-Process (Mock) |
| Risk Management | RabbitMQ | Saga Orchestration | AMQP (asynchron) |
| Beide BCs | Shared Kernel | Shared Kernel | Projekt-Referenz |
| Beide BCs | Keycloak | Conformist | OIDC / JWT |
| Frontend | Beide APIs | BFF Proxy | HTTP |
