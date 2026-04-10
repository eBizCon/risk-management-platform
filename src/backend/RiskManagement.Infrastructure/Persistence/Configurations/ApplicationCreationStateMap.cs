using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RiskManagement.Application.Sagas.ApplicationCreation;

namespace RiskManagement.Infrastructure.Persistence.Configurations;

public class ApplicationCreationStateConfiguration : IEntityTypeConfiguration<ApplicationCreationState>
{
    public void Configure(EntityTypeBuilder<ApplicationCreationState> entity)
    {
        entity.ToTable("saga_application_creation_state");

        entity.HasKey(x => x.CorrelationId);
        entity.Property(x => x.CorrelationId).HasColumnName("correlation_id");
        entity.Property(x => x.CurrentState).HasColumnName("current_state").HasMaxLength(64).IsRequired();

        entity.Property(x => x.ApplicationId).HasColumnName("application_id");
        entity.Property(x => x.CustomerId).HasColumnName("customer_id");
        entity.Property(x => x.Income).HasColumnName("income");
        entity.Property(x => x.FixedCosts).HasColumnName("fixed_costs");
        entity.Property(x => x.DesiredRate).HasColumnName("desired_rate");
        entity.Property(x => x.LoanAmount).HasColumnName("loan_amount");
        entity.Property(x => x.LoanTerm).HasColumnName("loan_term");
        entity.Property(x => x.UserEmail).HasColumnName("user_email").HasMaxLength(256);
        entity.Property(x => x.AutoSubmit).HasColumnName("auto_submit");

        entity.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(100);
        entity.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(100);
        entity.Property(x => x.EmploymentStatus).HasColumnName("employment_status").HasMaxLength(30);
        entity.Property(x => x.DateOfBirth).HasColumnName("date_of_birth").HasMaxLength(20);
        entity.Property(x => x.Street).HasColumnName("street").HasMaxLength(200);
        entity.Property(x => x.City).HasColumnName("city").HasMaxLength(100);
        entity.Property(x => x.ZipCode).HasColumnName("zip_code").HasMaxLength(20);
        entity.Property(x => x.Country).HasColumnName("country").HasMaxLength(100);

        entity.Property(x => x.HasPaymentDefault).HasColumnName("has_payment_default");
        entity.Property(x => x.CreditScore).HasColumnName("credit_score");
        entity.Property(x => x.CreditCheckedAt).HasColumnName("credit_checked_at");
        entity.Property(x => x.CreditProvider).HasColumnName("credit_provider").HasMaxLength(50);

        entity.Property(x => x.FailureReason).HasColumnName("failure_reason").HasMaxLength(500);
        entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        entity.Property(x => x.CompletedAt).HasColumnName("completed_at");
    }
}
