using CodeWithSaar.CosmosDBSync.CLI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CodeWithSaar.CosmosDBSync.Core;

public static class CosmosDBSyncCoreRegister
{
    public static IServiceCollection TryAddCosmosDBSyncCoreServices(this IServiceCollection services)
    {
        services.TryAddSingleton(typeof(ICosmosClientProvider<>), typeof(CosmosClientProvider<>));
        return services;
    }
}