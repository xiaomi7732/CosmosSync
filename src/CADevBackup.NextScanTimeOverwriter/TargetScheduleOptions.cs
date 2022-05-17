using CADevBackup.ChangeFeedProcessing;

namespace CADevBackup.NextScanTimeOverwriter;

internal class TargetScheduleOptions : CosmosDBOptionsBase
{
    public const string SectionName = "TargetDB";
    public string ContainerName { get; set; } = "SubscriptionSchedule";
}