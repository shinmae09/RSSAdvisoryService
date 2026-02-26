using RSSAdvisoryAlert.Domain.DTO;
using RSSAdvisoryAlert.Domain.Entities;

namespace RSSAdvisoryAlert.Domain.Common.Mapper
{
    public class RssFeedRequestMapper : BaseRequestMapper<RssFeedDto, RssFeed>
    {
        protected override RssFeed CreateEntity(RssFeedDto request)
        {
            return new RssFeed
            {
                Name = request.Name,
                URL = request.URL,
                ServiceProviderId = null,
            };
        }
        protected override void Map(RssFeedDto request, RssFeed entity)
        {
            entity.Name = request.Name;
            entity.URL = request.URL;
        }
    }
}
