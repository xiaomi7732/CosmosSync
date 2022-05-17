using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CADevBackup.ChangeFeedProcessing;

internal class ChangeFeedProcessorManager : BackgroundService
{
    private readonly ISourceCosmosClientProvider _sourceCosmosClient;
    private readonly IDestCosmosClientProvider _destCosmosClient;
    private readonly ILogger _logger;
    private readonly BackupOptions _backupOptions;

    public ChangeFeedProcessorManager(
        IOptions<BackupOptions> backupOptions,
        ISourceCosmosClientProvider sourceCosmosClient,
        IDestCosmosClientProvider destCosmosClient,
        ILogger<ChangeFeedProcessorManager> logger)
    {
        _backupOptions = backupOptions?.Value ?? throw new ArgumentNullException(nameof(backupOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _sourceCosmosClient = sourceCosmosClient ?? throw new ArgumentNullException(nameof(sourceCosmosClient));
        _destCosmosClient = destCosmosClient ?? throw new ArgumentNullException(nameof(destCosmosClient));
    }

    public async Task<ChangeFeedProcessor> StartChangeFeedProcessorAsync(string leaseContainerName)
    {
        if (string.IsNullOrWhiteSpace(leaseContainerName))
        {
            throw new ArgumentException($"'{nameof(leaseContainerName)}' cannot be null or whitespace.", nameof(leaseContainerName));
        }

        ContainerProperties leaseContainerProperties = new ContainerProperties()
        {
            Id = leaseContainerName,
            PartitionKeyPath = "/id"
        };
        Database destDatabase = _destCosmosClient.GetCosmosClient().GetDatabase(_destCosmosClient.DatabaseId);
        await destDatabase.CreateContainerIfNotExistsAsync(leaseContainerProperties);
        Container leaseContainer = _destCosmosClient.GetCosmosClient().GetContainer(_destCosmosClient.DatabaseId, leaseContainerName);

        ChangeFeedProcessor changeFeedProcessor = _sourceCosmosClient.GetCosmosClient().GetContainer(_sourceCosmosClient.DatabaseId, _backupOptions.SourceContainerName)
            .GetChangeFeedProcessorBuilder<dynamic>(processorName: "scheduleSubscriptionBackup", onChangesDelegate: HandleChangesAsync)
            .WithInstanceName("consoleHost")
            .WithLeaseContainer(leaseContainer)
            .WithStartTime(DateTime.MinValue.ToUniversalTime())
            .Build();

        _logger.LogInformation("Starting Change Feed Processor...");
        await changeFeedProcessor.StartAsync();
        _logger.LogInformation("Change Feed Processor started.");
        return changeFeedProcessor;
    }

    /// <summary>
    /// The delegate receives batches of changes as they are generated in the change feed and can process them.
    /// </summary>
    private async Task HandleChangesAsync(ChangeFeedProcessorContext context, IReadOnlyCollection<dynamic> changes, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Started handling changes for lease {context.LeaseToken}...");
        _logger.LogInformation($"Change Feed request consumed {context.Headers.RequestCharge} RU.");
        // SessionToken if needed to enforce Session consistency on another client instance
        _logger.LogInformation($"SessionToken ${context.Headers.Session}");

        // We may want to track any operation's Diagnostics that took longer than some threshold
        if (context.Diagnostics.GetClientElapsedTime() > TimeSpan.FromSeconds(1))
        {
            _logger.LogInformation($"Change Feed request took longer than expected. Diagnostics:" + context.Diagnostics.ToString());
        }

        Container toContainer = _destCosmosClient.GetCosmosClient().GetContainer(_destCosmosClient.DatabaseId, _backupOptions.DestContainerName);
        foreach (dynamic item in changes)
        {
            _logger.LogInformation("{sourceId} => {destId}", (string)item.id, (string)item.id);
            await toContainer.UpsertItemAsync<dynamic>(item);
        }

        _logger.LogInformation("Finished handling changes.");
    }
}