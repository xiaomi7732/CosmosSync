namespace CADevBackup.ChangeFeedProcessing;

internal class SourceCosmosDBOptions : CosmosDBOptionsBase
{
    public const string SectionName = "SourceCosmosDB";
    public string? SourceContainerName { get; set; }
}