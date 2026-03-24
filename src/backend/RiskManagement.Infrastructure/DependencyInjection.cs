using System.Reflection;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RiskManagement.Application.DTOs;
using RiskManagement.Application.Sagas.ApplicationCreation;
using RiskManagement.Application.Services;
using RiskManagement.Application.Validation;
using RiskManagement.Domain.Aggregates.ApplicationAggregate;
using RiskManagement.Domain.Aggregates.ScoringConfigAggregate;
using RiskManagement.Domain.ReadModels;
using RiskManagement.Domain.Services;
using RiskManagement.Infrastructure.ExternalServices;
using RiskManagement.Infrastructure.HttpClients;
using RiskManagement.Infrastructure.Persistence;
using RiskManagement.Infrastructure.Consumers;
using RiskManagement.Infrastructure.Sagas;
using RiskManagement.Infrastructure.Sagas.Consumers;
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
        services.AddScoped<ICustomerReadModelRepository, CustomerReadModelRepository>();
        services.AddScoped<DatabaseSeeder>();

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        string customerServiceUrl, string serviceApiKey)
    {
        services.AddSingleton<IScoringService, ScoringService>();
        services.AddScoped<ICreditCheckService, MockSchufaProvider>();

        services.AddScoped<IValidator<ApplicationCreateDto>, ApplicationValidator>();
        services.AddScoped<IValidator<ApplicationUpdateDto>, ApplicationUpdateValidator>();
        services.AddScoped<IValidator<ApproveApplicationDto>, ApproveApplicationValidator>();
        services.AddScoped<IValidator<RejectApplicationDto>, RejectApplicationValidator>();

        services.AddHttpClient<ICustomerProfileService, CustomerServiceClient>(client =>
        {
            client.BaseAddress = new Uri(customerServiceUrl);
            client.DefaultRequestHeaders.Add("X-Api-Key", serviceApiKey);
        });

        services.AddScoped<IDispatcher, Dispatcher>();

        var applicationAssembly = typeof(ApplicationCreateDto).Assembly;
        RegisterHandlers(services, typeof(ICommandHandler<,>), applicationAssembly);
        RegisterHandlers(services, typeof(IQueryHandler<,>), applicationAssembly);
        RegisterHandlers(services, typeof(IDomainEventHandler<>), applicationAssembly);

        return services;
    }

    public static IServiceCollection AddMessaging(this IServiceCollection services, string connectionString)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<FetchCustomerProfileConsumer>();
            x.AddConsumer<FetchCustomerProfileFaultConsumer>();
            x.AddConsumer<PerformCreditCheckConsumer>();
            x.AddConsumer<PerformCreditCheckFaultConsumer>();
            x.AddConsumer<FinalizeApplicationConsumer>();
            x.AddConsumer<FinalizeApplicationUpdateConsumer>();
            x.AddConsumer<FinalizeApplicationUpdateFaultConsumer>();
            x.AddConsumer<MarkApplicationFailedConsumer>();

            x.AddConsumer<CustomerCreatedConsumer>();
            x.AddConsumer<CustomerUpdatedConsumer>();
            x.AddConsumer<CustomerActivatedConsumer>();
            x.AddConsumer<CustomerArchivedConsumer>();
            x.AddConsumer<CustomerDeletedConsumer>();

            x.AddSagaStateMachine<ApplicationCreationStateMachine, ApplicationCreationState>()
                .EntityFrameworkRepository(r =>
                {
                    r.ExistingDbContext<ApplicationDbContext>();
                    r.UsePostgres();
                });

            x.SetKebabCaseEndpointNameFormatter();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(connectionString));

                cfg.UseMessageRetry(r => r.Intervals(
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(15),
                    TimeSpan.FromSeconds(30)));

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, Type openGenericInterface, Assembly assembly)
    {
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