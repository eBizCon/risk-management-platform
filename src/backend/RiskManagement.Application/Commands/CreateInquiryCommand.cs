using RiskManagement.Application.Common;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;

namespace RiskManagement.Application.Commands;

public record CreateInquiryCommand(int ApplicationId, string InquiryText, string ProcessorEmail) : ICommand<object>;

public class CreateInquiryHandler : ICommandHandler<CreateInquiryCommand, object>
{
    private readonly IApplicationRepository _repository;
    private readonly IDispatcher _dispatcher;

    public CreateInquiryHandler(IApplicationRepository repository, IDispatcher dispatcher)
    {
        _repository = repository;
        _dispatcher = dispatcher;
    }

    public async Task<Result<object>> HandleAsync(CreateInquiryCommand command, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.InquiryText))
            return Result<object>.Failure("Rückfragetext darf nicht leer sein");

        var application = await _repository.GetByIdAsync(command.ApplicationId, ct);
        if (application is null)
            return Result<object>.NotFound("Antrag nicht gefunden");

        application.RequestInformation(command.InquiryText, command.ProcessorEmail);
        await _repository.SaveChangesAsync(ct);

        await _dispatcher.PublishDomainEventsAsync(application, ct);

        var inquiry = application.Inquiries.Last();
        return Result<object>.Success(new { inquiry });
    }
}
