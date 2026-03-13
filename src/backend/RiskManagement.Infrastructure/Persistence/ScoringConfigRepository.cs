using Microsoft.EntityFrameworkCore;
using RiskManagement.Domain.Aggregates.ScoringConfigAggregate;

namespace RiskManagement.Infrastructure.Persistence;

public class ScoringConfigRepository : IScoringConfigRepository
{
    private readonly ApplicationDbContext _context;

    public ScoringConfigRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ScoringConfigVersion?> GetCurrentAsync(CancellationToken ct = default)
    {
        return await _context.ScoringConfigVersions
            .OrderByDescending(c => c.Version)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<ScoringConfigVersion?> GetByIdAsync(ScoringConfigVersionId id, CancellationToken ct = default)
    {
        return await _context.ScoringConfigVersions
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task AddAsync(ScoringConfigVersion configVersion, CancellationToken ct = default)
    {
        await _context.ScoringConfigVersions.AddAsync(configVersion, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
