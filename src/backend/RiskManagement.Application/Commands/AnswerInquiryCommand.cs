using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;

namespace RiskManagement.Application.Commands;

public record AnswerInquiryCommand(int ApplicationId, string ResponseText, string UserEmail)
    : ICommand<ApplicationResponse>;

public class AnswerInquiryHandler : ICommandHandler<AnswerInquiryCommand, ApplicationResponse>
{
    private readonly IApplicationRepository _repository;

    public AnswerInquiryHandler(IApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ApplicationResponse>> HandleAsync(AnswerInquiryCommand command,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.ResponseText))
            return Result<ApplicationResponse>.Failure("Antworttext darf nicht leer sein");

        var application = await _repository.GetByIdAsync(command.ApplicationId, ct);
        if (application is null)
            return Result<ApplicationResponse>.NotFound("Antrag nicht gefunden");

        if (application.CreatedBy != command.UserEmail)
            return Result<ApplicationResponse>.Forbidden("Zugriff verweigert");

        application.AnswerInquiry(command.ResponseText);
        await _repository.SaveChangesAsync(ct);

        return Result<ApplicationResponse>.Success(ApplicationMapper.ToResponse(application));
    }
}