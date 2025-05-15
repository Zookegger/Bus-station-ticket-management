    using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bus_Station_Ticket_Management.Models
{
    public class Driver : ApplicationUser
    {
        public ICollection<DriverLicense>? DriverLicenses { get; set; }

        public ICollection<TripDriverAssignment>? TripDriverAssignments { get; set; }
    }
}
