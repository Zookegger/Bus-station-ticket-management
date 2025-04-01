using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bus_Station_Ticket_Management.Models
{
    public class Ticket
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }

        public int TripId { get; set; }
        [ForeignKey(nameof(TripId))]
        public Trip Trip { get; set; }

        public int SeatId { get; set; }
        [ForeignKey(nameof(SeatId))]
        public Seat Seat { get; set; }

        public DateTime BookingDate { get; set; }
        public string Status { get; set; }
        //public string PaymentStatus { get; set; }
    }
}
