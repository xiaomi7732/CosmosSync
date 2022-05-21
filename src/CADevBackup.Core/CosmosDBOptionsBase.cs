namespace CodeWithSaar.CosmosDBSync.CLI;

public class CosmosDBOptionsBase
{
    public string? ConnectionString { get; set; }
    public string? DatabaseId { get; set; }

    public virtual void UpdateFrom(CosmosDBOptionsBase other)
    {
        ConnectionString = other.ConnectionString;
        DatabaseId = other.DatabaseId;
    }
}