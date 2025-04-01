using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bus_Station_Ticket_Management.Models
{
    public class Seat
    {
        [Key]
        public int Id { get; set; }
        
        [DisplayName("Số ghế")]
        public string Number { get; set; }
        
        [DisplayName("Trạng thái")]
        public bool IsAvailable { get; set; }

        public int? VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public Vehicle? Vehicle { get; set; }
    }
}
