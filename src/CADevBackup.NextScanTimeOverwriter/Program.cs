using CADevBackup.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace CADevBackup.NextScanTimeOverwriter
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Make this an option
            DateTime minNextScanTime = DateTime.UtcNow.AddMinutes(30);

            // Configuration
            IConfiguration configuration = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .AddJsonFile("appsettings.jsonc", optional: false, reloadOnChange: true)
                .Build();

            IServiceCollection serviceCollection = new ServiceCollection();
            // Logging
            serviceCollection.AddLogging(logging =>
            {
                logging.AddConfiguration(configuration);
                logging.AddSimpleConsole(opt =>
                {
                    opt.SingleLine = true;
                });

                logging.AddFile();
            });

            using (ServiceProvider provider = RegisterServices(serviceCollection, configuration).BuildServiceProvider())
            {
                Overwriter engine = provider.GetRequiredService<Overwriter>();
                await engine.RunAsync(DateTime.UtcNow.AddMinutes(30), TimeSpan.FromHours(6), default);
            }
        }

        private static IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<TargetScheduleOptions>().Bind(configuration.GetSection(TargetScheduleOptions.SectionName));
            services.TryAddCADevBackupCoreServices();
            services.TryAddSingleton<Overwriter>();
            return services;
        }
    }
}