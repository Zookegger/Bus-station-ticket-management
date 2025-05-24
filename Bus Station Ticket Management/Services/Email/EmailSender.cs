using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace Bus_Station_Ticket_Management.Services.Email
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;
        private readonly IEmailBackgroundQueue _emailQueue;

        public EmailSender(
            IConfiguration configuration, 
            ILogger<EmailSender> logger,
            IEmailBackgroundQueue emailQueue)
        {
            _configuration = configuration;
            _logger = logger;
            _emailQueue = emailQueue;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            await SendEmailAsync(email, subject, htmlMessage, null, null);
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage, byte[]? inlineImage = null, string? imageContentId = null)
        {
            try {
                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress(
                    _configuration["EmailSender:Name"],
                    _configuration["EmailSender:Email"]
                ));

                _logger.LogInformation("Preparing to queue email.");
                _logger.LogInformation("Sender: {SenderName} <{SenderEmail}>", 
                    _configuration["EmailSender:Name"], 
                    _configuration["EmailSender:Email"]);
                _logger.LogInformation("Recipient: {Recipient}", email);
                _logger.LogInformation("Subject: {Subject}", subject);

                mimeMessage.To.Add(new MailboxAddress("", email));
                mimeMessage.Subject = subject;
                
                var builder = new BodyBuilder();
                builder.HtmlBody = htmlMessage;
                mimeMessage.Body = builder.ToMessageBody();

                if (inlineImage != null && !string.IsNullOrEmpty(imageContentId)) {
                    var image = builder.LinkedResources.Add(imageContentId, inlineImage);
                    image.ContentId = imageContentId;
                    mimeMessage.Body = builder.ToMessageBody();
                }

                await _emailQueue.QueueEmail(mimeMessage);
                _logger.LogInformation("Email queued successfully for {Recipient}", email);
            } catch (Exception ex) {
                _logger.LogError(ex, "Failed to queue email for {Recipient}", email);
                throw;
            }
        }
    }
}