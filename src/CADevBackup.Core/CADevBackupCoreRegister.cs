using CADevBackup.ChangeFeedProcessing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CADevBackup.Core;

public static class CADevBackupCoreRegister
{
    public static IServiceCollection TryAddCADevBackupCoreServices(this IServiceCollection services)
    {
        services.TryAddSingleton(typeof(ICosmosClientProvider<>), typeof(CosmosClientProvider<>));
        return services;
    }
}