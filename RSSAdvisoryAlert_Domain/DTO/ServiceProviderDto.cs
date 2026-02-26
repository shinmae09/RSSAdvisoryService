namespace RSSAdvisoryAlert.Domain.DTO
{
    public class ServiceProviderDto
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public ServiceProviderDto(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
