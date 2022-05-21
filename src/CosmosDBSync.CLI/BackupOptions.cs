namespace CodeWithSaar.CosmosDBSync.CLI;

public class BackupOptions
{
    public const string SectionName = "BackupOptions";

    public string SourceContainerName { get; set; } = "SubscriptionSchedule";
    public string DestContainerName { get; set; } = "SubscriptionSchedule2";
    public string DestContainerPartitionKeyPath { get; set; } = "/PartitionKey";
    public string LeaseContainerName { get; set; } = "SubscriptionScheduleLease";

    public void UpdateFrom(BackupOptions other)
    {
        SourceContainerName = other.SourceContainerName;
        DestContainerName = other.DestContainerName;
        DestContainerPartitionKeyPath = other.DestContainerPartitionKeyPath;
        LeaseContainerName = other.LeaseContainerName;
    }
}