using Microsoft.EntityFrameworkCore;
using RSSAdvisoryAlert.Domain.Entities;

namespace RSSAdvisoryAlert.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // DbSets
        public DbSet<RssFeed> RssFeeds { get; set; }
    }
}
