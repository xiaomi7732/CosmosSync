using CADevBackup.ChangeFeedProcessing;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace CADevBackup.NextScanTimeOverwriter;

internal class Overwriter
{
    private readonly ICosmosClientProvider<TargetScheduleOptions> _cosmosClientProvider;
    private readonly ILogger _logger;

    public Overwriter(
        ICosmosClientProvider<TargetScheduleOptions> cosmosClientProvider,
        ILogger<Overwriter> logger
        )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cosmosClientProvider = cosmosClientProvider ?? throw new ArgumentNullException(nameof(cosmosClientProvider));
    }

    public async Task RunAsync(DateTime minNextScanTime, TimeSpan interval, CancellationToken cancellationToken)
    {
        Database db = _cosmosClientProvider.GetCosmosClient().GetDatabase(_cosmosClientProvider.Options.DatabaseId);
        Container targetContainer = db.GetContainer(_cosmosClientProvider.Options.ContainerName);

        FeedIterator<dynamic> iterator = targetContainer.GetItemQueryIterator<dynamic>(requestOptions: new QueryRequestOptions()
        {
            MaxItemCount = -1,
        });

        while (iterator.HasMoreResults)
        {
            FeedResponse<dynamic> group = await iterator.ReadNextAsync(cancellationToken);
            foreach (dynamic item in group)
            {
                DateTime nextScanTime = item.NextScanTime;

                if(nextScanTime >= minNextScanTime)
                {
                    continue;
                }
                DateTime newNextScanTime = nextScanTime;
                while (newNextScanTime <= minNextScanTime)
                {
                    newNextScanTime += interval;
                }
                _logger.LogInformation("Partition Key: {partitionKey}. Next Scan Time: {from:o}. New min: {min:o}. New next scan time: {new:o}", (string)item.PartitionKey, nextScanTime, minNextScanTime, newNextScanTime);
                item.NextScanTime = newNextScanTime;

                targetContainer.UpsertItemAsync(item);
            }
        }
    }
}