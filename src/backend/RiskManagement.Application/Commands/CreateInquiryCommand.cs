using RiskManagement.Application.Common;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.ValueObjects;
using AppId = RiskManagement.Domain.Aggregates.ApplicationAggregate.ApplicationId;

namespace RiskManagement.Application.Commands;

public record CreateInquiryCommand(int ApplicationId, string InquiryText, string ProcessorEmail) : ICommand<object>;

public class CreateInquiryHandler : ICommandHandler<CreateInquiryCommand, object>
{
    private readonly IApplicationRepository _repository;

    public CreateInquiryHandler(IApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<object>> HandleAsync(CreateInquiryCommand command, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.InquiryText))
            return Result<object>.Failure("Rückfragetext darf nicht leer sein");

        var application = await _repository.GetByIdAsync(new AppId(command.ApplicationId), ct);
        if (application is null)
            return Result<object>.NotFound("Antrag nicht gefunden");

        application.RequestInformation(command.InquiryText, EmailAddress.Create(command.ProcessorEmail));
        await _repository.SaveChangesAsync(ct);

        var inquiry = application.Inquiries.Last();
        return Result<object>.Success(new { inquiry });
    }
}
