# Saga Pattern — Asynchrone Antragserstellung

## Was ist das Saga Pattern?

Das Saga Pattern ist ein Entwurfsmuster für verteilte Transaktionen, bei dem ein langlebiger Geschäftsprozess in eine Folge von einzelnen, unabhängigen Schritten zerlegt wird. Jeder Schritt wird durch ein Event ausgelöst und produziert selbst wieder ein Event, das den nächsten Schritt anstößt. Eine zentrale **State Machine** (Orchestrator) verwaltet den aktuellen Zustand und entscheidet, welcher Schritt als nächstes ausgeführt wird.

Im Gegensatz zu einer klassischen verteilten Transaktion (2-Phase-Commit) bietet die Saga:

- **Lose Kopplung** — Jeder Schritt ist ein eigener Consumer, unabhängig deploybar
- **Fehlerbehandlung** — Jeder Zustand kann auf Fehler reagieren und kompensieren
- **Persistenz** — Der Saga-State wird in der Datenbank gespeichert, überlebt Neustarts
- **Sichtbarkeit** — Der aktuelle Zustand ist jederzeit abfragbar

## Umsetzung in dieser Anwendung

### Technologie-Stack

| Komponente | Technologie |
|---|---|
| Saga Framework | **MassTransit** (State Machine) |
| Message Broker | **RabbitMQ** |
| State Persistence | **EF Core + PostgreSQL** |
| Transport | AMQP via `MassTransit.RabbitMQ` |

### Anwendungsfall: Asynchrone Antragserstellung

Wenn ein Antragsteller einen neuen Kreditantrag erstellt, werden mehrere Schritte ausgeführt, die externe Services aufrufen und Zeit benötigen:

1. **Kundendaten abrufen** — vom CustomerManagement Bounded Context
2. **Bonitätsprüfung durchführen** — via SCHUFA (Mock)
3. **Antrag finalisieren** — Scoring berechnen, Status setzen

Statt all diese Schritte synchron in einem einzigen Request auszuführen, wird der Prozess asynchron über eine Saga orchestriert.

### Ablauf im Detail

```
Client                API              Saga              Consumers
  |                    |                 |                    |
  |--- POST /api/applications --------->|                    |
  |                    |                 |                    |
  |    Application(Status=Processing)   |                    |
  |    erstellt + gespeichert           |                    |
  |                    |                 |                    |
  |<-- 202 Accepted ---|                 |                    |
  |                    |                 |                    |
  |                    |-- ApplicationCreationStarted ------->|
  |                    |                 |                    |
  |                    |                 |-- FetchingCustomer |
  |                    |                 |                    |
  |                    |                 |    FetchCustomerProfileConsumer
  |                    |                 |    ruft CustomerManagement API auf
  |                    |                 |                    |
  |                    |                 |<-- CustomerProfileFetched
  |                    |                 |                    |
  |                    |                 |-- CheckingCredit   |
  |                    |                 |                    |
  |                    |                 |    PerformCreditCheckConsumer
  |                    |                 |    ruft SCHUFA Mock auf
  |                    |                 |                    |
  |                    |                 |<-- CreditCheckCompleted
  |                    |                 |                    |
  |                    |                 |-- Finalizing       |
  |                    |                 |                    |
  |                    |                 |    FinalizeApplicationConsumer
  |                    |                 |    berechnet Score, setzt Status
  |                    |                 |                    |
  |                    |                 |<-- ApplicationCreationCompleted
  |                    |                 |                    |
  |                    |                 |-- Completed        |
  |                    |                 |   (Saga entfernt)  |
  |                    |                 |                    |
  |--- GET /api/applications/{id} ----->|                    |
  |<-- Application(Status=Draft) -------|                    |
```

### State Machine — Zustände und Übergänge

```
                    ApplicationCreationStarted
                              |
                              v
                      ┌───────────────┐
                      │ FetchingCustomer │
                      └───────┬───────┘
                              |
                   CustomerProfileFetched
                              |
                              v
                      ┌───────────────┐
                      │ CheckingCredit  │
                      └───────┬───────┘
                              |
                    CreditCheckCompleted
                              |
                              v
                      ┌───────────────┐
                      │  Finalizing     │
                      └───────┬───────┘
                              |
              ApplicationCreationCompleted
                              |
                              v
                      ┌───────────────┐
                      │  Completed ✓    │
                      └───────────────┘

    ──── Von JEDEM Zustand ────
              |
    ApplicationCreationFailed
              |
              v
      ┌───────────────┐
      │   Failed ✗      │
      └───────────────┘
```

### Beteiligte Komponenten

#### 1. Saga State (`ApplicationCreationState`)

**Datei:** `RiskManagement.Application/Sagas/ApplicationCreation/ApplicationCreationState.cs`

Speichert alle Zwischenergebnisse der Saga in der Datenbank-Tabelle `saga_application_creation_state`:

- **Eingabedaten** — `ApplicationId`, `CustomerId`, `Income`, `FixedCosts`, `DesiredRate`, `UserEmail`, `AutoSubmit`
- **Kundendaten** (nach Schritt 1) — `FirstName`, `LastName`, `EmploymentStatus`, `DateOfBirth`, Adressdaten
- **Bonitätsdaten** (nach Schritt 2) — `HasPaymentDefault`, `CreditScore`, `CreditCheckedAt`, `CreditProvider`
- **Metadaten** — `CurrentState`, `FailureReason`, `CreatedAt`, `CompletedAt`

#### 2. State Machine (`ApplicationCreationStateMachine`)

**Datei:** `RiskManagement.Infrastructure/Sagas/ApplicationCreationStateMachine.cs`

Definiert die Zustände, Events und Übergänge:

- `Initially` → empfängt `ApplicationCreationStarted`, publiziert `FetchCustomerProfile`, wechselt zu `FetchingCustomer`
- `During(FetchingCustomer)` → empfängt `CustomerProfileFetched`, publiziert `PerformCreditCheck`, wechselt zu `CheckingCredit`
- `During(CheckingCredit)` → empfängt `CreditCheckCompleted`, publiziert `FinalizeApplication`, wechselt zu `Finalizing`
- `During(Finalizing)` → empfängt `ApplicationCreationCompleted`, wechselt zu `Completed`, finalisiert
- `DuringAny` → empfängt `ApplicationCreationFailed`, wechselt zu `Failed`, finalisiert

#### 3. Saga Messages (Events)

**Verzeichnis:** `RiskManagement.Application/Sagas/ApplicationCreation/Events/`

| Message | Typ | Beschreibung |
|---|---|---|
| `ApplicationCreationStarted` | Trigger | Startet die Saga mit Antragsdaten |
| `FetchCustomerProfile` | Command | Fordert Kundenprofil an |
| `CustomerProfileFetched` | Event | Kundendaten wurden abgerufen |
| `PerformCreditCheck` | Command | Fordert Bonitätsprüfung an |
| `CreditCheckCompleted` | Event | Bonitätsdaten liegen vor |
| `FinalizeApplication` | Command | Fat Message mit allen gesammelten Daten |
| `ApplicationCreationCompleted` | Event | Antrag erfolgreich finalisiert |
| `ApplicationCreationFailed` | Event | Fehler in einem beliebigen Schritt |

#### 4. Consumers

**Verzeichnis:** `RiskManagement.Infrastructure/Sagas/Consumers/`

| Consumer | Injizierte Services | Aufgabe |
|---|---|---|
| `FetchCustomerProfileConsumer` | `ICustomerProfileService` | Ruft Kundendaten über HTTP vom CustomerManagement BC ab |
| `PerformCreditCheckConsumer` | `ICreditCheckService` | Führt Bonitätsprüfung via SCHUFA Mock durch |
| `FinalizeApplicationConsumer` | `IApplicationRepository`, `IScoringConfigRepository`, `IScoringService` | Lädt den Antrag, ruft `Finalize()` (+ optional `Submit()`) auf, speichert |

#### 5. Command Handler (Einstiegspunkt)

**Datei:** `RiskManagement.Application/Commands/CreateApplicationHandler.cs`

```
1. Validierung der Eingabedaten
2. Application.CreateProcessing() → Status = Processing
3. Repository.AddAsync() + SaveChangesAsync()
4. Publish(ApplicationCreationStarted) → startet die Saga
5. Return 202 Accepted
```

### Frontend-Integration

Das Frontend reagiert auf den asynchronen Ablauf:

1. **POST** erstellt den Antrag → erhält **HTTP 202 Accepted** mit der Application-ID
2. Redirect zur Detailseite mit `?processing=true`
3. **Polling** alle 2 Sekunden auf `GET /api/applications/{id}` bis `status != "processing"`
4. Bei **Erfolg** → Antrag wird mit Score und Status `draft` oder `submitted` angezeigt
5. Bei **Fehler** → Fehlermeldung mit `failureReason`, Buttons "Erneut versuchen" und "Löschen"

### MassTransit-Konfiguration

**Datei:** `RiskManagement.Infrastructure/DependencyInjection.cs` → `AddMessaging()`

```csharp
services.AddMassTransit(x =>
{
    x.AddConsumer<FetchCustomerProfileConsumer>();
    x.AddConsumer<PerformCreditCheckConsumer>();
    x.AddConsumer<FinalizeApplicationConsumer>();

    x.AddSagaStateMachine<ApplicationCreationStateMachine, ApplicationCreationState>()
        .EntityFrameworkRepository(r =>
        {
            r.ExistingDbContext<ApplicationDbContext>();
            r.UsePostgres();
        });

    x.SetKebabCaseEndpointNameFormatter();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(new Uri(connectionString));
        cfg.UseMessageRetry(r => r.Intervals(1s, 5s, 15s, 30s));
        cfg.ConfigureEndpoints(context);
    });
});
```

### Fehlerbehandlung

- **Retry Policy** — MassTransit wiederholt fehlgeschlagene Nachrichten automatisch (1s, 5s, 15s, 30s)
- **Globaler Fehler-Handler** — `DuringAny(ApplicationCreationFailed)` fängt Fehler aus jedem Zustand ab
- **Domain-Level** — `Application.MarkFailed(reason)` speichert den Fehlergrund und setzt Status auf `Failed`
- **Consumer-Level** — `FinalizeApplicationConsumer` fängt Exceptions, ruft `MarkFailed()` auf und publiziert `ApplicationCreationFailed`
- **Saga Cleanup** — `SetCompletedWhenFinalized()` entfernt abgeschlossene und fehlgeschlagene Saga-Instanzen aus der Datenbank

### Domain-Auswirkungen auf das Application Aggregate

Die Saga erforderte folgende Erweiterungen des `Application` Aggregates:

| Methode/Property | Beschreibung |
|---|---|
| `ApplicationStatus.Processing` | Neuer Status während der Saga läuft |
| `ApplicationStatus.Failed` | Neuer Status bei Saga-Fehler |
| `FailureReason` (string?) | Speichert den Fehlergrund |
| `CreateProcessing()` | Factory für Anträge im Processing-Status (ohne CreditReport/Score) |
| `Finalize()` | Setzt EmploymentStatus, CreditReport, berechnet Score → wechselt zu Draft |
| `MarkFailed(reason)` | Speichert Fehlergrund → wechselt zu Failed |
| `Delete()` | Erlaubt Löschung von `Failed`-Anträgen (zusätzlich zu `Draft`) |
