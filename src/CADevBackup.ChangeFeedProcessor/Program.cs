using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace CADevBackup.ChangeFeedProcessing // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        public delegate ICosmosClientProvider ServiceResolver(CosmosDBRole role);

        static async Task Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.jsonc", optional: false, reloadOnChange: true)
                .AddUserSecrets<Program>().Build();

            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection = RegisterServices(serviceCollection, configuration);

            using (ServiceProvider provider = serviceCollection.BuildServiceProvider())
            {
                ChangeFeedProcessorManager manager = provider.GetRequiredService<ChangeFeedProcessorManager>();
                ChangeFeedProcessor processor = await manager.StartChangeFeedProcessorAsync("SubscriptionSchedule", "SubscriptionSchedule", "SubscriptionScheduleLease");

                // ISourceCosmosClientProvider clientProvider = provider.GetRequiredService<ISourceCosmosClientProvider>();
                // Container sourceContainer = clientProvider.GetCosmosClient().GetContainer(clientProvider.DatabaseId, "SubscriptionSchedule");
                // FeedIterator<dynamic> items =  sourceContainer.GetItemQueryIterator<dynamic>(queryText:null, continuationToken:null);
                // while(items.HasMoreResults)
                // {
                //     FeedResponse<dynamic> aPage = await items.ReadNextAsync();
                //     foreach(dynamic item in aPage)
                //     {
                //         Console.WriteLine("{0}: {1}", item.id, item.creationTime);
                //     }
                // }
                Console.WriteLine("Press any key to finish");
                Console.ReadKey();
            }
        }

        private static IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConfiguration(configuration.GetSection("Logging"));

                loggingBuilder.AddSimpleConsole(opt =>
                {
                    opt.SingleLine = true;
                });
                loggingBuilder.AddFile();
            });

            services.AddOptions<BackupOptions>().Bind(configuration.GetSection(BackupOptions.SectionName));
            services.AddOptions<SourceCosmosDBOptions>().Bind(configuration.GetSection(SourceCosmosDBOptions.SectionName));
            services.AddOptions<DestCosmosDBOptions>().Bind(configuration.GetSection(DestCosmosDBOptions.SectionName));

            services.TryAddSingleton<ISourceCosmosClientProvider, CosmosClientProvider<SourceCosmosDBOptions>>();
            services.TryAddSingleton<IDestCosmosClientProvider, CosmosClientProvider<DestCosmosDBOptions>>();

            services.TryAddTransient<ServiceResolver>(p => r => r switch
            {
                CosmosDBRole.Source => p.GetRequiredService<ISourceCosmosClientProvider>(),
                CosmosDBRole.Dest => p.GetRequiredService<IDestCosmosClientProvider>(),
                _ => throw new KeyNotFoundException($"Unsupported cosmos db role: {r}"),
            });

            services.TryAddSingleton<ChangeFeedProcessorManager>();

            return services;
        }
    }
}