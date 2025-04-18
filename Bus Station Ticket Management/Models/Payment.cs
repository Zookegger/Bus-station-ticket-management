using System.ComponentModel.DataAnnotations.Schema;

namespace Bus_Station_Ticket_Management.Models
{
    public class Payment
    {
        public string Id { get; set; }

        public string TicketId { get; set; }
        
        [ForeignKey("TicketId")]
        public Ticket Ticket { get; set; }

        public int Amount { get; set; }
        
        public string PaymentMethod { get; set; }
        
        public byte PaymentStatus { get; set; }
    }
}
