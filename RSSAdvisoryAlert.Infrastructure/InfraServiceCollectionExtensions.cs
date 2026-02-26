using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RSSAdvisoryAlert.Domain.Abstractions;
using RSSAdvisoryAlert.Infrastructure.Data;
using RSSAdvisoryAlert.Infrastructure.Database.Repositories;

namespace RSSAdvisoryAlert.Infrastructure
{
    public static class InfraServiceCollectionExtensions
    {
        public static IServiceCollection RegisterInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration["SQL_CONNECTION_STRING"];

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(connectionString, sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 6,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    }));
            }

            // Services
            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            services.AddScoped<IRssFeedRepository, RssFeedRepository>();

            return services;
        }
    }
}
