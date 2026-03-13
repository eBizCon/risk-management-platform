using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RiskManagement.Application.Commands;
using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Application.Queries;
using RiskManagement.Application.Validation;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.Services;
using RiskManagement.Infrastructure.Persistence;
using RiskManagement.Infrastructure.Seeding;

namespace RiskManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<DatabaseSeeder>();

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<ScoringService>();

        services.AddScoped<IValidator<ApplicationCreateDto>, ApplicationValidator>();
        services.AddScoped<IValidator<ApplicationUpdateDto>, ApplicationUpdateValidator>();
        services.AddScoped<IValidator<ProcessorDecisionDto>, ProcessorDecisionValidator>();

        services.AddScoped<ICommandHandler<CreateApplicationCommand, CreateApplicationResult>, CreateApplicationHandler>();
        services.AddScoped<ICommandHandler<SubmitApplicationCommand, ApplicationResponse>, SubmitApplicationHandler>();
        services.AddScoped<ICommandHandler<UpdateApplicationCommand, UpdateApplicationResult>, UpdateApplicationHandler>();
        services.AddScoped<ICommandHandler<DeleteApplicationCommand, bool>, DeleteApplicationHandler>();
        services.AddScoped<ICommandHandler<ProcessDecisionCommand, ProcessDecisionResult>, ProcessDecisionHandler>();
        services.AddScoped<ICommandHandler<CreateInquiryCommand, object>, CreateInquiryHandler>();
        services.AddScoped<ICommandHandler<AnswerInquiryCommand, ApplicationResponse>, AnswerInquiryHandler>();

        services.AddScoped<IQueryHandler<GetApplicationQuery, ApplicationResponse>, GetApplicationHandler>();
        services.AddScoped<IQueryHandler<GetApplicationsByUserQuery, ApplicationResponse[]>, GetApplicationsByUserHandler>();
        services.AddScoped<IQueryHandler<GetProcessorApplicationsQuery, ProcessorApplicationsResponse>, GetProcessorApplicationsHandler>();
        services.AddScoped<IQueryHandler<GetDashboardStatsQuery, DashboardStatsDto>, GetDashboardStatsHandler>();
        services.AddScoped<IQueryHandler<GetInquiriesQuery, List<ApplicationInquiry>>, GetInquiriesHandler>();

        return services;
    }
}
