using MassTransit;
using RiskManagement.Application.Sagas.ApplicationCreation.Events;

namespace RiskManagement.Infrastructure.Sagas.Consumers;

/// <summary>
/// Consumer für Fault-Events wenn FetchCustomerProfile fehlschlägt.
/// 
/// MassTransit publiziert ein Fault&lt;T&gt; Event, wenn ein Consumer nach allen Retries fehlschlägt.
/// Dieser Consumer fängt das Fault-Event ab und publiziert ApplicationCreationFailed,
/// um die Saga sauber zu beenden.
/// </summary>
public class FetchCustomerProfileFaultConsumer : IConsumer<Fault<FetchCustomerProfile>>
{
    public async Task Consume(ConsumeContext<Fault<FetchCustomerProfile>> context)
    {
        var correlationId = context.Message.Message.CorrelationId;

        // Extrahiere die Exception-Information für den Fehlergrund
        var exceptions = context.Message.Exceptions;
        var reason = exceptions.Length > 0
            ? $"Customer profile fetch failed: {exceptions[0].Message}"
            : "Customer profile fetch failed with unknown error";

        await context.Publish(new ApplicationCreationFailed(correlationId, reason));
    }
}