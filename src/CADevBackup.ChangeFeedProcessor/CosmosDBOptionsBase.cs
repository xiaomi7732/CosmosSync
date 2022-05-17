namespace CADevBackup.ChangeFeedProcessing;

public class CosmosDBOptionsBase
{
    public Uri? DatabaseUri { get; set; }
    public string? DatabaseKey { get; set; }
    public string? DatabaseId { get; set; }
}