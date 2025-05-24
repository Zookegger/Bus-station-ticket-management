using System.Threading.Channels;
using MimeKit;

namespace Bus_Station_Ticket_Management.Services.Email
{
    public interface IEmailBackgroundQueue {
        Task QueueEmail(MimeMessage message);
        ChannelReader<MimeMessage> Reader { get; }
    }
}