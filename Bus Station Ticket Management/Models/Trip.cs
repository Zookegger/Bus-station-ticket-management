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
        public Routes Route { get; set; }

        public int VehicleId { get; set; }
        [ForeignKey(nameof(VehicleId))]
        public Vehicle Vehicle { get; set; }

        public ICollection<TripDriverAssignment> TripDriverAssignments { get; set; }

    }
}
