using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bus_Station_Ticket_Management.Models
{
    public class Seat
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("Seat Number")]
        public string Number { get; set; }

        [DisplayName("Row")]
        public int Row { get; set; }      // Hàng trong sơ đồ 2D

        [DisplayName("Column")]
        public int Column { get; set; }   // Cột trong sơ đồ 2D

        [DisplayName("Floor")]
        public int Floor { get; set; }    // Tầng (0: tầng dưới, 1: tầng trên, -1: không phân tầng)

        [DisplayName("Seat Status")]
        public bool IsAvailable { get; set; }

        public int? TripId { get; set; }
        
        [ForeignKey("TripId")]
        public Trip? Trip { get; set; }
    
        [DisplayName("Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [DisplayName("Last Updated")]
        public DateTime? LastUpdated { get; set; } = DateTime.Now;
    }
}
