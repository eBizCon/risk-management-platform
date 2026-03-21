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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}