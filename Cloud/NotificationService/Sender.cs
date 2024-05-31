
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Diagnostics;
using System.Threading.Tasks;

namespace NotificationService.Mailer
{
    public class Sender
    {
        public static async Task<bool> SendEmail(string message, string to, string subject)
        {
            var apiKey = "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";
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("cloudprojekat2024@gmail.com", "sender");
            var toEmail = new EmailAddress(to, to.Split('@')[0]);
            var plainTextContent = message;
            var htmlContent = "<strong>" + message + "</strong>";
            var msg = MailHelper.CreateSingleEmail(from, toEmail, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
            return response.IsSuccessStatusCode;
        }
    }
}
