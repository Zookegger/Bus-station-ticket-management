using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bus_Station_Ticket_Management.Models
{
    public class Routes
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StartLocation { get; set; }
        [ForeignKey(nameof(StartLocation))]
        public Location StartId { get; set; }

        [Required]
        public int DestinationLocation { get; set; }
        [ForeignKey(nameof(DestinationLocation))]
        public Location DestinationId { get; set; }

        [Required]
        public int Price { get; set; }

        //public int distance_km;
        //public int estimated_duration;
        //public DateTime CreatedAt { get; set; }
        //public DateTime LastUpdatedAt { get; set; }
    }
}
