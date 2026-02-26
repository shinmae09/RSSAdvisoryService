using RSSAdvisoryAlert.Domain.Abstractions;
using RSSAdvisoryAlert.Domain.Entities;
using RSSAdvisoryAlert.Infrastructure.Data;

namespace RSSAdvisoryAlert.Infrastructure.Database.Repositories
{
    public class RssFeedRepository : BaseRepository<RssFeed>, IRssFeedRepository
    {
        public RssFeedRepository(AppDbContext dbContext)
            : base(dbContext)
        {
        }
    }
}
