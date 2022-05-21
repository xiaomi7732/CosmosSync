
using CommandLine;

namespace CodeWithSaar.CosmosDBSync.CLI;

public class CLIOptions
{
    [Option('i', "interactive", Required = false, HelpText = "Interactive mode", Default = false)]
    public bool IsInteractive { get; set; }
}