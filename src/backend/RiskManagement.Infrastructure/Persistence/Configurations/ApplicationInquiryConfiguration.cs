using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;

namespace RiskManagement.Infrastructure.Persistence.Configurations;

public class ApplicationInquiryConfiguration : IEntityTypeConfiguration<ApplicationInquiry>
{
    public void Configure(EntityTypeBuilder<ApplicationInquiry> entity)
    {
        entity.ToTable("application_inquiries");

        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        entity.Property(e => e.ApplicationId).HasColumnName("application_id").IsRequired();
        entity.Property(e => e.InquiryText).HasColumnName("inquiry_text").IsRequired();
        entity.Property(e => e.Status).HasColumnName("status").IsRequired().HasDefaultValue("open");
        entity.Property(e => e.ProcessorEmail).HasColumnName("processor_email").IsRequired();
        entity.Property(e => e.ResponseText).HasColumnName("response_text");
        entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        entity.Property(e => e.RespondedAt).HasColumnName("responded_at");
    }
}