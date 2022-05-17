using System;
using System.Threading;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace CADevBackup.ChangeFeedProcessing;

public class CosmosClientProvider<TOptions> : ICosmosClientProvider<TOptions>
    where TOptions : CosmosDBOptionsBase, new()
{
    private readonly TOptions _cosmosDBOptions;
    private readonly Uri _cosmosDBEndpoint;
    private readonly string _cosmosDBKey;
    private readonly Lazy<CosmosClient> _cosmosClient;

    public CosmosClientProvider(IOptions<TOptions> cosmosDBOptions)
    {
        _cosmosDBOptions = cosmosDBOptions?.Value ?? throw new ArgumentNullException(nameof(cosmosDBOptions));
        _cosmosDBEndpoint = _cosmosDBOptions.DatabaseUri ?? throw new ArgumentNullException(nameof(cosmosDBOptions.Value.DatabaseUri));
        _cosmosDBKey = _cosmosDBOptions.DatabaseKey ?? throw new ArgumentNullException(nameof(cosmosDBOptions.Value.DatabaseKey));

        _cosmosClient = new Lazy<CosmosClient>(() => new CosmosClient(accountEndpoint: _cosmosDBEndpoint.AbsoluteUri, authKeyOrResourceToken: _cosmosDBKey), LazyThreadSafetyMode.PublicationOnly);
        DatabaseId = _cosmosDBOptions.DatabaseId ?? throw new ArgumentNullException(nameof(_cosmosDBOptions.DatabaseId));
    }

    public string DatabaseId { get; }

    public TOptions Options => _cosmosDBOptions;

    public CosmosClient GetCosmosClient() => _cosmosClient.Value;
}