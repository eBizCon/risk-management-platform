using MassTransit;
using RiskManagement.Application.Sagas.ApplicationCreation.Events;

namespace RiskManagement.Infrastructure.Sagas.Consumers;

public class FinalizeApplicationUpdateFaultConsumer : IConsumer<Fault<FinalizeApplicationUpdate>>
{
    public async Task Consume(ConsumeContext<Fault<FinalizeApplicationUpdate>> context)
    {
        var correlationId = context.Message.Message.CorrelationId;

        var exceptions = context.Message.Exceptions;
        var reason = exceptions.Length > 0
            ? $"Application update finalization failed: {exceptions[0].Message}"
            : "Application update finalization failed with unknown error";

        await context.Publish(new ApplicationCreationFailed(correlationId, reason));
    }
}