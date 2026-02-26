using Edi.SyndicationFeed.ReaderWriter;
using Edi.SyndicationFeed.ReaderWriter.Rss;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using RSSAdvisoryAlert.Domain.Abstractions;
using RSSAdvisoryAlert.Domain.Abstractions.Common;
using RSSAdvisoryAlert.Domain.Abstractions.Services;
using RSSAdvisoryAlert.Domain.Common.Extensions;
using RSSAdvisoryAlert.Domain.DTO;
using RSSAdvisoryAlert.Domain.Entities;
using RSSAdvisoryAlert.Domain.Models;
using System.Xml;

namespace RSSAdvisoryAlert.AzureFunctions.Services
{
    public class RssFeedService : IRssFeedService
    {
        private const int DEFAULT_SECURITY_ALERT_THRESHOLD = 3;

        private readonly IRssFeedRepository _rssFeedRepository;
        private readonly IConfiguration _config;
        private readonly IRequestMapper<RssFeedDto, RssFeed> _mapper;
        private readonly ISalesforceService _salesforceService;

        public RssFeedService(
            IRssFeedRepository rssFeedRepository,
            IConfiguration config,
            IRequestMapper<RssFeedDto, RssFeed> mapper,
            ISalesforceService salesforceService)
        {
            _rssFeedRepository = rssFeedRepository.ThrowIfNull(nameof(rssFeedRepository));
            _config = config.ThrowIfNull(nameof(config));
            _mapper = mapper.ThrowIfNull(nameof(mapper));
            _salesforceService = salesforceService.ThrowIfNull(nameof(salesforceService));
        }

        public async Task<Dictionary<RssFeed, List<RssFeedItem>>> GetLatestRssFeedItemsAsync()
        {
            var latestRssFeedAndItems = new Dictionary<RssFeed, List<RssFeedItem>>();

            var rssFeeds = await _rssFeedRepository.GetAllAsync();
            foreach (var rssFeed in rssFeeds)
            {
                var rssFeedItems = new List<RssFeedItem>();

                using var client = new HttpClient();
                using var stream = await client.GetStreamAsync(rssFeed.URL);
                using var xmlReader = XmlReader.Create(stream);

                var reader = new RssFeedReader(xmlReader);
                while (await reader.Read())
                {
                    if (reader.ElementType == SyndicationElementType.Item)
                    {
                        var item = await reader.ReadItem();

                        var rssFeedItem = new RssFeedItem()
                        {
                            Id = item.Id,
                            Title = item.Title,
                            Description = item.Description,
                            Link = item.Links.FirstOrDefault()?.Uri.OriginalString,
                            PublishDate = item.Published.DateTime,
                        };

                        if (rssFeedItem.Id.Equals(rssFeed.LatestItemId))
                        {
                            //RSS Item already processed.
                            break;
                        }

                        rssFeedItems.Add(rssFeedItem);
                    }
                }

                if (rssFeedItems.Count > 0)
                {
                    rssFeed.LatestItemId = rssFeedItems.First().Id;
                    latestRssFeedAndItems.Add(rssFeed, rssFeedItems);
                }

                rssFeed.LastRun = DateTime.UtcNow;
            }

            await _rssFeedRepository.SaveChangesAsync();

            return latestRssFeedAndItems;
        }

        public List<RssFeedItem> FilterRssFeedItems(List<RssFeedItem> rssFeedItems)
        {
            var filteredRssFeedItems = new List<RssFeedItem>();
            var keywordsJson = _config["FILTER_KEYWORDS"];
            var securityAlertThreshold = _config["SECURITY_ALERT_THRESHOLD"] != null ?
                Convert.ToInt32(_config["SECURITY_ALERT_THRESHOLD"]) :
                DEFAULT_SECURITY_ALERT_THRESHOLD;

            if (!string.IsNullOrWhiteSpace(keywordsJson))
            {
                var keywords = JObject.Parse(keywordsJson);

                foreach (var rssFeedItem in rssFeedItems)
                {
                    if (string.IsNullOrWhiteSpace(rssFeedItem.Description))
                    {
                        continue;
                    }

                    int score = 0;
                    foreach (var keyword in keywords.Properties())
                    {
                        if (rssFeedItem.Description.ToLower().Contains(keyword.Name))
                        {
                            score += Convert.ToInt32(keyword.Value);
                        }
                    }

                    if (score >= securityAlertThreshold)
                    {
                        filteredRssFeedItems.Add(rssFeedItem);
                    }
                }
            }

            return filteredRssFeedItems;
        }

        public async Task<RssFeed> SaveNewRssFeedSourceAsync(RssFeedDto rssFeedRequest)
        {
            rssFeedRequest.ThrowIfNull(nameof(rssFeedRequest));

            var servicerProvider = await _salesforceService.GetServiceProviderAsync(rssFeedRequest.Name);
            if (servicerProvider == null)
            {
                throw new Exception("Invalid Service Provider name.");
            }

            var rssFeed = _mapper.MapToEntity(rssFeedRequest);
            rssFeed.ServiceProviderId = servicerProvider.Id;

            var result = await _rssFeedRepository.AddAsync(rssFeed);

            return result;
        }
    }
}
