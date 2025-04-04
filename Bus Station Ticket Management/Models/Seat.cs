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

        [DisplayName("Hàng")]
        public int Row { get; set; }      // Hàng trong sơ đồ 2D

        [DisplayName("Cột")]
        public int Column { get; set; }   // Cột trong sơ đồ 2D

        [DisplayName("Tầng")]
        public int Floor { get; set; }    // Tầng (0: tầng dưới, 1: tầng trên, -1: không phân tầng)

        [DisplayName("Trạng thái")]
        public bool IsAvailable { get; set; }

        public int? VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public Vehicle? Vehicle { get; set; }
    }
}
