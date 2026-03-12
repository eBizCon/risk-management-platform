using Microsoft.EntityFrameworkCore;
using RiskManagement.Api.Models;

namespace RiskManagement.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Application> Applications { get; set; } = null!;
    public DbSet<ApplicationInquiry> ApplicationInquiries { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Application>(entity =>
        {
            entity.ToTable("applications");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.Income).HasColumnName("income").IsRequired();
            entity.Property(e => e.FixedCosts).HasColumnName("fixed_costs").IsRequired();
            entity.Property(e => e.DesiredRate).HasColumnName("desired_rate").IsRequired();
            entity.Property(e => e.EmploymentStatus).HasColumnName("employment_status").IsRequired();
            entity.Property(e => e.HasPaymentDefault).HasColumnName("has_payment_default").IsRequired();
            entity.Property(e => e.Status).HasColumnName("status").IsRequired().HasDefaultValue("draft");
            entity.Property(e => e.Score).HasColumnName("score");
            entity.Property(e => e.TrafficLight).HasColumnName("traffic_light");
            entity.Property(e => e.ScoringReasons).HasColumnName("scoring_reasons");
            entity.Property(e => e.ProcessorComment).HasColumnName("processor_comment");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.SubmittedAt).HasColumnName("submitted_at");
            entity.Property(e => e.ProcessedAt).HasColumnName("processed_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").IsRequired();
        });

        modelBuilder.Entity<ApplicationInquiry>(entity =>
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
        });
    }
}
