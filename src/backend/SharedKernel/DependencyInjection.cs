using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Dispatching;

namespace SharedKernel;

public static class SharedKernelDependencyInjection
{
    public static IServiceCollection AddSharedKernel(this IServiceCollection services, params Assembly[] handlerAssemblies)
    {
        services.AddScoped<IDispatcher, Dispatcher>();

        foreach (var assembly in handlerAssemblies)
        {
            RegisterHandlers(services, typeof(ICommandHandler<,>), assembly);
            RegisterHandlers(services, typeof(IQueryHandler<,>), assembly);
            RegisterHandlers(services, typeof(IDomainEventHandler<>), assembly);
        }

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
