using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CodeWithSaar.CosmosDBSync.CLI;

internal class ChangeFeedProcessorManager : BackgroundService
{
    private readonly ICosmosClientProvider<SourceCosmosDBOptions> _sourceCosmosClient;
    private readonly ICosmosClientProvider<DestCosmosDBOptions> _destCosmosClient;
    private readonly ILogger _logger;
    private readonly BackupOptions _backupOptions;

    private ChangeFeedProcessor? _feedProcessor;

    public ChangeFeedProcessorManager(
        IOptions<BackupOptions> backupOptions,
        ICosmosClientProvider<SourceCosmosDBOptions> sourceCosmosClient,
        ICosmosClientProvider<DestCosmosDBOptions> destCosmosClient,
        ILogger<ChangeFeedProcessorManager> logger)
    {
        _backupOptions = backupOptions?.Value ?? throw new ArgumentNullException(nameof(backupOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _sourceCosmosClient = sourceCosmosClient ?? throw new ArgumentNullException(nameof(sourceCosmosClient));
        _destCosmosClient = destCosmosClient ?? throw new ArgumentNullException(nameof(destCosmosClient));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting backup service.");

        await PrepareSourceDBAsync(stoppingToken);
        await PrepareDestDBAsync(stoppingToken);
        await PrepareLeaseContainerAsync(stoppingToken);

        _feedProcessor = CreateChangeFeedProcessor();
        _logger.LogInformation("Starting Change Feed Processor...");
        await _feedProcessor.StartAsync();
        _logger.LogInformation("Watching the change feed ...");
    }

    private async Task PrepareSourceDBAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Checking source database {dbName}", _sourceCosmosClient.Options.DatabaseId);
            Database sourceDataBase = _sourceCosmosClient.GetCosmosClient().GetDatabase(_sourceCosmosClient.Options.DatabaseId);
            DatabaseResponse sourceDatabaseResponse = await sourceDataBase.ReadAsync(cancellationToken: cancellationToken);
            if (sourceDatabaseResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException("Source database can't be reached.");
            }

            string containerName = _backupOptions.SourceContainerName;
            _logger.LogInformation("Checking source container {containerName}", containerName);
            Container container = sourceDataBase.GetContainer(_backupOptions.SourceContainerName);
            ContainerResponse sourceContainerResponse = await container.ReadContainerAsync(cancellationToken: cancellationToken);
            if (sourceContainerResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException($"Source container {containerName} can't be reached.");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Unexpected error preparing source db.", ex);
        }
    }

    private async Task PrepareDestDBAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Prepare the destination database");

        try
        {
            string databaseId = _destCosmosClient.Options.DatabaseId!;
            DatabaseResponse targetDBResponse = await _destCosmosClient.GetCosmosClient().CreateDatabaseIfNotExistsAsync(databaseId, cancellationToken: cancellationToken);
            if (targetDBResponse.StatusCode == HttpStatusCode.Created)
            {
                _logger.LogInformation("Target database {databaseId} created.", databaseId);
            }
            else if (targetDBResponse.StatusCode == HttpStatusCode.OK)
            {
                _logger.LogInformation("Target database {databaseId} exists.", databaseId);
            }
            else
            {
                _logger.LogWarning("Unexpected status code returned preparing destination db: {statusCode}", targetDBResponse.StatusCode);
            }

            Database destDatabase = targetDBResponse.Database;

            string containerId = _backupOptions.DestContainerName;
            _logger.LogInformation("Prepare target container {containerName}", containerId);
            ContainerProperties targetContainerProperties = new ContainerProperties()
            {
                Id = _backupOptions.DestContainerName,
                PartitionKeyPath = _backupOptions.DestContainerPartitionKeyPath,
            };
            ContainerResponse containerResponse = await destDatabase.CreateContainerIfNotExistsAsync(targetContainerProperties, cancellationToken: cancellationToken);
            if (containerResponse.StatusCode == HttpStatusCode.OK)
            {
                _logger.LogInformation("Target container {containerId} exists.", containerId);
            }
            else if (containerResponse.StatusCode == HttpStatusCode.Created)
            {
                _logger.LogInformation("Target container {containerId} created", containerId);
            }
            else
            {
                _logger.LogWarning("Unexpected status code returned creating target container: {statusCode}", containerResponse.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error preparing the target.");
            throw;
        }
    }

    private async Task PrepareLeaseContainerAsync(CancellationToken cancellationToken)
    {
        string leaseContainerName = _backupOptions.LeaseContainerName;
        _logger.LogInformation("Prepare lease container {containerName}...", leaseContainerName);

        ContainerProperties leaseContainerProperties = new ContainerProperties()
        {
            Id = leaseContainerName,
            PartitionKeyPath = "/id",
        };
        Database destDatabase = _destCosmosClient.GetCosmosClient().GetDatabase(_destCosmosClient.Options.DatabaseId);
        ContainerResponse leaseContainerResponse = await destDatabase.CreateContainerIfNotExistsAsync(leaseContainerProperties);
        if (leaseContainerResponse.StatusCode == HttpStatusCode.OK)
        {
            _logger.LogInformation("Lease container {containerName} exists.", leaseContainerName);
        }
        else if (leaseContainerResponse.StatusCode == HttpStatusCode.Created)
        {
            _logger.LogInformation("Lease container {containerId} created.", leaseContainerName);
        }
    }

    private ChangeFeedProcessor CreateChangeFeedProcessor()
    {
        Container leaseContainer = _destCosmosClient.GetCosmosClient().GetContainer(_destCosmosClient.Options.DatabaseId, _backupOptions.LeaseContainerName);
        ChangeFeedProcessor changeFeedProcessor = _sourceCosmosClient.GetCosmosClient().GetContainer(_sourceCosmosClient.Options.DatabaseId, _backupOptions.SourceContainerName)
            .GetChangeFeedProcessorBuilder<dynamic>(processorName: "scheduleSubscriptionBackup", onChangesDelegate: HandleChangesAsync)
            .WithInstanceName("consoleHost")
            .WithLeaseContainer(leaseContainer)
            .WithStartTime(DateTime.MinValue.ToUniversalTime())
            .WithErrorNotification(OnError)
            .Build();

        return changeFeedProcessor;
    }

    private Task OnError(string leaseToken, Exception exception)
    {
        _logger.LogError(exception, $"Error processing lease: {leaseToken}", leaseToken);
        return Task.CompletedTask;
    }

    /// <summary>
    /// The delegate receives batches of changes as they are generated in the change feed and can process them.
    /// </summary>
    private async Task HandleChangesAsync(ChangeFeedProcessorContext context, IReadOnlyCollection<dynamic> changes, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Started handling a batch of changes for lease {context.LeaseToken}...");
        _logger.LogInformation($"Change Feed request consumed {context.Headers.RequestCharge} RU.");
        // SessionToken if needed to enforce Session consistency on another client instance
        _logger.LogInformation($"SessionToken ${context.Headers.Session}");

        // We may want to track any operation's Diagnostics that took longer than some threshold
        if (context.Diagnostics.GetClientElapsedTime() > TimeSpan.FromSeconds(1))
        {
            _logger.LogInformation($"Change Feed request took longer than expected. Diagnostics:" + context.Diagnostics.ToString());
        }

        Container? destContainer = _destCosmosClient.GetCosmosClient().GetContainer(_destCosmosClient.Options.DatabaseId, _backupOptions.DestContainerName);

        _logger.LogInformation("{from,40} => {to}", "From", "To");
        foreach (dynamic item in changes)
        {
            _logger.LogInformation("{sourceId,40} => {destId}", (string)item.id, (string)item.id);
            await destContainer.UpsertItemAsync<dynamic>(item);
        }

        _logger.LogInformation("Finished handling a batch of changes.");
    }
}