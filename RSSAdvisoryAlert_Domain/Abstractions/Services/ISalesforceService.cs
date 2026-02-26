using RSSAdvisoryAlert.Domain.DTO;

namespace RSSAdvisoryAlert.Domain.Abstractions.Services
{
    public interface ISalesforceService
    {
        Task<IEnumerable<OpportunityContactAlertDto>> GetOpportunityContactsAsync(string serviceProviderId);
        Task<ServiceProviderDto> GetServiceProviderAsync(string name);
    }
}
