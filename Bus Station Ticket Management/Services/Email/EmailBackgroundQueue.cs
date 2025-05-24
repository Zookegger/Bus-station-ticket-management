using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using MimeKit;
namespace Bus_Station_Ticket_Management.Services.Email
{
    // This class is used to queue email messages for the background service
    // Use this to send emails to users, not EmailBackgroundService

    public class EmailBackgroundQueue : IEmailBackgroundQueue
    {
        private readonly Channel<MimeMessage> _queue;
        private readonly ILogger<EmailBackgroundQueue> _logger;

        public EmailBackgroundQueue(ILogger<EmailBackgroundQueue> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _queue = Channel.CreateUnbounded<MimeMessage>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
        }

        public async Task QueueEmail(MimeMessage message)
        {
            if (message == null)
            {
                _logger.LogError("Attempted to queue null email message");
                throw new ArgumentNullException(nameof(message));
            }
            try
            {
                await _queue.Writer.WriteAsync(message);
                _logger.LogInformation("Email message queued for {Email}", message.To.FirstOrDefault()?.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to queue email message for {Email}", message.To.FirstOrDefault()?.ToString());
                throw new InvalidOperationException("Failed to queue email message", ex);
            }
        }

        public ChannelReader<MimeMessage> Reader => _queue.Reader;
    }
}