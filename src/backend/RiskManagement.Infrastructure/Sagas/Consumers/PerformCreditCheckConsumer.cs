using MassTransit;
using RiskManagement.Application.Sagas.ApplicationCreation.Events;
using RiskManagement.Domain.Services;

namespace RiskManagement.Infrastructure.Sagas.Consumers;

public class PerformCreditCheckConsumer : IConsumer<PerformCreditCheck>
{
    private readonly ICreditCheckService _creditCheckService;

    public PerformCreditCheckConsumer(ICreditCheckService creditCheckService)
    {
        _creditCheckService = creditCheckService;
    }

    public async Task Consume(ConsumeContext<PerformCreditCheck> context)
    {
        var result = await _creditCheckService.CheckAsync(
            context.Message.FirstName,
            context.Message.LastName,
            context.Message.DateOfBirth,
            context.Message.Street,
            context.Message.City,
            context.Message.ZipCode,
            context.Message.Country);

        await context.Publish(new CreditCheckCompleted(
            context.Message.CorrelationId,
            result.HasPaymentDefault,
            result.CreditScore,
            result.CheckedAt,
            result.Provider));
    }
}