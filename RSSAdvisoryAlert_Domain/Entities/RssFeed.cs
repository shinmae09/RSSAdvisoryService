using System.ComponentModel.DataAnnotations.Schema;

namespace RSSAdvisoryAlert.Domain.Entities
{
    [Table("RSS_Feeds")]
    public class RssFeed
    {
        public long Id { get; set; }

        public required string Name { get; set; }

        public required string URL { get; set; }

        public required string ServiceProviderId { get; set; }

        public string? LatestItemId { get; set; }

        public DateTime? LastRun { get; set; }

        public DateTime? LastNotify { get; set; }
    }
}
