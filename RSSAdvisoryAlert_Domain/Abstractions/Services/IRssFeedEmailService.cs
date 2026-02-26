using RSSAdvisoryAlert.Domain.Entities;
using RSSAdvisoryAlert.Domain.Models;

namespace RSSAdvisoryAlert.Domain.Abstractions.Services
{
    public interface IRssFeedEmailService
    {
        Task SendEmailAsync(IEnumerable<string> emailAddresses, IDictionary<RssFeed, IEnumerable<RssFeedItem>> rssFeeds);
    }
}
