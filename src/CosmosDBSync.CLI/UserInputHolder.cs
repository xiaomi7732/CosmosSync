using System.Text;

namespace CodeWithSaar.CosmosDBSync.CLI;

internal class UserInputHolder
{
    public BackupOptions BackupOptions { get; } = new BackupOptions();
    public SourceCosmosDBOptions SourceCosmosDBOptions { get; } = new SourceCosmosDBOptions();
    public DestCosmosDBOptions DestCosmosDBOptions { get; } = new DestCosmosDBOptions();

    public string GetSummary()
    {
        StringBuilder builder = new StringBuilder();

        // Source
        builder.AppendLine("==== Source DB ====");
        builder.AppendLine("Source DB Connection string:");
        builder.AppendLine(SourceCosmosDBOptions.ConnectionString?.Substring(0, 80) + "...");
        builder.AppendLine("Source DB Name:");
        builder.AppendLine(SourceCosmosDBOptions.DatabaseId);
        builder.AppendLine("Source DB container name:");
        builder.AppendLine(BackupOptions.SourceContainerName);

        // Dest
        builder.AppendLine();
        builder.AppendLine("==== Target DB ====");
        builder.AppendLine("Target DB Connection string:");
        builder.AppendLine(DestCosmosDBOptions.ConnectionString?.Substring(0, 80) + "...");
        builder.AppendLine("Target DB Name:");
        builder.AppendLine(DestCosmosDBOptions.DatabaseId);
        builder.AppendLine("Target DB container name:");
        builder.AppendLine(BackupOptions.DestContainerName);
        builder.AppendLine("Target DB container partition key when needed:");
        builder.AppendLine(BackupOptions.DestContainerPartitionKeyPath);

        // Lease
        builder.AppendLine();
        builder.AppendLine("==== Lease Container ====");
        builder.AppendLine("Lease container name:");
        builder.AppendLine(BackupOptions.LeaseContainerName);

        return builder.ToString();
    }
}