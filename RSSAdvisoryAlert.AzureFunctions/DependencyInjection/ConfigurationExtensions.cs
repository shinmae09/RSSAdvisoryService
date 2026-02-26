using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;

namespace RSSAdvisoryAlert.AzureFunctions.DependencyInjection
{
    public static class ConfigurationExtensions
    {
        public static void ConfigureAppConfiguration(this FunctionsApplicationBuilder builder)
        {
            builder.Configuration
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("local.settings.json", optional: true)
                .AddEnvironmentVariables();
        }
    }
}
