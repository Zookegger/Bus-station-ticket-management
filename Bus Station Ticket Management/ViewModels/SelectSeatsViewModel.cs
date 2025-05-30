using Bus_Station_Ticket_Management.Models;

namespace Bus_Station_Ticket_Management.ViewModels
{
    public class SelectSeatsViewModel
    {
        public int TripId { get; set; }
        public int VehicleId { get; set; }
        public string? RouteName { get; set; }
        public string? DepartureLocation { get; set; }
        public string? DepartureLocationAddress { get; set; }
        public string? DestinationLocation { get; set; }
        public string? DestinationLocationAddress { get; set; }
        public DateTime DepartureTime { get; set; }
        public string? VehicleName { get; set; }
        public string? LicensePlate { get; set; }
        public int Price { get; set; }
        public ApplicationUser? User { get; set; } 
        public List<Seat> Seats { get; set; } = new();
        public int TotalSeats { get; set; }
        public int TotalColumns { get; set; }
        public List<int> RowsPerFloor { get; set; } = [];
        public int TotalFloors { get; set; }
    }
}
