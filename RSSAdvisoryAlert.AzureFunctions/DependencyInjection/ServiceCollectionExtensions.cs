using Microsoft.Extensions.DependencyInjection;
using RSSAdvisoryAlert.AzureFunctions.Services;
using RSSAdvisoryAlert.Domain.Abstractions.Common;
using RSSAdvisoryAlert.Domain.Abstractions.Services;
using RSSAdvisoryAlert.Domain.Common.Mapper;
using RSSAdvisoryAlert.Domain.DTO;
using RSSAdvisoryAlert.Domain.Entities;

namespace RSSAdvisoryAlert.AzureFunctions.DependencyInjection
{
    public static class ServicesCollectionExtensions
    {
        public static IServiceCollection RegisterApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IRssFeedService, RssFeedService>();
            services.AddScoped<ISalesforceService, SalesforceService>();
            services.AddScoped<IRssFeedEmailService, RssFeedEmailService>();

            services.AddScoped<IRequestMapper<RssFeedDto, RssFeed>, RssFeedRequestMapper>();

            return services;
        }
    } 
}
