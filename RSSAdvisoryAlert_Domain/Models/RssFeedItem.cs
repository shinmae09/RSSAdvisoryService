namespace RSSAdvisoryAlert.Domain.Models
{
    public class RssFeedItem
    {
        public required string Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public string? Link { get; set; }
        public DateTime PublishDate { get; set; }
    }
}
