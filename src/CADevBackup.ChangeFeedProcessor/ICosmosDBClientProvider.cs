using Microsoft.Azure.Cosmos;

namespace CADevBackup.ChangeFeedProcessing;

internal interface ICosmosClientProvider
{
    public string DatabaseId { get; }
    CosmosClient GetCosmosClient();
}

internal interface ISourceCosmosClientProvider : ICosmosClientProvider
{

}

internal interface IDestCosmosClientProvider : ICosmosClientProvider
{

}