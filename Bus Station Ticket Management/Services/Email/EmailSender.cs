using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace Bus_Station_Ticket_Management.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try {
                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress(
                    _configuration["EmailSender:Name"],
                    _configuration["EmailSender:Email"]
                ));

                _logger.LogInformation("Preparing to send email.");
                _logger.LogInformation("Sender: {SenderName} <{SenderEmail}>", 
                    _configuration["EmailSender:Name"], 
                    _configuration["EmailSender:Email"]);
                _logger.LogInformation("Recipient: {Recipient}", email);
                _logger.LogInformation("Subject: {Subject}", subject);


                mimeMessage.To.Add(new MailboxAddress("", email));
                mimeMessage.Subject = subject;
                mimeMessage.Body = new TextPart("html") { Text = htmlMessage };

                using (var client = new SmtpClient())
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    var host = _configuration["EmailSender:SmtpHost"];
                    var port = int.Parse(_configuration["EmailSender:SmtpPort"]);
                    
                    _logger.LogInformation($"{host}:{port}");

                    _logger.LogInformation("Connecting to SMTP server {Host}:{Port}", host, port);
                    await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);

                    _logger.LogInformation("Authenticating SMTP user.");
                    await client.AuthenticateAsync(
                        _configuration["EmailSender:Username"],
                        _configuration["EmailSender:Password"]
                    );

                    _logger.LogInformation("Sending email...");
                    await client.SendAsync(mimeMessage);
                    _logger.LogInformation("Email sent successfully.");

                    await client.DisconnectAsync(true);
                    _logger.LogInformation("Disconnected from SMTP server.");
                }   
            } catch (Exception ex) {
                _logger.LogError(ex, "Failed to send email to {Recipient}", email);
                throw;
            }
        }
    }
}