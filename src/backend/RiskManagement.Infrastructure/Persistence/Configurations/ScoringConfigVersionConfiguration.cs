using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RiskManagement.Domain.Aggregates.ScoringConfigAggregate;
using RiskManagement.Domain.ValueObjects;

namespace RiskManagement.Infrastructure.Persistence.Configurations;

public class ScoringConfigVersionConfiguration : IEntityTypeConfiguration<ScoringConfigVersion>
{
    public void Configure(EntityTypeBuilder<ScoringConfigVersion> entity)
    {
        entity.ToTable("scoring_config_versions");

        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id)
            .HasColumnName("id")
            .UseHiLo("scoring_config_version_id_seq")
            .HasConversion(
                v => v.Value,
                v => new ScoringConfigVersionId(v));

        entity.Property(e => e.Version).HasColumnName("version").IsRequired();
        entity.HasIndex(e => e.Version).IsUnique();

        entity.OwnsOne(e => e.Config, config =>
        {
            config.Property(c => c.GreenThreshold).HasColumnName("green_threshold").IsRequired();
            config.Property(c => c.YellowThreshold).HasColumnName("yellow_threshold").IsRequired();

            config.Property(c => c.IncomeRatioGood).HasColumnName("income_ratio_good").HasColumnType("numeric(4,2)")
                .IsRequired();
            config.Property(c => c.IncomeRatioModerate).HasColumnName("income_ratio_moderate")
                .HasColumnType("numeric(4,2)").IsRequired();
            config.Property(c => c.IncomeRatioLimited).HasColumnName("income_ratio_limited")
                .HasColumnType("numeric(4,2)").IsRequired();
            config.Property(c => c.PenaltyModerateRatio).HasColumnName("penalty_moderate_ratio").IsRequired();
            config.Property(c => c.PenaltyLimitedRatio).HasColumnName("penalty_limited_ratio").IsRequired();
            config.Property(c => c.PenaltyCriticalRatio).HasColumnName("penalty_critical_ratio").IsRequired();

            config.Property(c => c.RateGood).HasColumnName("rate_good").HasColumnType("numeric(4,2)").IsRequired();
            config.Property(c => c.RateModerate).HasColumnName("rate_moderate").HasColumnType("numeric(4,2)")
                .IsRequired();
            config.Property(c => c.RateHeavy).HasColumnName("rate_heavy").HasColumnType("numeric(4,2)").IsRequired();
            config.Property(c => c.PenaltyModerateRate).HasColumnName("penalty_moderate_rate").IsRequired();
            config.Property(c => c.PenaltyHeavyRate).HasColumnName("penalty_heavy_rate").IsRequired();
            config.Property(c => c.PenaltyExcessiveRate).HasColumnName("penalty_excessive_rate").IsRequired();

            config.Property(c => c.PenaltySelfEmployed).HasColumnName("penalty_self_employed").IsRequired();
            config.Property(c => c.PenaltyRetired).HasColumnName("penalty_retired").IsRequired();
            config.Property(c => c.PenaltyUnemployed).HasColumnName("penalty_unemployed").IsRequired();
            config.Property(c => c.PenaltyPaymentDefault).HasColumnName("penalty_payment_default").IsRequired();
        });

        entity.Property(e => e.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired()
            .HasConversion(
                v => v.Value,
                v => EmailAddress.Create(v));

        entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
    }
}