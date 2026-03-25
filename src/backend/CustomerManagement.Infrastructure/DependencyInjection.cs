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
using SharedKernel;
using SharedKernel.Persistence;

namespace CustomerManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCustomerInfrastructure(this IServiceCollection services,
        string connectionString)
    {
        services.AddSingleton<DomainEventDispatchInterceptor>();

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

        var applicationAssembly = typeof(CustomerCreateDto).Assembly;
        var infrastructureAssembly = typeof(DependencyInjection).Assembly;
        services.AddSharedKernel(applicationAssembly, infrastructureAssembly);

        return services;
    }

    public static IServiceCollection AddMessaging(this IServiceCollection services, string connectionString)
    {
        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(connectionString));
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
