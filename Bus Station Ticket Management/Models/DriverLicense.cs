using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bus_Station_Ticket_Management.Models
{
    public class DriverLicense
    {
        [DisplayName("Driver ID")]
        [Required]
        [Key]
        public string? DriverId { get; set; }

        [ForeignKey("DriverId")]
        public Driver? Driver { get; set; }

        [DisplayName("License ID")]
        [Required]
        [Key]
        public string? LicenseId { get; set; }

        [DisplayName("License Class")]
        [Required]
        public string? LicenseClass { get; set; }  // A1, A2, A3, A4, B1, B2, C, D, E, F

        [DisplayName("License Issue Date")]
        [Required]
        public DateOnly? LicenseIssueDate { get; set; }

        [DisplayName("License Expiration Date")]
        [Required]
        public DateOnly? LicenseExpirationDate { get; set; }

        [DisplayName("License Issue Place")]
        [Required]
        public string? LicenseIssuePlace { get; set; }

        [DisplayName("Front Image")]
        public string? FrontImg { get; set; }

        [DisplayName("Back Image")]
        public string? BackImg { get; set; }
    }
}
