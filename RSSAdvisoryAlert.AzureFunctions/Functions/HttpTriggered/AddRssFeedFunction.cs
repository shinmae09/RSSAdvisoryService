using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using RSSAdvisoryAlert.Domain.Abstractions.Services;
using RSSAdvisoryAlert.Domain.Common.Extensions;
using RSSAdvisoryAlert.Domain.DTO;
using System.Net;

namespace RSSAdvisoryAlert.AzureFunctions.Functions.HttpTriggered;

public class AddRssFeedFunction
{
    private readonly ILogger<AddRssFeedFunction> _logger;
    private readonly IRssFeedService _rssFeedService;

    public AddRssFeedFunction(
        ILogger<AddRssFeedFunction> logger,
        IRssFeedService rssFeedService)
    {
        _logger = logger.ThrowIfNull(nameof(logger));
        _rssFeedService = rssFeedService.ThrowIfNull(nameof(rssFeedService));
    }

    [Function("AddRssFeedFunction")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "rss/add-new-rss")] HttpRequestData req)
    {
        try
        {
            var rssFeedRequest = await req.ReadFromJsonAsync<RssFeedDto>();

            if (rssFeedRequest == null)
            {
                throw new ArgumentException("Invalid RSS Feed request.");
            }

            var saveNewRssFeedResult = await _rssFeedService.SaveNewRssFeedSourceAsync(rssFeedRequest);

            var response = req.CreateResponse();
            response.StatusCode = HttpStatusCode.OK;
            await response.WriteAsJsonAsync(saveNewRssFeedResult);

            _logger.LogInformation("Successfully saved new RSS Feed source to the database.");

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error was encountered when saving new RSS Feed source to the database. {ex.Message}");
            throw new InvalidOperationException(ex.Message);
        }
    }
}