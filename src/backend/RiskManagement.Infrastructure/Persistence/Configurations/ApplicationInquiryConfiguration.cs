using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.ValueObjects;
using AppId = RiskManagement.Domain.Aggregates.ApplicationAggregate.ApplicationId;

namespace RiskManagement.Infrastructure.Persistence.Configurations;

public class ApplicationInquiryConfiguration : IEntityTypeConfiguration<ApplicationInquiry>
{
    public void Configure(EntityTypeBuilder<ApplicationInquiry> entity)
    {
        entity.ToTable("application_inquiries");

        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id)
            .HasColumnName("id")
            .UseHiLo("inquiry_id_seq")
            .HasConversion(
                v => v.Value,
                v => new InquiryId(v));

        entity.Property(e => e.ApplicationId)
            .HasColumnName("application_id")
            .IsRequired()
            .HasConversion(
                v => v.Value,
                v => new AppId(v));

        entity.Property(e => e.InquiryText).HasColumnName("inquiry_text").IsRequired();

        entity.Property(e => e.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasDefaultValue(InquiryStatus.Open)
            .HasConversion(
                v => v.Value,
                v => InquiryStatus.From(v));

        entity.Property(e => e.ProcessorEmail)
            .HasColumnName("processor_email")
            .IsRequired()
            .HasConversion(
                v => v.Value,
                v => EmailAddress.Create(v));

        entity.Property(e => e.ResponseText).HasColumnName("response_text");
        entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        entity.Property(e => e.AnsweredAt).HasColumnName("responded_at");
    }
}
