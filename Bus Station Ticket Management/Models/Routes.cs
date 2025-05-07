using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bus_Station_Ticket_Management.Models
{
    public class Routes
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("Starting Point")]
        [Required]
        public int StartId { get; set; }
        [ForeignKey(nameof(StartId))]
        public Location? StartLocation { get; set; }

        [DisplayName("Destination Point")]
        [Required]
        public int DestinationId { get; set; }
        [ForeignKey(nameof(DestinationId))]
        public Location? DestinationLocation { get; set; }

        [DisplayName("Base Fare")]
        [Required]
        public int Price { get; set; }

        [DisplayName("Distance")]
        public double Distance { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastUpdatedAt { get; set; }
    }
}
