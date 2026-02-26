using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RSSAdvisoryAlert.AzureFunctions.DependencyInjection;
using RSSAdvisoryAlert.Infrastructure;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureAppConfiguration();
builder.Services.RegisterInfrastructure(builder.Configuration);

builder.Services.RegisterApplicationServices();

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Build().Run();