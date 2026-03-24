using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.Aggregates.ScoringConfigAggregate;
using RiskManagement.Domain.Services;

namespace RiskManagement.Application.Commands;

public record RescoreOpenApplicationsCommand : ICommand<RescoreResult>;

public class RescoreOpenApplicationsHandler : ICommandHandler<RescoreOpenApplicationsCommand, RescoreResult>
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IScoringConfigRepository _configRepository;
    private readonly IScoringService _scoringService;

    public RescoreOpenApplicationsHandler(
        IApplicationRepository applicationRepository,
        IScoringConfigRepository configRepository,
        IScoringService scoringService)
    {
        _applicationRepository = applicationRepository;
        _configRepository = configRepository;
        _scoringService = scoringService;
    }

    public async Task<Result<RescoreResult>> HandleAsync(RescoreOpenApplicationsCommand command,
        CancellationToken ct = default)
    {
        var configVersion = await _configRepository.GetCurrentAsync(ct);
        if (configVersion is null)
            return Result<RescoreResult>.Failure("Keine Scoring-Konfiguration gefunden");

        var openApplications = await _applicationRepository.GetOpenApplicationsAsync(ct);

        foreach (var application in openApplications)
            application.Rescore(_scoringService, configVersion.Config, configVersion.Id);

        await _applicationRepository.SaveChangesAsync(ct);

        return Result<RescoreResult>.Success(new RescoreResult(openApplications.Count));
    }
}