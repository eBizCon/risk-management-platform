using System.Reflection;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;
using RiskManagement.Application.Validation;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.Aggregates.ScoringConfigAggregate;
using RiskManagement.Domain.Services;
using RiskManagement.Infrastructure.Dispatching;
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
        services.AddScoped<IScoringConfigRepository, ScoringConfigRepository>();
        services.AddScoped<DatabaseSeeder>();

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IScoringService, ScoringService>();

        services.AddScoped<IValidator<ApplicationCreateDto>, ApplicationValidator>();
        services.AddScoped<IValidator<ApplicationUpdateDto>, ApplicationUpdateValidator>();
        services.AddScoped<IValidator<ApproveApplicationDto>, ApproveApplicationValidator>();
        services.AddScoped<IValidator<RejectApplicationDto>, RejectApplicationValidator>();

        services.AddScoped<IDispatcher, Dispatcher>();

        RegisterHandlers(services, typeof(ICommandHandler<,>));
        RegisterHandlers(services, typeof(IQueryHandler<,>));
        RegisterHandlers(services, typeof(IDomainEventHandler<>));

        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, Type openGenericInterface)
    {
        var assembly = Assembly.GetAssembly(typeof(ICommandHandler<,>))!;

        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface)
                continue;

            foreach (var iface in type.GetInterfaces())
            {
                if (!iface.IsGenericType)
                    continue;

                if (iface.GetGenericTypeDefinition() == openGenericInterface) services.AddScoped(iface, type);
            }
        }
    }
}