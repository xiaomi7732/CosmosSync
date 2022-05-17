namespace CADevBackup.ChangeFeedProcessing;

public class BackupOptions
{
    public const string SectionName = "BackupOptions";

    public string SourceContainerName { get; set; } = "SubscriptionSchedule";
    public string DestContainerName { get; set; } = "SubscriptionSchedule2";
    public string DestContainerPartitionKeyPath { get; set; } = "/PartitionKey";
    public string LeaseContainerName { get; set; } = "SubscriptionScheduleLease";
}