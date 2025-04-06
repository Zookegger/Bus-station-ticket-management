using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bus_Station_Ticket_Management.Models
{
    public class Ticket
    {
        [Key]
        public String Id { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }
        public int TripId { get; set; }
        [ForeignKey(nameof(TripId))]
        public Trip Trip { get; set; }
        [Required(ErrorMessage = "Seat number is required")]
        public int SeatId { get; set; }
        [ForeignKey(nameof(SeatId))]
        public Seat Seat { get; set; }
        public DateTime BookingDate { get; set; }
        public bool IsPaid { get; set; }
        public bool IsCanceled { get; set; }
        public DateTime CancelationTime { get; set; }
    }
}
