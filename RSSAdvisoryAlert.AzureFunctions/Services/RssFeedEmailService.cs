using RSSAdvisoryAlert.Domain.Abstractions.Services;
using RSSAdvisoryAlert.Domain.Entities;
using RSSAdvisoryAlert.Domain.Models;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;

namespace RSSAdvisoryAlert.AzureFunctions.Services
{
    public class RssFeedEmailService : IRssFeedEmailService
    {
        private const string RSS_ADVISORY_EMAIL_TEMPLATE = "RSSAdvisoryAlert.AzureFunctions.Resources.EmailTemplate.html";
        private const string RSS_ADVISORY_EMAIL_SUBJECT = "Security Alert: Important Update for Your Subscribed Service";

        public RssFeedEmailService()
        {

        }

        public async Task SendEmailAsync(IEnumerable<string> emailAddresses, IDictionary<RssFeed, IEnumerable<RssFeedItem>> rssFeeds)
        {
            var emailTemplateHtml = ReadEmailTemplate();

            var parsedContent = new StringBuilder();
            foreach (var item in rssFeeds)
            {
                parsedContent.AppendLine(GetDividerHtml());
                parsedContent.AppendLine(GetServiceProviderHeaderHtml(item.Key.Name));
                parsedContent.AppendLine(GetFeedItemsHtml(item.Value));
            }

            emailTemplateHtml = emailTemplateHtml.Replace("{{PARSED_CONTENT}}", parsedContent.ToString());

            var requestBody = new
            {
                BCC = emailAddresses,
                Subject = RSS_ADVISORY_EMAIL_SUBJECT,
                Content = emailTemplateHtml,
            };

            string url = Environment.GetEnvironmentVariable("MAILTRIGGER_URL") ??
                throw new InvalidOperationException("MAILTRIGGER_URL value is not configured.");

            using (var client = new HttpClient())
            {
                var response = await client.PostAsJsonAsync(url, requestBody);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to send email");
                }
            }
        }

        private string ReadEmailTemplate()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(RSS_ADVISORY_EMAIL_TEMPLATE))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private string GetDividerHtml()
        {
            string divider = @"<hr class=""divider"" style=""border:0; border-top:1px solid #e5e7eb; margin: 20px 0;"">";
            return divider;
        }

        private string GetServiceProviderHeaderHtml(string header)
        {
            string headerHtml = @$"
                <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"">
                    <tr>
                        <td style=""font-weight:700; font-size:12px; letter-spacing:0.8px; color:#333333; padding-bottom:8px;"">
                        {header.ToUpper()}
                        </td>
                    </tr>
                </table>";
            return headerHtml;
        }

        private string GetFeedItemsHtml(IEnumerable<RssFeedItem> rssFeedItems)
        {
            var headerTable = @"
                <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""border-collapse:collapse;"">
                    <tr class=""table-head"">
                        <th align=""left"" style=""font-family: Arial, Helvetica, sans-serif; font-size:13px; color:#666666; padding:6px 0;"">Title</th>
                        <th align=""left"" style=""font-family: Arial, Helvetica, sans-serif; font-size:13px; color:#666666; padding:6px 0;"">Published Date</th>
                        <th align=""left"" style=""font-family: Arial, Helvetica, sans-serif; font-size:13px; color:#666666; padding:6px 0;"">Link</th>
                    </tr>";

            var bodyTable = "";
            foreach (var rssFeedItem in rssFeedItems)
            {
                bodyTable += @$"
                    <tr class=""table-row"">
                        <td style=""font-size:14px; color:#333333; padding:6px 0;"">{rssFeedItem.Title}</td>
                        <td style=""font-size:14px; color:#333333; padding:6px 0;"">{rssFeedItem.PublishDate.ToShortDateString()}</td>
                        <td style=""font-size:14px; color:#1a73e8; padding:6px 0;"">
                            <a href=""{rssFeedItem.Link}"" class=""link-btn"" style=""color:#1a73e8; text-decoration:none;"">Read more</a>
                        </td>
                    </tr>";
            }

            var endTable = "</table>";

            return headerTable + bodyTable + endTable;
        }
    }
}
