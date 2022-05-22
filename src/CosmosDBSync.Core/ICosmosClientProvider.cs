using Microsoft.Azure.Cosmos;

namespace CodeWithSaar.CosmosDBSync.CLI;

public interface ICosmosClientProvider<TOptions>
{
    public TOptions Options { get; }
    CosmosClient GetCosmosClient();
}
