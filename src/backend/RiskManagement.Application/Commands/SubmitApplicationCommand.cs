using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.Services;

namespace RiskManagement.Application.Commands;

public record SubmitApplicationCommand(int ApplicationId, string UserEmail) : ICommand<ApplicationResponse>;

public class SubmitApplicationHandler : ICommandHandler<SubmitApplicationCommand, ApplicationResponse>
{
    private readonly IApplicationRepository _repository;
    private readonly ScoringService _scoringService;
    private readonly IDispatcher _dispatcher;

    public SubmitApplicationHandler(IApplicationRepository repository, ScoringService scoringService, IDispatcher dispatcher)
    {
        _repository = repository;
        _scoringService = scoringService;
        _dispatcher = dispatcher;
    }

    public async Task<Result<ApplicationResponse>> HandleAsync(SubmitApplicationCommand command, CancellationToken ct = default)
    {
        var application = await _repository.GetByIdAsync(command.ApplicationId, ct);
        if (application is null)
            return Result<ApplicationResponse>.NotFound("Antrag nicht gefunden");

        if (application.CreatedBy != command.UserEmail)
            return Result<ApplicationResponse>.Forbidden("Zugriff verweigert");

        application.Submit(_scoringService);
        await _repository.SaveChangesAsync(ct);

        await _dispatcher.PublishDomainEventsAsync(application, ct);

        return Result<ApplicationResponse>.Success(ApplicationMapper.ToResponse(application));
    }
}
