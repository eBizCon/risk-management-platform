using MassTransit;
using RiskManagement.Application.Sagas.ApplicationCreation.Events;
using RiskManagement.Application.Services;

namespace RiskManagement.Infrastructure.Sagas.Consumers;

public class FetchCustomerProfileConsumer : IConsumer<FetchCustomerProfile>
{
    private readonly ICustomerProfileService _customerProfileService;

    public FetchCustomerProfileConsumer(ICustomerProfileService customerProfileService)
    {
        _customerProfileService = customerProfileService;
    }

    public async Task Consume(ConsumeContext<FetchCustomerProfile> context)
    {
        var profile = await _customerProfileService.GetCustomerProfileAsync(
            context.Message.CustomerId, context.CancellationToken);

        if (profile is null)
        {
            await context.Publish(new ApplicationCreationFailed(
                context.Message.CorrelationId,
                "Kunde nicht gefunden"));
            return;
        }

        await context.Publish(new CustomerProfileFetched(
            context.Message.CorrelationId,
            profile.FirstName,
            profile.LastName,
            profile.EmploymentStatus,
            profile.DateOfBirth,
            profile.Address.Street,
            profile.Address.City,
            profile.Address.ZipCode,
            profile.Address.Country));
    }
}