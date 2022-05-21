using System;
using System.Threading;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace CodeWithSaar.CosmosDBSync.CLI;

public class CosmosClientProvider<TOptions> : ICosmosClientProvider<TOptions>
    where TOptions : CosmosDBOptionsBase, new()
{
    private readonly TOptions _cosmosDBOptions;
    private readonly Lazy<CosmosClient> _cosmosClient;

    public CosmosClientProvider(IOptions<TOptions> cosmosDBOptions)
    {
        _cosmosDBOptions = cosmosDBOptions?.Value ?? throw new ArgumentNullException(nameof(cosmosDBOptions));
        _cosmosClient = new Lazy<CosmosClient>(() => new CosmosClient(connectionString: _cosmosDBOptions.ConnectionString), LazyThreadSafetyMode.PublicationOnly);
    }

    public TOptions Options => _cosmosDBOptions;

    public CosmosClient GetCosmosClient() => _cosmosClient.Value;
}