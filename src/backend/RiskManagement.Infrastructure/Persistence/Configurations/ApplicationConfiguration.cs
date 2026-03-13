using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.Aggregates.ScoringConfigAggregate;
using RiskManagement.Domain.ValueObjects;
using ApplicationEntity = RiskManagement.Domain.Aggregates.ApplicationAggregate.Application;
using AppId = RiskManagement.Domain.Aggregates.ApplicationAggregate.ApplicationId;

namespace RiskManagement.Infrastructure.Persistence.Configurations;

public class ApplicationConfiguration : IEntityTypeConfiguration<ApplicationEntity>
{
    public void Configure(EntityTypeBuilder<ApplicationEntity> entity)
    {
        entity.ToTable("applications");

        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd()
            .HasConversion(
                v => v.Value,
                v => new AppId(v));

        entity.Property(e => e.CustomerId).HasColumnName("customer_id").IsRequired();

        entity.Property(e => e.Income)
            .HasColumnName("income")
            .IsRequired()
            .HasConversion(
                v => (double)v.Amount,
                v => Money.Create((decimal)v));

        entity.Property(e => e.FixedCosts)
            .HasColumnName("fixed_costs")
            .IsRequired()
            .HasConversion(
                v => (double)v.Amount,
                v => Money.Create((decimal)v));

        entity.Property(e => e.DesiredRate)
            .HasColumnName("desired_rate")
            .IsRequired()
            .HasConversion(
                v => (double)v.Amount,
                v => Money.Create((decimal)v));

        entity.Property(e => e.EmploymentStatus)
            .HasColumnName("employment_status")
            .IsRequired()
            .HasConversion(
                v => v.Value,
                v => EmploymentStatus.From(v));

        entity.Property(e => e.HasPaymentDefault).HasColumnName("has_payment_default").IsRequired();

        entity.Property(e => e.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasDefaultValue(ApplicationStatus.Draft)
            .HasConversion(
                v => v.Value,
                v => ApplicationStatus.From(v));

        entity.Property(e => e.Score).HasColumnName("score");

        entity.Property(e => e.TrafficLight)
            .HasColumnName("traffic_light")
            .HasConversion(
                v => v != null ? v.Value : null,
                v => v != null ? TrafficLight.From(v) : null);

        entity.Property(e => e.ScoringReasons).HasColumnName("scoring_reasons");

        entity.Property(e => e.ScoringConfigVersionId)
            .HasColumnName("scoring_config_version_id")
            .HasConversion(
                v => v != null ? v.Value.Value : (int?)null,
                v => v != null ? new ScoringConfigVersionId(v.Value) : null);

        entity.HasOne<ScoringConfigVersion>()
            .WithMany()
            .HasForeignKey(e => e.ScoringConfigVersionId)
            .IsRequired(false);

        entity.Property(e => e.ProcessorComment).HasColumnName("processor_comment");
        entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        entity.Property(e => e.SubmittedAt).HasColumnName("submitted_at");
        entity.Property(e => e.ProcessedAt).HasColumnName("processed_at");

        entity.Property(e => e.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired()
            .HasConversion(
                v => v.Value,
                v => EmailAddress.Create(v));

        entity.HasMany(e => e.Inquiries)
            .WithOne()
            .HasForeignKey(e => e.ApplicationId);

        entity.Navigation(e => e.Inquiries)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}