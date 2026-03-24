namespace RiskManagement.Domain.Aggregates.ScoringConfigAggregate;

public interface IScoringConfigRepository
{
    Task<ScoringConfigVersion?> GetCurrentAsync(CancellationToken ct = default);
    Task<ScoringConfigVersion?> GetByIdAsync(ScoringConfigVersionId id, CancellationToken ct = default);
    Task AddAsync(ScoringConfigVersion configVersion, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}