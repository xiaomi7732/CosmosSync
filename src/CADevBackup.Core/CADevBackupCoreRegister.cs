using CodeWithSaar.CosmosDBSync.CLI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CodeWithSaar.CosmosDBSync.Core;

public static class CADevBackupCoreRegister
{
    public static IServiceCollection TryAddCADevBackupCoreServices(this IServiceCollection services)
    {
        services.TryAddSingleton(typeof(ICosmosClientProvider<>), typeof(CosmosClientProvider<>));
        return services;
    }
}