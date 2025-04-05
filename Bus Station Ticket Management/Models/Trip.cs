using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bus_Station_Ticket_Management.Models
{
    public class Trip
    {
        [Key]
        public int Id { get; set; }
        //public bool IsTwoWay { get; set; }
        [Required]
        public DateTime DepartureTime { get; set; }
        [Required]
        public DateTime ArrivalTime { get; set; }
        public string? Status { get; set; }
        public int TotalPrice { get; set; }

        public int RouteId { get; set; }
        [ForeignKey(nameof(RouteId))]
        
        public required Routes Route { get; set; }
        [Required]
        public int VehicleId { get; set; }
        [ForeignKey(nameof(VehicleId))]
        public required Vehicle Vehicle { get; set; }

        public ICollection<TripDriverAssignment> TripDriverAssignments { get; set; } = new List<TripDriverAssignment>();
        [NotMapped]
        public string TripDisplayName
        {
            get
            {
                if (Route?.StartLocation?.Name != null && Route?.DestinationLocation?.Name != null)
                {
                    return $"{Route.StartLocation.Name} - {Route.DestinationLocation.Name}";
                }
                return $"Chuyến #{Id}";
            }
        }

    }
    
}
