using System.Threading.Channels;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Bus_Station_Ticket_Management.Services
{
    public class EmailBackgroundService : BackgroundService
    {
        private readonly ChannelReader<EmailMessage> _reader;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<EmailBackgroundService> _logger;

        public EmailBackgroundService(
            IEmailBackgroundQueue queue,
            IEmailSender emailSender,
            ILogger<EmailBackgroundService> logger
        )
        {
            _reader = (queue as EmailBackgroundQueue)?.Reader ?? throw new ArgumentNullException(nameof(queue));
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await foreach (var message in _reader.ReadAllAsync(stoppingToken))
                {
                    try
                    {
                        await _emailSender.SendEmailAsync(message.To, message.Subject, message.HtmlContent);
                        _logger.LogInformation("Email sent successfully to {Email}", message.To);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending email to {Email}", message.To);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EmailBackgroundService");
            }
        }
    }
}