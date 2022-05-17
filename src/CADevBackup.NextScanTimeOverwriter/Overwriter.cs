using CADevBackup.ChangeFeedProcessing;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CADevBackup.NextScanTimeOverwriter;

internal class Overwriter
{
    private readonly NextScanTimeOptions _options;
    private readonly ICosmosClientProvider<TargetScheduleOptions> _cosmosClientProvider;
    private readonly ILogger _logger;

    public Overwriter(
        IOptions<NextScanTimeOptions> options,
        ICosmosClientProvider<TargetScheduleOptions> cosmosClientProvider,
        ILogger<Overwriter> logger
        )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _cosmosClientProvider = cosmosClientProvider ?? throw new ArgumentNullException(nameof(cosmosClientProvider));
    }

    public async Task RunAsync(DateTime utcNow, CancellationToken cancellationToken)
    {
        DateTime minNextScanTime = utcNow.Add(_options.MinNextScanTimeFromUTCNow);
        TimeSpan interval = _options.SLAInterval;

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
                item.OriginalNextScanTime = item.NextScanTime;
                DateTime nextScanTime = item.NextScanTime;

                if (nextScanTime >= minNextScanTime)
                {
                    _logger.LogDebug("Next scan time {nextScanTime:o} is already >= minimum: {min:o}", nextScanTime, minNextScanTime);
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