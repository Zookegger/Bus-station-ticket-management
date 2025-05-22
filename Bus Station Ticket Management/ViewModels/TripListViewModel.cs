using Bus_Station_Ticket_Management.Models;

namespace Bus_Station_Ticket_Management.ViewModels {
    public class TripListViewModel {
        public List<Trip>? TripsList { get; set; }
        public bool IsSearchResult { get; set; } = false;

        public string? Departure { get; set; }
        public string? Destination { get; set; }
        public DateOnly? DepartureTime { get; set; }

        public List<Coupon>? Coupons { get; set; }
    }
}