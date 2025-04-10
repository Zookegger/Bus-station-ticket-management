public interface IEmailBackgroundQueue {
    void QueueEmail(EmailMessage message);
}