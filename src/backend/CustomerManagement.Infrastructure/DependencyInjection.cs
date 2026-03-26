using System.Reflection;
using CustomerManagement.Application.Commands;
using CustomerManagement.Application.DTOs;
using CustomerManagement.Application.Validation;
using CustomerManagement.Domain.Aggregates.CustomerAggregate;
using CustomerManagement.Infrastructure.HttpClients;
using CustomerManagement.Infrastructure.Persistence;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Dispatching;
using SharedKernel.Persistence;

namespace CustomerManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCustomerInfrastructure(this IServiceCollection services,
        string connectionString)
    {
        services.AddScoped<DomainEventDispatchInterceptor>();

        services.AddDbContext<CustomerDbContext>((sp, options) =>
            options.UseNpgsql(connectionString)
                .AddInterceptors(sp.GetRequiredService<DomainEventDispatchInterceptor>()));

        services.AddScoped<ICustomerRepository, CustomerRepository>();

        return services;
    }

    public static IServiceCollection AddCustomerApplicationServices(this IServiceCollection services,
        string applicationServiceUrl, string serviceApiKey)
    {
        services.AddScoped<IValidator<CustomerCreateDto>, CustomerCreateValidator>();
        services.AddScoped<IValidator<CustomerUpdateDto>, CustomerUpdateValidator>();

        services.AddHttpClient<IApplicationServiceClient, ApplicationServiceClient>(client =>
        {
            client.BaseAddress = new Uri(applicationServiceUrl);
            client.DefaultRequestHeaders.Add("X-Api-Key", serviceApiKey);
        });

        services.AddScoped<IDispatcher, Dispatcher>();

        var applicationAssembly = typeof(CustomerCreateDto).Assembly;
        var infrastructureAssembly = typeof(DependencyInjection).Assembly;
        RegisterHandlers(services, typeof(ICommandHandler<,>), applicationAssembly);
        RegisterHandlers(services, typeof(IQueryHandler<,>), applicationAssembly);
        RegisterHandlers(services, typeof(IDomainEventHandler<>), applicationAssembly);
        RegisterHandlers(services, typeof(IDomainEventHandler<>), infrastructureAssembly);

        return services;
    }

    public static IServiceCollection AddMessaging(this IServiceCollection services, string connectionString)
    {
        services.AddMassTransit(x =>
        {
            x.AddEntityFrameworkOutbox<CustomerDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });

            x.SetKebabCaseEndpointNameFormatter();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(connectionString));
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

                if (iface.GetGenericTypeDefinition() == openGenericInterface)
                    services.AddScoped(iface, type);
            }
        }
    }
}