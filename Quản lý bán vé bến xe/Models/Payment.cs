using System.ComponentModel.DataAnnotations.Schema;

namespace Bus_Station_Ticket_Management.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        [ForeignKey("TicketId")]
        public Ticket Ticket { get; set; }

        public int Amount { get; set; }
        public string PaymentMethod { get; set; }
        public byte PaymentStatus { get; set; }

    }
}
