﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CADevBackup.ChangeFeedProcessing // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        public delegate ICosmosClientProvider ServiceResolver(CosmosDBRole role);

        static async Task Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddJsonFile("appsettings.jsonc", optional: false, reloadOnChange: true)
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

            services.AddHostedService<ChangeFeedProcessorManager>();
        }
    }
}