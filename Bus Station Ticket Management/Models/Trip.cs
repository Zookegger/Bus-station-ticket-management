using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bus_Station_Ticket_Management.Models
{
    public class Trip
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("Two way")]
        public bool IsTwoWay { get; set; }
        
        [Required]
        [DisplayName("Departure Time")]
        public DateTime DepartureTime { get; set; }
        
        [Required]
        [DisplayName("Arrival Time")]
        public DateTime ArrivalTime { get; set; }
        
        [DisplayName("Status")]
        public string? Status { get; set; }

        [DisplayName("Total Price")] [DisplayFormat(DataFormatString = "{0:N0} VNĐ")]
        public int TotalPrice { get; set; }

        [Required]
        [DisplayName("Route Name")]
        public int RouteId { get; set; }
        
        [ForeignKey(nameof(RouteId))]
        public Routes? Route { get; set; }

        [Required]
        [DisplayName("Vehicle Name")]
        public int VehicleId { get; set; }
        
        [ForeignKey(nameof(VehicleId))]
        public Vehicle? Vehicle { get; set; }

        [DisplayName("Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [DisplayName("Last Updated")]
        public DateTime? LastUpdated { get; set; } = DateTime.Now;

        public ICollection<TripDriverAssignment>? TripDriverAssignments { get; set; }
    }
    
}
