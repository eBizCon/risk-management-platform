# Domain Policies & Pipeline Behaviors

## Kontext

Wir haben entschieden, Domain Events **nach dem Save** zu dispatchen (After-Save). Das passt semantisch: Events beschreiben Fakten in der Vergangenheit (`ApplicationSubmittedEvent` = "wurde submitted"). Side-Effects wie Emails oder Audit-Logs passieren nach dem Commit.

Aber was, wenn wir **transaktionale Prozess-Schritte** brauchen, die **vor dem Save** laufen müssen und Teil derselben Transaktion sein sollen?

Dafür gibt es zwei separate Konzepte: **Domain Policies** und **Pipeline Behaviors**.

---

## Domain Policies

### Was ist eine Domain Policy?

Eine Policy reagiert auf ein Domain Event und löst einen **synchronen Folge-Command** innerhalb derselben Transaktion aus. Sie beschreibt eine Geschäftsregel der Form: *"Wenn X passiert, dann muss Y passieren."*

### Abgrenzung zu Domain Events

```
Domain Event (After-Save)               Domain Policy (Before-Save)
───────────────────────                  ──────────────────────────
Past Tense: "ist passiert"              Regel: "wenn X, dann Y"
Fire-and-forget                          Transaktional (Rollback möglich)
0..N Handler                             Genau 1 Policy pro Regel
Side-Effects (Email, Log, ...)           Zustandsänderungen im selben Aggregate oder anderen Aggregates
Eventual Consistency ok                  Strong Consistency erforderlich
Fehler = Log & Retry                     Fehler = Gesamte Transaktion schlägt fehl
```

### Beispiel

```
Regel: "Wenn ein Antrag genehmigt wird und der Antragsteller 
        mehr als 3 genehmigte Anträge hat, wird er automatisch 
        als Premium-Kunde markiert."

┌──────────────┐     ┌─────────────────────────┐     ┌─────────────────┐
│ Approve()    │────▶│ PromoteToPremuimPolicy   │────▶│ Customer.       │
│ on Application│     │                         │     │ MarkAsPremium() │
└──────────────┘     └─────────────────────────┘     └─────────────────┘
                      Läuft VOR SaveChanges
                      Gleiche Transaktion
                      Rollback wenn Fehler
```

### Interface-Entwurf

```csharp
public interface IDomainPolicy<in TEvent> where TEvent : IDomainEvent
{
    Task ExecuteAsync(TEvent domainEvent, CancellationToken ct = default);
}
```

### Ablauf im Handler (mit Policies + Events)

```
Handler:
  1. Business Logic (Aggregate-Methoden aufrufen)
  2. Policies ausführen (transaktional, vor Save)
  3. SaveChangesAsync()
  4. Domain Events publishen (nach Save, fire-and-forget)
  5. ClearDomainEvents()
```

### Wann braucht man das?

- Aggregate-übergreifende Geschäftsregeln innerhalb einer Transaktion
- Invarianten, die über ein einzelnes Aggregate hinausgehen
- Prozess-Schritte, bei denen "alles oder nichts" gelten muss

### Wann braucht man das NICHT?

- Notifications (Email, Push) → Domain Events
- Logging / Audit → Domain Events
- Externe API-Calls → Domain Events + Outbox Pattern
- Regeln innerhalb eines Aggregates → direkt im Aggregate

---

## Pipeline Behaviors

### Was ist ein Pipeline Behavior?

Ein Behavior ist ein **Decorator/Middleware** um die Handler-Ausführung herum. Es ist kein fachliches Konzept, sondern eine **technische Cross-Cutting Concern**.

```
Request ──▶ [Behavior 1] ──▶ [Behavior 2] ──▶ [Handler] ──▶ Response
             Validation        Logging          Business
                               Transaction      Logic
```

### Interface-Entwurf

```csharp
public interface IPipelineBehavior<in TRequest, TResult>
{
    Task<Result<TResult>> HandleAsync(
        TRequest request,
        Func<Task<Result<TResult>>> next,
        CancellationToken ct = default);
}
```

### Typische Behaviors

| Behavior | Zweck |
|----------|-------|
| `ValidationBehavior` | Validiert Commands/Queries bevor der Handler läuft |
| `LoggingBehavior` | Loggt Request/Response und Dauer |
| `TransactionBehavior` | Wraps Handler in DB-Transaktion |
| `AuthorizationBehavior` | Prüft Berechtigungen vor Handler-Ausführung |

### Beispiel: ValidationBehavior

```csharp
public class ValidationBehavior<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
{
    private readonly IValidator<TRequest>? _validator;

    public async Task<Result<TResult>> HandleAsync(
        TRequest request, Func<Task<Result<TResult>>> next, CancellationToken ct)
    {
        if (_validator is not null)
        {
            var validationResult = await _validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
                return Result<TResult>.ValidationError(validationResult.Errors);
        }

        return await next();
    }
}
```

### Wann braucht man das?

- Wenn sich dasselbe Pattern in vielen Handlern wiederholt (Validation, Try/Catch, Logging)
- Wenn man Cross-Cutting Concerns zentral statt in jedem Handler implementieren will

### Wann braucht man das NICHT?

- Wenn die Handler übersichtlich und wenig repetitiv sind
- Wenn man explizites Verhalten im Handler bevorzugt (wie bei uns aktuell)

---

## Zusammenfassung: Drei Ebenen

```
┌──────────────────────────────────────────────────────────────────┐
│                     Gesamtarchitektur                             │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Pipeline Behaviors    (technisch, um den Handler herum)         │
│  ┌────────────────────────────────────────────────────────────┐  │
│  │  Validation → Logging → Transaction →                      │  │
│  │  ┌──────────────────────────────────────────────────────┐  │  │
│  │  │  Handler                                              │  │  │
│  │  │    1. Business Logic                                  │  │  │
│  │  │    2. Domain Policies  (fachlich, vor Save)           │  │  │
│  │  │    3. SaveChanges                                     │  │  │
│  │  │    4. Domain Events    (fachlich, nach Save)          │  │  │
│  │  └──────────────────────────────────────────────────────┘  │  │
│  └────────────────────────────────────────────────────────────┘  │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘
```

## Status

**Aktuell nicht benötigt.** Dieses Dokument dient als Referenz, falls wir eines der Konzepte in Zukunft einführen wollen. Der aktuelle Dispatcher-Ansatz (Commands, Queries, Domain Events) deckt unsere Anforderungen ab.
