namespace CADevBackup.ChangeFeedProcessing;

internal class DestCosmosDBOptions : CosmosDBOptionsBase
{
    public const string SectionName = "DestCosmosDB";
    public string? LeaseContainerName { get; set; }
}