using Microsoft.AspNetCore.WebUtilities;
using RSSAdvisoryAlert.Domain.Abstractions.Services;
using RSSAdvisoryAlert.Domain.DTO;
using System.Net.Http.Json;

namespace RSSAdvisoryAlert.AzureFunctions.Services
{
    public class SalesforceService : ISalesforceService
    {
        public SalesforceService() { }

        public async Task<IEnumerable<OpportunityContactAlertDto>> GetOpportunityContactsAsync(string serviceProviderId)
        {
            string url = Environment.GetEnvironmentVariable("SALEFORCE_GET_OPPORTUNITY_CONTACTS_URL") ??
                throw new InvalidOperationException("SALEFORCE_GET_OPPORTUNITY_CONTACTS_URL value is not configured.");
            var formattedUrl = url.Replace("{serviceProviderId}", serviceProviderId);

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(formattedUrl);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to retrieve the opportunity contacts for Service Provider ID: {serviceProviderId}");
                }

                var contacts = await response.Content.ReadFromJsonAsync<IEnumerable<OpportunityContactAlertDto>>() 
                    ?? Enumerable.Empty<OpportunityContactAlertDto>();

                return contacts;
            }
        }

        public async Task<ServiceProviderDto> GetServiceProviderAsync(string name)
        {
            string url = Environment.GetEnvironmentVariable("SALEFORCE_GET_SERVICE_PROVIDER_URL") ??
                throw new InvalidOperationException("SALEFORCE_GET_SERVICE_PROVIDER_URL value is not configured.");

            var updatedUrl = QueryHelpers.AddQueryString(url, new Dictionary<string, string?>
            {
                ["name"] = name,
            });

            using (var client = new HttpClient())
            {                
                var response = await client.GetAsync(updatedUrl);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to retrieve the Service Provider: {name}");
                }

                var serviceProvider = await response.Content.ReadFromJsonAsync<ServiceProviderDto>() ??
                    throw new InvalidOperationException("No service provider retrieved.");

                return serviceProvider;
            }
        }
    }
}
