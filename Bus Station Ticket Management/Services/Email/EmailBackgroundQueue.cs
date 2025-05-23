using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace Bus_Station_Ticket_Management.Services
{
    // This class is used to queue email messages for the background service
    // Use this to send emails to users, not EmailBackgroundService

    public class EmailBackgroundQueue : IEmailBackgroundQueue
    {
        private readonly Channel<EmailMessage> _queue;
        private readonly ILogger<EmailBackgroundQueue> _logger;

        public EmailBackgroundQueue(ILogger<EmailBackgroundQueue> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _queue = Channel.CreateUnbounded<EmailMessage>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
        }

        public void QueueEmail(EmailMessage message)
        {
            if (message == null)
            {
                _logger.LogError("Attempted to queue null email message");
                throw new ArgumentNullException(nameof(message));
            }

            if (!_queue.Writer.TryWrite(message))
            {
                _logger.LogError("Failed to queue email message for {Email}", message.To);
                throw new InvalidOperationException("Failed to queue email message");
            }

            _logger.LogInformation("Email message queued for {Email}", message.To);
        }

        public ChannelReader<EmailMessage> Reader => _queue.Reader;
    }
}