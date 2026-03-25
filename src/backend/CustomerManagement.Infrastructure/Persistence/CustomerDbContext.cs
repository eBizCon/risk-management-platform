using CustomerManagement.Domain.Aggregates.CustomerAggregate;
using CustomerManagement.Domain.ValueObjects;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SharedKernel.ValueObjects;

namespace CustomerManagement.Infrastructure.Persistence;

public class CustomerDbContext : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();

    public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("customer");

        modelBuilder.HasSequence<int>("customer_id_seq", "customer").StartsAt(1000).IncrementsBy(10);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("customers");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasConversion(id => id.Value, value => new CustomerId(value))
                .UseHiLo("customer_id_seq", "customer");

            entity.Property(e => e.FirstName).HasColumnName("first_name").HasMaxLength(50).IsRequired();
            entity.Property(e => e.LastName).HasColumnName("last_name").HasMaxLength(50).IsRequired();

            entity.Property(e => e.Email)
                .HasColumnName("email")
                .HasConversion(
                    e => e != null ? e.Value : null,
                    v => v != null ? EmailAddress.Create(v) : null)
                .HasMaxLength(200);

            entity.Property(e => e.Phone)
                .HasColumnName("phone")
                .HasConversion(p => p.Value, v => PhoneNumber.Create(v))
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth").IsRequired();

            entity.OwnsOne(e => e.Address, address =>
            {
                address.Property(a => a.Street).HasColumnName("street").HasMaxLength(200).IsRequired();
                address.Property(a => a.City).HasColumnName("city").HasMaxLength(100).IsRequired();
                address.Property(a => a.ZipCode).HasColumnName("zip_code").HasMaxLength(20).IsRequired();
                address.Property(a => a.Country).HasColumnName("country").HasMaxLength(100).IsRequired();
            });

            entity.Property(e => e.EmploymentStatus)
                .HasColumnName("employment_status")
                .HasConversion(s => s.Value, v => EmploymentStatus.From(v))
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion(s => s.Value, v => CustomerStatus.From(v))
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.CreatedBy)
                .HasColumnName("created_by")
                .HasConversion(e => e.Value, v => EmailAddress.Create(v))
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.Ignore(e => e.DomainEvents);
        });
    }
}
