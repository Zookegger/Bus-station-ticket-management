using System.Threading.Channels;

namespace Bus_Station_Ticket_Management.Services
{
    public class EmailBackgroundQueue : IEmailBackgroundQueue
    {
        private readonly Channel<EmailMessage> _queue;

        public EmailBackgroundQueue()
        {
            // Unbounded queue, you can also specify limits
            _queue = Channel.CreateUnbounded<EmailMessage>();
        }

        public void QueueEmail(EmailMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            _queue.Writer.TryWrite(message);
        }

        public ChannelReader<EmailMessage> Reader => _queue.Reader;
    }
}