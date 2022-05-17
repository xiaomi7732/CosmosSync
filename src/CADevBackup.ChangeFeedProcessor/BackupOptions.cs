namespace CADevBackup.ChangeFeedProcessing;

public class BackupOptions
{
    public const string SectionName = "BackupOptions";

    public string SourceContainerName { get; set; } = "SubscriptionSchedule";
    public string DestContainerName { get; set; } = "SubscriptionSchedule";
}