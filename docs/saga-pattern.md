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

### Anwendungsfall: Asynchrone Antragserstellung und -aktualisierung

Die Saga wird in **zwei Szenarien** ausgelöst:

1. **Neue Antragserstellung** (`ApplicationCreationStarted`) — Ein neuer Antrag wird erstellt
2. **Antragsaktualisierung / Einreichung** (`ApplicationUpdateStarted`) — Ein bestehender Draft wird aktualisiert oder eingereicht

In beiden Fällen werden mehrere Schritte ausgeführt, die externe Services aufrufen und Zeit benötigen:

1. **Kundendaten abrufen** — vom CustomerManagement Bounded Context
2. **Bonitätsprüfung durchführen** — via SCHUFA (Mock)
3. **Antrag finalisieren** — Scoring berechnen, Status setzen

Statt all diese Schritte synchron in einem einzigen Request auszuführen, wird der Prozess asynchron über eine Saga orchestriert.

Der Unterschied zwischen den beiden Pfaden wird über das Feld `OperationType` im Saga State gesteuert:

| OperationType | Trigger-Event | Finalize-Command | Consumer |
|---|---|---|---|
| `"Create"` | `ApplicationCreationStarted` | `FinalizeApplication` | `FinalizeApplicationConsumer` |
| `"Update"` | `ApplicationUpdateStarted` | `FinalizeApplicationUpdate` | `FinalizeApplicationUpdateConsumer` |

### Ablauf im Detail

#### Pfad A: Neue Antragserstellung (Create)

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
  |                    |                 |  OperationType=Create
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
  |                    |                 |    Finalize() + optional Submit()
  |                    |                 |                    |
  |                    |                 |<-- ApplicationCreationCompleted
  |                    |                 |                    |
  |                    |                 |-- Completed        |
  |                    |                 |   (Saga entfernt)  |
  |                    |                 |                    |
  |--- GET /api/applications/{id} ----->|                    |
  |<-- Application(Status=Draft) -------|                    |
```

#### Pfad B: Antragsaktualisierung (Update/Submit)

```
Client                API              Saga              Consumers
  |                    |                 |                    |
  |--- PUT /api/applications/{id} ----->|                    |
  |    (oder POST .../submit)           |                    |
  |                    |                 |                    |
  |    Application.SetProcessing()      |                    |
  |    Draft → Processing              |                    |
  |                    |                 |                    |
  |<-- 200 OK ---------|                 |                    |
  |                    |                 |                    |
  |                    |-- ApplicationUpdateStarted --------->|
  |                    |                 |  OperationType=Update
  |                    |                 |-- FetchingCustomer |
  |                    |                 |                    |
  |                    |                 |    (gleiche Schritte wie Pfad A)
  |                    |                 |                    |
  |                    |                 |-- Finalizing       |
  |                    |                 |                    |
  |                    |                 |    FinalizeApplicationUpdateConsumer
  |                    |                 |    UpdateDetails() + optional Submit()
  |                    |                 |                    |
  |                    |                 |<-- ApplicationCreationCompleted
  |                    |                 |                    |
  |                    |                 |-- Completed        |
  |                    |                 |   (Saga entfernt)  |
```

### State Machine — Zustände und Übergänge

```
    ApplicationCreationStarted          ApplicationUpdateStarted
    (OperationType=Create)              (OperationType=Update)
                    \                        /
                     \                      /
                      v                    v
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
                    ┌─────────┬─────────┐
                    |  OperationType?  |
                    └────┬────┴────┬────┘
               Create|              |Update
     FinalizeApplication    FinalizeApplicationUpdate
                    |              |
                    v              v
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
      │   Failed ✗      │  → publiziert MarkApplicationFailed
      └───────────────┘     → MarkApplicationFailedConsumer
                              setzt Application.MarkFailed()
```

### Beteiligte Komponenten

#### 1. Saga State (`ApplicationCreationState`)

**Datei:** `RiskManagement.Application/Sagas/ApplicationCreation/ApplicationCreationState.cs`

Speichert alle Zwischenergebnisse der Saga in der Datenbank-Tabelle `saga_application_creation_state`:

- **Eingabedaten** — `ApplicationId`, `CustomerId`, `Income`, `FixedCosts`, `DesiredRate`, `UserEmail`, `AutoSubmit`
- **Operationstyp** — `OperationType` (`"Create"` oder `"Update"`) — bestimmt den Finalisierungspfad
- **Kundendaten** (nach Schritt 1) — `FirstName`, `LastName`, `EmploymentStatus`, `DateOfBirth`, Adressdaten
- **Bonitätsdaten** (nach Schritt 2) — `HasPaymentDefault`, `CreditScore`, `CreditCheckedAt`, `CreditProvider`
- **Metadaten** — `CurrentState`, `FailureReason`, `CreatedAt`, `CompletedAt`

#### 2. State Machine (`ApplicationCreationStateMachine`)

**Datei:** `RiskManagement.Infrastructure/Sagas/ApplicationCreationStateMachine.cs`

Definiert die Zustände, Events und Übergänge:

- `Initially` → empfängt `ApplicationCreationStarted` (OperationType=Create), publiziert `FetchCustomerProfile`, wechselt zu `FetchingCustomer`
- `Initially` → empfängt `ApplicationUpdateStarted` (OperationType=Update), publiziert `FetchCustomerProfile`, wechselt zu `FetchingCustomer`
- `During(FetchingCustomer)` → empfängt `CustomerProfileFetched`, publiziert `PerformCreditCheck`, wechselt zu `CheckingCredit`
- `During(CheckingCredit)` → empfängt `CreditCheckCompleted`:
  - Wenn `OperationType == "Create"`: publiziert `FinalizeApplication`
  - Wenn `OperationType == "Update"`: publiziert `FinalizeApplicationUpdate`
  - Wechselt zu `Finalizing`
- `During(Finalizing)` → empfängt `ApplicationCreationCompleted`, wechselt zu `Completed`, finalisiert
- `DuringAny` → empfängt `ApplicationCreationFailed`, publiziert `MarkApplicationFailed`, wechselt zu `Failed`, finalisiert

#### 3. Saga Messages (Events)

**Verzeichnis:** `RiskManagement.Application/Sagas/ApplicationCreation/Events/`

| Message | Typ | Beschreibung |
|---|---|---|
| `ApplicationCreationStarted` | Trigger | Startet die Saga für neue Anträge (OperationType=Create) |
| `ApplicationUpdateStarted` | Trigger | Startet die Saga für Updates/Einreichungen (OperationType=Update) |
| `FetchCustomerProfile` | Command | Fordert Kundenprofil an |
| `CustomerProfileFetched` | Event | Kundendaten wurden abgerufen |
| `PerformCreditCheck` | Command | Fordert Bonitätsprüfung an |
| `CreditCheckCompleted` | Event | Bonitätsdaten liegen vor |
| `FinalizeApplication` | Command | Fat Message — finalisiert neuen Antrag (Create-Pfad) |
| `FinalizeApplicationUpdate` | Command | Fat Message — aktualisiert bestehenden Antrag (Update-Pfad) |
| `MarkApplicationFailed` | Command | Markiert den Antrag als fehlgeschlagen in der Domain |
| `ApplicationCreationCompleted` | Event | Antrag erfolgreich finalisiert |
| `ApplicationCreationFailed` | Event | Fehler in einem beliebigen Schritt |

#### 4. Consumers

**Verzeichnis:** `RiskManagement.Infrastructure/Sagas/Consumers/`

| Consumer | Injizierte Services | Aufgabe |
|---|---|---|
| `FetchCustomerProfileConsumer` | `ICustomerProfileService` | Ruft Kundendaten über HTTP vom CustomerManagement BC ab |
| `PerformCreditCheckConsumer` | `ICreditCheckService` | Führt Bonitätsprüfung via SCHUFA Mock durch |
| `FinalizeApplicationConsumer` | `IApplicationRepository`, `IScoringConfigRepository`, `IScoringService` | Lädt neuen Antrag, ruft `Finalize()` (+ optional `Submit()`) auf, speichert |
| `FinalizeApplicationUpdateConsumer` | `IApplicationRepository`, `IScoringConfigRepository`, `IScoringService` | Lädt bestehenden Antrag, ruft `UpdateDetails()` (+ optional `Submit()`) auf, speichert |
| `MarkApplicationFailedConsumer` | `IApplicationRepository` | Lädt den Antrag und ruft `MarkFailed(reason)` auf (nur wenn Status == Processing) |

#### 5. Command Handler (Einstiegspunkte)

**Neue Anträge:** `RiskManagement.Application/Commands/CreateApplicationHandler.cs`

```
1. Validierung der Eingabedaten
2. Application.CreateProcessing() → Status = Processing
3. Repository.AddAsync() + SaveChangesAsync()
4. Publish(ApplicationCreationStarted) → startet die Saga (OperationType=Create)
5. Return 202 Accepted
```

**Update bestehender Anträge:** `RiskManagement.Application/Commands/UpdateApplicationHandler.cs`

```
1. Antrag laden und Besitz prüfen (CreatedBy == UserEmail)
2. Status prüfen (muss Draft sein)
3. Validierung der Eingabedaten
4. Application.SetProcessing() → Draft → Processing
5. SaveChangesAsync()
6. Publish(ApplicationUpdateStarted, AutoSubmit=false) → startet die Saga (OperationType=Update)
```

**Einreichung:** `RiskManagement.Application/Commands/SubmitApplicationHandler.cs`

```
1. Antrag laden und Besitz prüfen
2. Status prüfen (muss Draft sein)
3. Application.SetProcessing() → Draft → Processing
4. SaveChangesAsync()
5. Publish(ApplicationUpdateStarted, AutoSubmit=true) → startet die Saga (OperationType=Update)
```

**Update + Einreichung:** `RiskManagement.Application/Commands/UpdateAndSubmitApplicationHandler.cs`

```
1. Antrag laden und Besitz prüfen
2. Status prüfen (muss Draft sein)
3. Validierung der Eingabedaten
4. Application.SetProcessing() → Draft → Processing
5. SaveChangesAsync()
6. Publish(ApplicationUpdateStarted, AutoSubmit=true) → startet die Saga (OperationType=Update)
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
    // Saga Consumers
    x.AddConsumer<FetchCustomerProfileConsumer>();
    x.AddConsumer<FetchCustomerProfileFaultConsumer>();
    x.AddConsumer<PerformCreditCheckConsumer>();
    x.AddConsumer<PerformCreditCheckFaultConsumer>();
    x.AddConsumer<FinalizeApplicationConsumer>();
    x.AddConsumer<FinalizeApplicationUpdateConsumer>();
    x.AddConsumer<FinalizeApplicationUpdateFaultConsumer>();
    x.AddConsumer<MarkApplicationFailedConsumer>();

    // Cross-BC Integration Event Consumers
    x.AddConsumer<CustomerCreatedConsumer>();
    x.AddConsumer<CustomerUpdatedConsumer>();
    x.AddConsumer<CustomerActivatedConsumer>();
    x.AddConsumer<CustomerArchivedConsumer>();
    x.AddConsumer<CustomerDeletedConsumer>();

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
- **Fault Consumers** — Fangen MassTransit `Fault<T>` Events ab, wenn ein Consumer nach allen Retries endgültig fehlschlägt:
  - `FetchCustomerProfileFaultConsumer` → `Fault<FetchCustomerProfile>` → publiziert `ApplicationCreationFailed`
  - `PerformCreditCheckFaultConsumer` → `Fault<PerformCreditCheck>` → publiziert `ApplicationCreationFailed`
  - `FinalizeApplicationUpdateFaultConsumer` → `Fault<FinalizeApplicationUpdate>` → publiziert `ApplicationCreationFailed`
- **Globaler Fehler-Handler** — `DuringAny(ApplicationCreationFailed)` fängt Fehler aus jedem Zustand ab und publiziert `MarkApplicationFailed`
- **MarkApplicationFailedConsumer** — Empfängt `MarkApplicationFailed`, lädt den Antrag und ruft `Application.MarkFailed(reason)` auf (nur wenn Status == Processing)
- **Consumer-Level** — `FinalizeApplicationConsumer` und `FinalizeApplicationUpdateConsumer` fangen Exceptions direkt, rufen `MarkFailed()` auf und publizieren `ApplicationCreationFailed`
- **Saga Cleanup** — `SetCompletedWhenFinalized()` entfernt abgeschlossene und fehlgeschlagene Saga-Instanzen aus der Datenbank

### Domain-Auswirkungen auf das Application Aggregate

Die Saga erforderte folgende Erweiterungen des `Application` Aggregates:

| Methode/Property | Beschreibung |
|---|---|
| `ApplicationStatus.Processing` | Neuer Status während der Saga läuft |
| `ApplicationStatus.Failed` | Neuer Status bei Saga-Fehler |
| `FailureReason` (string?) | Speichert den Fehlergrund |
| `CreateProcessing()` | Factory für Anträge im Processing-Status (ohne CreditReport/Score) |
| `SetProcessing()` | Setzt einen Draft-Antrag zurück in den Processing-Status (für Update/Submit) |
| `Finalize()` | Setzt EmploymentStatus, CreditReport, berechnet Score → wechselt zu Draft |
| `UpdateDetails()` | Aktualisiert bestehende Antragsdaten und berechnet Score neu (kein Status-Wechsel) |
| `MarkFailed(reason)` | Speichert Fehlergrund → wechselt zu Failed |
| `Delete()` | Erlaubt Löschung von `Failed`-Anträgen (zusätzlich zu `Draft`) |
