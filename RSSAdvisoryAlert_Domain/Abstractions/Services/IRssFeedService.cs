using RSSAdvisoryAlert.Domain.DTO;
using RSSAdvisoryAlert.Domain.Entities;
using RSSAdvisoryAlert.Domain.Models;

namespace RSSAdvisoryAlert.Domain.Abstractions.Services
{
    public interface IRssFeedService
    {
        Task<Dictionary<RssFeed, List<RssFeedItem>>> GetLatestRssFeedItemsAsync();

        List<RssFeedItem> FilterRssFeedItems(List<RssFeedItem> rssFeedItems);

        Task<RssFeed> SaveNewRssFeedSourceAsync(RssFeedDto rssFeedRequest);
    }
}
