namespace Bus_Station_Ticket_Management.Services
{
    public interface IEmailBackgroundQueue {
        void QueueEmail(EmailMessage message);
    }
}