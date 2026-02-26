namespace RSSAdvisoryAlert.Domain.DTO
{
    public class OpportunityContactAlertDto
    {
        public string Name { get; set; }
        public string Email { get; set; }

        public OpportunityContactAlertDto(string name, string email)
        {
            Name = name;
            Email = email;
        }
    }
}
