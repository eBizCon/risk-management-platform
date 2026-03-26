using MassTransit;
using RiskManagement.Application.Sagas.ApplicationCreation;
using RiskManagement.Application.Sagas.ApplicationCreation.Events;

namespace RiskManagement.Infrastructure.Sagas;

/// <summary>
/// State Machine (Zustandsautomat) für die koordinierte Erstellung eines Kreditantrags.
/// 
/// WAS IST EINE SAGA?
/// Eine Saga ist ein Entwurfsmuster für langlaufende Prozesse, die mehrere Schritte umfassen.
/// Anders als eine einzelne Transaktion kann eine Saga über längere Zeit laufen und muss
/// ihren Zustand persistent speichern, um bei Fehlern oder Systemneustarts fortgesetzt werden zu können.
/// 
/// WAS IST EINE STATE MACHINE?
/// Eine State Machine (Zustandsautomat) definiert eine Reihe von Zuständen und die Übergänge zwischen ihnen.
/// Sie reagiert auf Events (Ereignisse) und wechselt basierend darauf von einem Zustand in einen anderen.
/// 
/// DIESER PROZESS:
/// Diese State Machine orchestriert die Antragserstellung in folgenden Schritten:
/// 1. Start: Antrag wird initiiert → Kundenprofil abrufen
/// 2. Kundenprofil abgerufen → Bonitätsprüfung durchführen
/// 3. Bonitätsprüfung abgeschlossen → Antrag finalisieren
/// 4. Finalisierung abgeschlossen → Prozess beendet
/// 
/// Der ApplicationCreationState speichert alle Zwischendaten (wie ein "Arbeitsblatt"),
/// damit der Prozess bei Bedarf fortgesetzt werden kann.
/// </summary>
public class ApplicationCreationStateMachine : MassTransitStateMachine<ApplicationCreationState>
{
    // =====================================================================
    // ZUSTÄNDE (STATES)
    // Jeder State repräsentiert eine Phase im Prozess.
    // Die Saga kann sich immer nur in EINEM Zustand gleichzeitig befinden.
    // =====================================================================

    /// <summary>
    /// Zustand: Kundenprofil wird gerade vom externen Kundensystem abgerufen.
    /// Wir warten auf die Antwort mit den Kundendaten.
    /// </summary>
    public State FetchingCustomer { get; private set; } = null!;

    /// <summary>
    /// Zustand: Bonitätsprüfung wird bei einem externen Anbieter durchgeführt.
    /// Wir warten auf das Ergebnis (Credit Score, Payment Defaults).
    /// </summary>
    public State CheckingCredit { get; private set; } = null!;

    /// <summary>
    /// Zustand: Alle Daten sind gesammelt, der Antrag wird nun final erstellt
    /// und ggf. automatisch eingereicht.
    /// </summary>
    public State Finalizing { get; private set; } = null!;

    /// <summary>
    /// Endzustand: Der Prozess wurde erfolgreich abgeschlossen.
    /// Der Antrag ist vollständig erstellt und ggf. eingereicht.
    /// </summary>
    public State Completed { get; private set; } = null!;

    /// <summary>
    /// Endzustand: Der Prozess ist fehlgeschlagen.
    /// Der Grund für den Fehler wird im State gespeichert.
    /// </summary>
    public State Failed { get; private set; } = null!;

    // =====================================================================
    // EVENTS (EREIGNISSE)
    // Events sind Nachrichten, die den Zustandsautomaten antreiben.
    // Wenn ein Event eintritt, reagiert die State Machine mit Aktionen
    // und wechselt ggf. in einen neuen Zustand.
    // =====================================================================

    /// <summary>
    /// Event: Ein neuer Antragserstellungsprozess wurde gestartet.
    /// Enthält alle Initialdaten (ApplicationId, CustomerId, Einkommen, etc.).
    /// Dies ist das Start-Event, das die Saga auslöst.
    /// </summary>
    public Event<ApplicationCreationStarted> ApplicationCreationStarted { get; private set; } = null!;

    public Event<ApplicationUpdateStarted> ApplicationUpdateStarted { get; private set; } = null!;

    /// <summary>
    /// Event: Das Kundenprofil wurde erfolgreich abgerufen.
    /// Enthält Name, Adresse, Geburtsdatum, Beschäftigungsstatus etc.
    /// </summary>
    public Event<CustomerProfileFetched> CustomerProfileFetched { get; private set; } = null!;

    /// <summary>
    /// Event: Die Bonitätsprüfung wurde abgeschlossen.
    /// Enthält Credit Score, Payment Default Status und Prüfzeitpunkt.
    /// </summary>
    public Event<CreditCheckCompleted> CreditCheckCompleted { get; private set; } = null!;

    /// <summary>
    /// Event: Der Antrag wurde erfolgreich finalisiert.
    /// Der Prozess ist nun abgeschlossen.
    /// </summary>
    public Event<ApplicationCreationCompleted> ApplicationCreationCompleted { get; private set; } = null!;

    /// <summary>
    /// Event: Ein Fehler ist im Prozess aufgetreten.
    /// Kann in JEDEM Zustand auftreten und führt zum Failed-Zustand.
    /// Enthält den Grund für den Fehler.
    /// </summary>
    public Event<ApplicationCreationFailed> ApplicationCreationFailed { get; private set; } = null!;

    /// <summary>
    /// Konstruktor: Definiert die gesamte State Machine.
    /// Hier werden:
    /// 1. Die Zustände konfiguriert
    /// 2. Die Events mit ihren Korrelationen definiert
    /// 3. Die Übergänge und Aktionen festgelegt
    /// </summary>
    public ApplicationCreationStateMachine()
    {
        // Konfiguriert, wie der aktuelle Zustand im State-Objekt gespeichert wird.
        // Der Zustand wird als String in der Eigenschaft "CurrentState" persistiert.
        InstanceState(x => x.CurrentState);

        // =====================================================================
        // EVENT-KONFIGURATION
        // Hier wird festgelegt, wie Events mit der Saga-Instanz verknüpft werden.
        // =====================================================================

        // Konfiguration des Start-Events:
        // - CorrelateById: Verknüpft das Event mit der Saga über die CorrelationId (eindeutige Prozess-ID)
        // - InsertOnInitial: Wenn das Event eintrifft und noch keine Saga existiert, wird eine neue erstellt
        // - SetSagaFactory: Definiert, wie die neue Saga-Instanz initialisiert wird (alle Startdaten werden kopiert)
        Event(() => ApplicationCreationStarted, e =>
        {
            e.CorrelateById(ctx => ctx.Message.CorrelationId);
            e.InsertOnInitial = true;
            e.SetSagaFactory(ctx => new ApplicationCreationState
            {
                CorrelationId = ctx.Message.CorrelationId,
                ApplicationId = ctx.Message.ApplicationId,
                CustomerId = ctx.Message.CustomerId,
                Income = ctx.Message.Income,
                FixedCosts = ctx.Message.FixedCosts,
                DesiredRate = ctx.Message.DesiredRate,
                UserEmail = ctx.Message.UserEmail,
                AutoSubmit = ctx.Message.AutoSubmit,
                OperationType = "Create",
                CreatedAt = DateTime.UtcNow
            });
        });

        Event(() => ApplicationUpdateStarted, e =>
        {
            e.CorrelateById(ctx => ctx.Message.CorrelationId);
            e.InsertOnInitial = true;
            e.SetSagaFactory(ctx => new ApplicationCreationState
            {
                CorrelationId = ctx.Message.CorrelationId,
                ApplicationId = ctx.Message.ApplicationId,
                CustomerId = ctx.Message.CustomerId,
                Income = ctx.Message.Income,
                FixedCosts = ctx.Message.FixedCosts,
                DesiredRate = ctx.Message.DesiredRate,
                UserEmail = ctx.Message.UserEmail,
                AutoSubmit = ctx.Message.AutoSubmit,
                OperationType = "Update",
                CreatedAt = DateTime.UtcNow
            });
        });

        // Die folgenden Events korrelieren ebenfalls über die CorrelationId.
        // Das bedeutet: Wenn ein Event eintrifft, wird anhand der CorrelationId
        // die passende Saga-Instanz gefunden und geladen.
        Event(() => CustomerProfileFetched, e => e.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => CreditCheckCompleted, e => e.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => ApplicationCreationCompleted, e => e.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => ApplicationCreationFailed, e => e.CorrelateById(ctx => ctx.Message.CorrelationId));

        // =====================================================================
        // ZUSTANDSÜBERGÄNGE UND AKTIONEN
        // Hier wird definiert, was passiert wenn ein Event in einem bestimmten Zustand eintrifft.
        // =====================================================================

        // ---------------------------------------------------------------------
        // ÜBERGANG 1: Initial → FetchingCustomer
        // ---------------------------------------------------------------------
        // "Initially" definiert das Verhalten, wenn die Saga neu erstellt wird.
        // Wenn das ApplicationCreationStarted Event eintrifft:
        // 1. .Then(): Speichere alle Initialdaten im Saga-State (wie ein Arbeitsblatt ausfüllen)
        // 2. .Publish(): Sende eine Nachricht "FetchCustomerProfile" an den Message Broker
        //    Ein anderer Service wird diese Nachricht verarbeiten und das Kundenprofil laden
        // 3. .TransitionTo(): Wechsle in den Zustand "FetchingCustomer"
        //    Wir warten nun auf die Antwort mit den Kundendaten
        Initially(
            When(ApplicationCreationStarted)
                .Then(ctx =>
                {
                    // Speichere alle relevanten Daten im Saga-State für spätere Verwendung
                    ctx.Saga.ApplicationId = ctx.Message.ApplicationId;
                    ctx.Saga.CustomerId = ctx.Message.CustomerId;
                    ctx.Saga.Income = ctx.Message.Income;
                    ctx.Saga.FixedCosts = ctx.Message.FixedCosts;
                    ctx.Saga.DesiredRate = ctx.Message.DesiredRate;
                    ctx.Saga.UserEmail = ctx.Message.UserEmail;
                    ctx.Saga.AutoSubmit = ctx.Message.AutoSubmit;
                    ctx.Saga.OperationType = "Create";
                    ctx.Saga.CreatedAt = DateTime.UtcNow;
                })
                .Publish(ctx => new FetchCustomerProfile(
                    ctx.Saga.CorrelationId,
                    ctx.Saga.CustomerId))
                .TransitionTo(FetchingCustomer));

        Initially(
            When(ApplicationUpdateStarted)
                .Then(ctx =>
                {
                    ctx.Saga.ApplicationId = ctx.Message.ApplicationId;
                    ctx.Saga.CustomerId = ctx.Message.CustomerId;
                    ctx.Saga.Income = ctx.Message.Income;
                    ctx.Saga.FixedCosts = ctx.Message.FixedCosts;
                    ctx.Saga.DesiredRate = ctx.Message.DesiredRate;
                    ctx.Saga.UserEmail = ctx.Message.UserEmail;
                    ctx.Saga.AutoSubmit = ctx.Message.AutoSubmit;
                    ctx.Saga.OperationType = "Update";
                    ctx.Saga.CreatedAt = DateTime.UtcNow;
                })
                .Publish(ctx => new FetchCustomerProfile(
                    ctx.Saga.CorrelationId,
                    ctx.Saga.CustomerId))
                .TransitionTo(FetchingCustomer));

        // ---------------------------------------------------------------------
        // ÜBERGANG 2: FetchingCustomer → CheckingCredit
        // ---------------------------------------------------------------------
        // "During" definiert das Verhalten, wenn die Saga in einem bestimmten Zustand ist.
        // Wenn wir im Zustand "FetchingCustomer" sind und das CustomerProfileFetched Event eintrifft:
        // 1. .Then(): Speichere die Kundendaten (Name, Adresse, Geburtsdatum, etc.) im Saga-State
        // 2. .Publish(): Sende "PerformCreditCheck" Nachricht, um die Bonitätsprüfung zu starten
        //    Ein externer Credit-Check-Service wird dies verarbeiten
        // 3. .TransitionTo(): Wechsle in den Zustand "CheckingCredit"
        //    Wir warten nun auf das Ergebnis der Bonitätsprüfung
        During(FetchingCustomer,
            When(CustomerProfileFetched)
                .Then(ctx =>
                {
                    // Speichere die abgerufenen Kundendaten im Saga-State
                    ctx.Saga.FirstName = ctx.Message.FirstName;
                    ctx.Saga.LastName = ctx.Message.LastName;
                    ctx.Saga.EmploymentStatus = ctx.Message.EmploymentStatus;
                    ctx.Saga.DateOfBirth = ctx.Message.DateOfBirth;
                    ctx.Saga.Street = ctx.Message.Street;
                    ctx.Saga.City = ctx.Message.City;
                    ctx.Saga.ZipCode = ctx.Message.ZipCode;
                    ctx.Saga.Country = ctx.Message.Country;
                })
                .Publish(ctx => new PerformCreditCheck(
                    ctx.Saga.CorrelationId,
                    ctx.Saga.FirstName!,
                    ctx.Saga.LastName!,
                    DateOnly.Parse(ctx.Saga.DateOfBirth!),
                    ctx.Saga.Street!,
                    ctx.Saga.City!,
                    ctx.Saga.ZipCode!,
                    ctx.Saga.Country!))
                .TransitionTo(CheckingCredit));

        // ---------------------------------------------------------------------
        // ÜBERGANG 3: CheckingCredit → Finalizing
        // ---------------------------------------------------------------------
        // Wenn wir im Zustand "CheckingCredit" sind und das CreditCheckCompleted Event eintrifft:
        // 1. .Then(): Speichere die Bonitätsdaten (Credit Score, Payment Defaults, etc.)
        // 2. .Publish(): Sende "FinalizeApplication" mit ALLEN gesammelten Daten
        //    Der Finalize-Service erstellt nun den kompletten Antrag im System
        // 3. .TransitionTo(): Wechsle in den Zustand "Finalizing"
        //    Wir warten auf die Bestätigung, dass der Antrag erstellt wurde
        During(CheckingCredit,
            When(CreditCheckCompleted)
                .Then(ctx =>
                {
                    ctx.Saga.HasPaymentDefault = ctx.Message.HasPaymentDefault;
                    ctx.Saga.CreditScore = ctx.Message.CreditScore;
                    ctx.Saga.CreditCheckedAt = ctx.Message.CheckedAt;
                    ctx.Saga.CreditProvider = ctx.Message.Provider;
                })
                .If(ctx => ctx.Saga.OperationType == "Create",
                    binder => binder.Publish(ctx => new FinalizeApplication(
                        ctx.Saga.CorrelationId,
                        ctx.Saga.ApplicationId,
                        ctx.Saga.CustomerId,
                        ctx.Saga.Income,
                        ctx.Saga.FixedCosts,
                        ctx.Saga.DesiredRate,
                        ctx.Saga.UserEmail,
                        ctx.Saga.EmploymentStatus!,
                        ctx.Saga.HasPaymentDefault!.Value,
                        ctx.Saga.CreditScore,
                        ctx.Saga.CreditCheckedAt!.Value,
                        ctx.Saga.CreditProvider!,
                        ctx.Saga.AutoSubmit)))
                .If(ctx => ctx.Saga.OperationType == "Update",
                    binder => binder.Publish(ctx => new FinalizeApplicationUpdate(
                        ctx.Saga.CorrelationId,
                        ctx.Saga.ApplicationId,
                        ctx.Saga.CustomerId,
                        ctx.Saga.Income,
                        ctx.Saga.FixedCosts,
                        ctx.Saga.DesiredRate,
                        ctx.Saga.UserEmail,
                        ctx.Saga.EmploymentStatus!,
                        ctx.Saga.HasPaymentDefault!.Value,
                        ctx.Saga.CreditScore,
                        ctx.Saga.CreditCheckedAt!.Value,
                        ctx.Saga.CreditProvider!,
                        ctx.Saga.AutoSubmit)))
                .TransitionTo(Finalizing));

        // ---------------------------------------------------------------------
        // ÜBERGANG 4: Finalizing → Completed (Erfolgreicher Abschluss)
        // ---------------------------------------------------------------------
        // Wenn wir im Zustand "Finalizing" sind und das ApplicationCreationCompleted Event eintrifft:
        // 1. .Then(): Setze den Abschlusszeitpunkt
        // 2. .TransitionTo(): Wechsle in den Endzustand "Completed"
        // 3. .Finalize(): Markiere die Saga als abgeschlossen
        //    Die Saga kann nun aus der Datenbank entfernt werden (je nach Konfiguration)
        During(Finalizing,
            When(ApplicationCreationCompleted)
                .Then(ctx => ctx.Saga.CompletedAt = DateTime.UtcNow)
                .TransitionTo(Completed)
                .Finalize());

        // ---------------------------------------------------------------------
        // FEHLERBEHANDLUNG: Jeder Zustand → Failed
        // ---------------------------------------------------------------------
        // "DuringAny" definiert Verhalten, das in JEDEM Zustand gilt.
        // Wenn das ApplicationCreationFailed Event eintrifft (egal in welchem Zustand wir sind):
        // 1. .Then(): Speichere den Fehlergrund und den Zeitpunkt
        // 2. .TransitionTo(): Wechsle in den Fehlerzustand "Failed"
        // 3. .Finalize(): Beende die Saga
        // Dies ermöglicht eine saubere Fehlerbehandlung an jeder Stelle des Prozesses.
        DuringAny(
            When(ApplicationCreationFailed)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = ctx.Message.Reason;
                    ctx.Saga.CompletedAt = DateTime.UtcNow;
                })
                .Publish(ctx => new MarkApplicationFailed(
                    ctx.Saga.CorrelationId,
                    ctx.Saga.ApplicationId,
                    ctx.Message.Reason))
                .TransitionTo(Failed)
                .Finalize());

        // Konfiguriert, dass die Saga als "Completed" markiert wird, wenn sie finalisiert wurde.
        // Dies ermöglicht es, abgeschlossene Sagas automatisch zu bereinigen.
        SetCompletedWhenFinalized();
    }
}