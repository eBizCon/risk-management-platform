using MassTransit;
using Microsoft.EntityFrameworkCore;
using RiskManagement.Application.Sagas.ApplicationCreation;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.Aggregates.ScoringConfigAggregate;
using RiskManagement.Domain.ReadModels;
using ApplicationEntity = RiskManagement.Domain.Aggregates.ApplicationAggregate.Application;

namespace RiskManagement.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<ApplicationEntity> Applications { get; set; } = null!;
    public DbSet<ApplicationInquiry> ApplicationInquiries { get; set; } = null!;
    public DbSet<ScoringConfigVersion> ScoringConfigVersions { get; set; } = null!;
    public DbSet<ApplicationCreationState> ApplicationCreationStates { get; set; } = null!;
    public DbSet<CustomerReadModel> CustomerReadModels { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasSequence<int>("application_id_seq").StartsAt(1000).IncrementsBy(10);
        modelBuilder.HasSequence<int>("inquiry_id_seq").StartsAt(1000).IncrementsBy(10);
        modelBuilder.HasSequence<int>("scoring_config_version_id_seq").StartsAt(100).IncrementsBy(10);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
