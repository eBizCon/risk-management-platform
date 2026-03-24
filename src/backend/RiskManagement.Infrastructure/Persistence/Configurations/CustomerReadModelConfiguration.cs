using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RiskManagement.Domain.ReadModels;

namespace RiskManagement.Infrastructure.Persistence.Configurations;

public class CustomerReadModelConfiguration : IEntityTypeConfiguration<CustomerReadModel>
{
    public void Configure(EntityTypeBuilder<CustomerReadModel> builder)
    {
        builder.ToTable("customer_read_models");

        builder.HasKey(e => e.CustomerId);

        builder.Property(e => e.CustomerId)
            .HasColumnName("customer_id")
            .ValueGeneratedNever();

        builder.Property(e => e.FirstName)
            .HasColumnName("first_name")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.LastName)
            .HasColumnName("last_name")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.LastUpdatedAt)
            .HasColumnName("last_updated_at")
            .IsRequired();
    }
}