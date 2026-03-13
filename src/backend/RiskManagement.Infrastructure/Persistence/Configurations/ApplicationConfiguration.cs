using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.ValueObjects;
using ApplicationEntity = RiskManagement.Domain.Aggregates.ApplicationAggregate.Application;

namespace RiskManagement.Infrastructure.Persistence.Configurations;

public class ApplicationConfiguration : IEntityTypeConfiguration<ApplicationEntity>
{
    public void Configure(EntityTypeBuilder<ApplicationEntity> entity)
    {
        entity.ToTable("applications");

        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        entity.Property(e => e.Name).HasColumnName("name").IsRequired();
        entity.Property(e => e.Income).HasColumnName("income").IsRequired();
        entity.Property(e => e.FixedCosts).HasColumnName("fixed_costs").IsRequired();
        entity.Property(e => e.DesiredRate).HasColumnName("desired_rate").IsRequired();

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
        entity.Property(e => e.ProcessorComment).HasColumnName("processor_comment");
        entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        entity.Property(e => e.SubmittedAt).HasColumnName("submitted_at");
        entity.Property(e => e.ProcessedAt).HasColumnName("processed_at");
        entity.Property(e => e.CreatedBy).HasColumnName("created_by").IsRequired();

        entity.HasMany(e => e.Inquiries)
            .WithOne()
            .HasForeignKey(e => e.ApplicationId);

        entity.Navigation(e => e.Inquiries)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}