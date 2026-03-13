using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Domain.Aggregates.ScoringConfigAggregate;

namespace RiskManagement.Application.Queries;

public record GetScoringConfigQuery : IQuery<ScoringConfigResponse>;

public class GetScoringConfigHandler : IQueryHandler<GetScoringConfigQuery, ScoringConfigResponse>
{
    private readonly IScoringConfigRepository _repository;

    public GetScoringConfigHandler(IScoringConfigRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ScoringConfigResponse>> HandleAsync(GetScoringConfigQuery query,
        CancellationToken ct = default)
    {
        var configVersion = await _repository.GetCurrentAsync(ct);
        if (configVersion is null)
            return Result<ScoringConfigResponse>.NotFound("Keine Scoring-Konfiguration gefunden");

        return Result<ScoringConfigResponse>.Success(ScoringConfigMapper.ToResponse(configVersion));
    }
}
