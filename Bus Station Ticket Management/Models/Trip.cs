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
        public Routes? Route { get; set; }

        public List<TripDriverAssignment>? TripDriverAssignments { get; set; }

    }
}
