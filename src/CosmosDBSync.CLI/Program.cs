using CodeWithSaar.CosmosDBSync.Core;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CodeWithSaar.CosmosDBSync.CLI // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        private static bool _isInteractiveMode;
        static async Task Main(string[] args)
        {
            Parser.Default.ParseArguments<CLIOptions>(args).WithParsed<CLIOptions>(o =>
            {
                _isInteractiveMode = o.IsInteractive;
                Console.WriteLine("========================================");
                Console.WriteLine("Sync your CosmosDB containers with ease.");
                Console.WriteLine("========================================");
            })
            .WithNotParsed(errors =>
            {
                Environment.Exit(1);
            });
            Console.WriteLine("Interactive mode: {0}", _isInteractiveMode);

            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddJsonFile("appsettings.jsonc", optional: false, reloadOnChange: true)
                        .AddCommandLine(args)
                        .AddUserSecrets<Program>().Build();

                })
                .ConfigureLogging(ConfigureLogging)
                .ConfigureServices(RegisterServices)
                .Build();

            await host.RunAsync();
        }

        private static void ConfigureLogging(HostBuilderContext context, ILoggingBuilder logging)
        {
            IConfiguration configuration = context.Configuration;

            logging.ClearProviders();
            logging.AddConfiguration(configuration.GetSection("Logging"));

            logging.AddSimpleConsole(opt =>
            {
                opt.SingleLine = true;
            });
            logging.AddFile();
        }

        private static void RegisterServices(HostBuilderContext context, IServiceCollection services)
        {

            IConfiguration configuration = context.Configuration;

            services.TryAddCADevBackupCoreServices();

            if (_isInteractiveMode)
            {
                UserInputHolder userInputs = GatherUserInputs();
                Console.WriteLine(userInputs.GetSummary());
                Console.WriteLine("Press any key to continue. Ctrl + C to abort.");
                Console.ReadKey(intercept: true);
                services.AddSingleton<UserInputHolder>(p => userInputs);

                services.AddOptions<BackupOptions>().Configure<UserInputHolder>((opt, holder) =>
                {
                    opt.UpdateFrom(holder.BackupOptions);
                });

                services.AddOptions<SourceCosmosDBOptions>().Configure<UserInputHolder>((opt, holder) =>
                {
                    opt.UpdateFrom(holder.SourceCosmosDBOptions);
                });

                services.AddOptions<DestCosmosDBOptions>().Configure<UserInputHolder>((opt, holder) =>
                {
                    opt.UpdateFrom(holder.DestCosmosDBOptions);
                });
            }
            else
            {
                services.AddOptions<BackupOptions>().Bind(configuration.GetSection(BackupOptions.SectionName));
                services.AddOptions<SourceCosmosDBOptions>().Bind(configuration.GetSection(SourceCosmosDBOptions.SectionName));
                services.AddOptions<DestCosmosDBOptions>().Bind(configuration.GetSection(DestCosmosDBOptions.SectionName));
            }

            services.AddHostedService<ChangeFeedProcessorManager>();
        }

        private static UserInputHolder GatherUserInputs()
        {
            UserInputHolder userInputHolder = new UserInputHolder();

            // Input
            Console.WriteLine("Let's start with Source DB:");
            string dbConnectionString = PromptForValue("Source DB connection string:");
            userInputHolder.SourceCosmosDBOptions.ConnectionString = dbConnectionString;
            string databaseId = PromptForValue("Source DB Name:");
            userInputHolder.SourceCosmosDBOptions.DatabaseId = databaseId;
            string containerName = PromptForValue("Source DB Container Name:");
            userInputHolder.BackupOptions.SourceContainerName = containerName;
            Console.WriteLine("Okay.");

            // Output
            Console.WriteLine("Destination DB:");
            dbConnectionString = PromptForValue("Destination DB connection string:");
            userInputHolder.DestCosmosDBOptions.ConnectionString = dbConnectionString;
            databaseId = PromptForValue("Destination DB Name:", userInputHolder.SourceCosmosDBOptions.DatabaseId);
            userInputHolder.DestCosmosDBOptions.DatabaseId = databaseId;
            containerName = PromptForValue("Destination DB container name:", userInputHolder.BackupOptions.SourceContainerName);
            userInputHolder.BackupOptions.DestContainerName = containerName;
            string partitionKey = PromptForValue("What will be the partition key for target container in case needed?", "/id");
            userInputHolder.BackupOptions.DestContainerPartitionKeyPath = partitionKey;
            Console.WriteLine("Okay.");

            // Lease
            Console.WriteLine("There is a lease container needed to record the working progress. Target DB will be used to host the lease container.");
            string leaseContainerName = PromptForValue("What do you like for the lease container name", userInputHolder.BackupOptions.DestContainerName + "Lease");
            userInputHolder.BackupOptions.LeaseContainerName = leaseContainerName;
            return userInputHolder;
        }

        private static string PromptForValue(string prompt, string defaultValue = "", Predicate<string>? validator = null)
        {
            WritePrompt(prompt, defaultValue);
            validator ??= (value) => !string.IsNullOrEmpty(value);

            string? value = Console.ReadLine();
            if (string.IsNullOrEmpty(value))
            {
                value = defaultValue;
            }

            while (!validator.Invoke(value))
            {
                Console.WriteLine("This value is required.");
                WritePrompt(prompt, defaultValue);
                value = Console.ReadLine();
                if (string.IsNullOrEmpty(value))
                {
                    value = defaultValue;
                }
            }
            return value;
        }

        private static void WritePrompt(string prompt, string defaultValue = "")
        {
            Console.Write(prompt);
            if (!string.IsNullOrEmpty(defaultValue))
            {
                Console.Write($"(Default: {defaultValue})");
            }
            Console.WriteLine();
        }
    }
}