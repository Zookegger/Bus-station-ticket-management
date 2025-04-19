using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bus_Station_Ticket_Management.Models
{
    public class Driver : ApplicationUser
    {
        // [DisplayName("Driver ID")]
        // [Key]
        // public int Id { get; set; }

        // [DisplayName("Full Name")]
        // [Required]
        // public string FullName { get; set; }

        // [DisplayName("Address")]
        // public string? Address { get; set; }

        [DisplayName("License ID")]
        [Required]
        public string? LicenseId { get; set; }

        // [DisplayName("Gender")]
        // [Required]
        // public string Gender { get; set; }

        // [DisplayName("Date Of Birth")]
        // [Required]
        // public DateOnly? DateOfBirth { get; set; }

        // [DisplayName("Email")]
        // [Required]
        // public string Email { get; set; }

        // [DisplayName("Phone Number")]
        // [StringLength(maximumLength: 10, MinimumLength = 9)]
        // [Required]
        // public string PhoneNumber { get; set; }

        public ICollection<TripDriverAssignment>? TripDriverAssignments { get; set; }
    }
}
