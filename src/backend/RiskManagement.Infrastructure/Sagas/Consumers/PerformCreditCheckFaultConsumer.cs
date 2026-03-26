using MassTransit;
using RiskManagement.Application.Sagas.ApplicationCreation;
using RiskManagement.Application.Sagas.ApplicationCreation.Events;

namespace RiskManagement.Infrastructure.Sagas.Consumers;

/// <summary>
/// Consumer für Fault-Events wenn PerformCreditCheck fehlschlägt.
/// 
/// MassTransit publiziert ein Fault&lt;T&gt; Event, wenn ein Consumer nach allen Retries fehlschlägt.
/// Dieser Consumer fängt das Fault-Event ab und publiziert ApplicationCreationFailed,
/// um die Saga sauber zu beenden.
/// </summary>
public class PerformCreditCheckFaultConsumer : IConsumer<Fault<PerformCreditCheck>>
{
    public async Task Consume(ConsumeContext<Fault<PerformCreditCheck>> context)
    {
        var correlationId = context.Message.Message.CorrelationId;

        // Extrahiere die Exception-Information für den Fehlergrund
        var exceptions = context.Message.Exceptions;
        var reason = exceptions.Length > 0
            ? $"Credit check failed: {exceptions[0].Message}"
            : "Credit check failed with unknown error";

        await context.Publish(new ApplicationCreationFailed(correlationId, reason));
    }
}