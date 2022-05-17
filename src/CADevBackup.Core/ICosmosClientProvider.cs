using Microsoft.Azure.Cosmos;

namespace CADevBackup.ChangeFeedProcessing;

public interface ICosmosClientProvider<TOptions>
{
    public TOptions Options { get; }
    CosmosClient GetCosmosClient();
}
