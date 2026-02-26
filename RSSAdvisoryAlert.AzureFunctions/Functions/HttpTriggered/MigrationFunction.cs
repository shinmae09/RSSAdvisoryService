using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RSSAdvisoryAlert.Infrastructure.Data;
using RSSAdvisoryAlert.Infrastructure.Database;
using System.Net;

namespace RSSAdvisoryAlert.AzureFunctions.Functions.HttpTriggered;

public class MigrationFunction
{
    private readonly AppDbContext _db;
    private readonly ILogger<MigrationFunction> _logger;

    public MigrationFunction(AppDbContext db, ILogger<MigrationFunction> logger)
    {
        _db = db;
        _logger = logger;
    }

    [Function("RunMigrations")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "rss/run-migrations")]
        HttpRequestData req)
    {
        try
        {
            _logger.LogInformation("Starting database migrations");

            if (Environment.GetEnvironmentVariable("ENABLE_DB_MIGRATIONS") != "true")
            {
                var forbidden = req.CreateResponse(HttpStatusCode.Forbidden);
                await forbidden.WriteStringAsync("Migrations disabled");
                return forbidden;
            }

            _logger.LogInformation("Migration request received");
            await MigrationLock.AcquireAsync(_db);

            if (!await MigrationGuard.HasPendingMigrationsAsync(_db))
            {
                var ok = req.CreateResponse(HttpStatusCode.OK);
                await ok.WriteStringAsync("No pending migrations");
                return ok;
            }

            _logger.LogInformation("Applying EF Core migrations");
            await _db.Database.MigrateAsync();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("Migrations applied successfully");

            return response;
        }
        catch (Exception ex) 
        {
            _logger.LogError(ex, "Migration failed");

            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync($"Migration failed: {ex.Message}");
            return response;
        }
    }
}
