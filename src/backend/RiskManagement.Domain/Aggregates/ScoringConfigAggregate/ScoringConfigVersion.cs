using RiskManagement.Domain.Common;
using RiskManagement.Domain.Exceptions;
using RiskManagement.Domain.ValueObjects;

namespace RiskManagement.Domain.Aggregates.ScoringConfigAggregate;

public class ScoringConfigVersion : Entity<ScoringConfigVersionId>
{
    public int Version { get; private set; }
    public ScoringConfig Config { get; private set; } = null!;
    public EmailAddress CreatedBy { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }

    private ScoringConfigVersion() { }

    public static ScoringConfigVersion Create(
        int version,
        ScoringConfig config,
        EmailAddress createdBy)
    {
        if (version < 1)
            throw new DomainException("Version muss mindestens 1 sein");

        return new ScoringConfigVersion
        {
            Version = version,
            Config = config,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
    }
}
