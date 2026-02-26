using System.ComponentModel.DataAnnotations;

namespace RSSAdvisoryAlert.Domain.DTO
{
    public class RssFeedDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string URL { get; set; } = string.Empty;
    }
}
