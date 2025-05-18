using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bus_Station_Ticket_Management.Models
{
    public class Driver
    {
        [Key]
        public string Id { get; set; }
        
        [ForeignKey("Id")]
        public ApplicationUser? Account { get; set; }

        public ICollection<DriverLicense>? DriverLicenses { get; set; }

        public ICollection<TripDriverAssignment>? TripDriverAssignments { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
