using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;

public class EmailSender : IEmailSender
{
    private readonly SmtpSettings _smtpSettings;

    public EmailSender(IOptions<SmtpSettings> smtpSettings)
    {
        _smtpSettings = smtpSettings.Value;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var mailMessage = new MailMessage
        {
            From = new MailAddress(_smtpSettings.SenderEmail, _smtpSettings.SenderName),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true  // This ensures the email body is treated as HTML
        };

        mailMessage.To.Add(new MailAddress(email));

        using (var client = new SmtpClient(_smtpSettings.Server, _smtpSettings.Port))
        {
            client.Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password);
            client.EnableSsl = true;

            await client.SendMailAsync(mailMessage);
        }
    }
}
