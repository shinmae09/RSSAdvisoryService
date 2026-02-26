using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using RSSAdvisoryAlert.Domain.Abstractions.Services;
using RSSAdvisoryAlert.Domain.Common.Extensions;
using RSSAdvisoryAlert.Domain.Entities;
using RSSAdvisoryAlert.Domain.Models;

namespace RSSAdvisoryAlert.AzureFunctions.Functions.TimerTriggered;

public class ReadRssFeedFunction
{
    private readonly ILogger _logger;
    private readonly IRssFeedService _rssFeedService;
    private readonly IRssFeedEmailService _rssFeedEmailService;
    private readonly ISalesforceService _salesforceService;

    public ReadRssFeedFunction(
        ILoggerFactory loggerFactory,
        IRssFeedService rssFeedService,
        IRssFeedEmailService rssFeedEmailService,
        ISalesforceService salesforceService)
    {
        _logger = loggerFactory.CreateLogger<ReadRssFeedFunction>();
        _rssFeedService = rssFeedService.ThrowIfNull(nameof(rssFeedService));
        _rssFeedEmailService = rssFeedEmailService.ThrowIfNull(nameof(rssFeedEmailService));
        _salesforceService = salesforceService.ThrowIfNull(nameof(salesforceService));
    }

    [Function("ReadRssFeed")]
    public async Task<IActionResult> Run([TimerTrigger("0 0 */4 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("Rss feed timer trigger function executed at: {executionTime}", DateTime.Now);

        try
        {
            var latestRssFeedAndItems = await _rssFeedService.GetLatestRssFeedItemsAsync();

            var filteredRssFeedAndItems = new Dictionary<RssFeed, IEnumerable<RssFeedItem>>();
            var contactSuscriptions = new Dictionary<string, HashSet<RssFeed>>();
            foreach (var rssFeed in latestRssFeedAndItems)
            {
                var filterResult = _rssFeedService.FilterRssFeedItems(rssFeed.Value);
                if (filterResult.Count > 0)
                {
                    filteredRssFeedAndItems[rssFeed.Key] = filterResult;
                }
                else
                {
                    continue;
                }

                _logger.LogInformation($"Feed Name: {rssFeed.Key.Name} New Items: {rssFeed.Value.Count} Filtered Items: {filterResult.Count}");

                var contacts = await _salesforceService.GetOpportunityContactsAsync(rssFeed.Key.ServiceProviderId);
                foreach (var contact in contacts)
                {
                    var email = contact.Email;
                    if (contactSuscriptions.ContainsKey(email.ToLower()))
                    {
                        contactSuscriptions[email].Add(rssFeed.Key);
                    }
                    else
                    {
                        contactSuscriptions.Add(email.ToLower(), new HashSet<RssFeed>() { rssFeed.Key });
                    }
                }
            }

            var contactSubscriptionGroups = contactSuscriptions
                .GroupBy(c => c.Value, HashSet<RssFeed>.CreateSetComparer())
                .Select(g => new
                {
                    RssFeeds = g.Key.ToList(),
                    EmailAddresses = g.Select(c => c.Key).ToList(),
                });

            foreach (var csg in contactSubscriptionGroups)
            {
                var filteredRssFeedGrouping = filteredRssFeedAndItems
                    .Where(f => csg.RssFeeds.Any(r => r == f.Key))
                    .ToDictionary();
                await _rssFeedEmailService.SendEmailAsync(csg.EmailAddresses, filteredRssFeedGrouping);

                _logger.LogInformation($"Email sent successfully. Rss Feeds: {string.Join(",", csg.RssFeeds.Select(r => r.Name))} Email Address Count: {csg.EmailAddresses.Count}");
            }

            return new OkResult();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while reading and processing RSS feeds. {ex.Message}");
        }
    }
}