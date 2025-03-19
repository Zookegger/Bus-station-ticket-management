using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bus_Station_Ticket_Management.Models
{
    public class Seat
    {
        [Required]
        public int Id { get; set; }
        [MaxLength(45)]
        public int Number { get; set; }
        public int Flooring { get; set; }
        public string Status { get; set; }

        public int? VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public Vehicle Vehicle { get; set; }
    }
}
