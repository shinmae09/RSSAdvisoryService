using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RSSAdvisoryAlert.Domain.Common.Extensions;
using RSSAdvisoryAlert.Infrastructure.Data;

namespace RSSAdvisoryAlert.AzureFunctions.Functions.HttpTriggered;

public class HealthCheckFunction
{
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<HealthCheckFunction> _logger;

    public HealthCheckFunction(
        AppDbContext dbContext,
        IConfiguration configuration,
        ILogger<HealthCheckFunction> logger)
    {
        _dbContext = dbContext.ThrowIfNull(nameof(dbContext));
        _configuration = configuration.ThrowIfNull(nameof(configuration));
        _logger = logger.ThrowIfNull(nameof(logger));
    }

    [Function("HealthCheckFunction")]
    public async Task<string> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "rss/services/health")] HttpRequestData req)
    {
        _logger.LogInformation("Checking SQL Database connection");

        var result = new Dictionary<string, string>();

        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync();
            result["SqlDatabase"] = canConnect ? "healthy" : "Unhealthy";
        }
        catch (Exception ex) 
        {
            _logger.LogError(ex, "SQL Database health check failed.");
            result["SqlDatabase"] = $"Unhealthy: {ex.Message}";
        }

        try
        {
            var kvUri = _configuration["KEY_VAULT_URL"];
            if (string.IsNullOrWhiteSpace(kvUri))
            {
                result["KeyVault"] = "Skipped. KEY_VAULT_URL is not configured";
            }
            else
            {
                var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());
                string secretName = "sql-connection-string";
                KeyVaultSecret secret = await client.GetSecretAsync(secretName);
                result["KeyVault"] = secret != null ? "Healthy" : "Unhealthy";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Key Vault health check failed");
            result["KeyVault"] = $"Unhealthy: {ex.Message}";
        }

        var responseMessage = string.Join("; ", result.Select(kv => $"{kv.Key}: {kv.Value}"));
        _logger.LogInformation(responseMessage);

        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await response.WriteStringAsync(responseMessage);

        return responseMessage;
    }
}