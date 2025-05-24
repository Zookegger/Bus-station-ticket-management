using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;
using Microsoft.AspNetCore.Identity.UI.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Bus_Station_Ticket_Management.Services.Email
{
    public class EmailBackgroundService : BackgroundService
    {
        private readonly IEmailBackgroundQueue _emailQueue;
        private readonly ILogger<EmailBackgroundService> _logger;
        private readonly IConfiguration _configuration;

        public EmailBackgroundService(
            IEmailBackgroundQueue emailQueue,
            IConfiguration configuration,
            ILogger<EmailBackgroundService> logger)
        {
            _emailQueue = emailQueue;
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Wait for a message to be available in the queue
                    var message = await _emailQueue.Reader.ReadAsync(stoppingToken);
                    _logger.LogInformation("Processing email for {Recipient}", message.To.FirstOrDefault()?.ToString());

                    // Ensure the message has a sender
                    if (!message.From.Any())
                    {
                        message.From.Add(new MailboxAddress(
                            _configuration["EmailSender:Name"],
                            _configuration["EmailSender:Email"]
                        ));
                    }

                    using (var client = new SmtpClient())
                    {
                        client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                        var host = _configuration["EmailSender:SmtpHost"];
                        var port = int.Parse(_configuration["EmailSender:SmtpPort"]);

                        _logger.LogInformation("Connecting to SMTP server {Host}:{Port}", host, port);
                        await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);

                        _logger.LogInformation("Authenticating SMTP user.");
                        await client.AuthenticateAsync(
                            _configuration["EmailSender:Username"],
                            _configuration["EmailSender:Password"]
                        );

                        _logger.LogInformation("Sending email...");
                        await client.SendAsync(message);
                        _logger.LogInformation("Email sent successfully.");

                        await client.DisconnectAsync(true);
                        _logger.LogInformation("Disconnected from SMTP server.");
                    }
                }
                catch (OperationCanceledException)
                {
                    // Normal shutdown
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing email queue");
                    // Wait a bit before retrying
                    await Task.Delay(5000, stoppingToken);
                }
            }

            _logger.LogInformation("Email Background Service is stopping.");
        }
    }
}