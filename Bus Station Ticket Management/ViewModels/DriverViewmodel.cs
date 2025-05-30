using System.ComponentModel;
using Bus_Station_Ticket_Management.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
namespace Bus_Station_Ticket_Management.ViewModels {
    public class DriverViewModel {
        [DisplayName("Driver ID")]
        [Key]
        public string? DriverId { get; set; }

        [DisplayName("Full Name")]
        [Required]
        public string? FullName { get; set; }

        [DisplayName("Address")]
        [Required]
        public string? Address { get; set; }
        
        [DisplayName("Email")]
        [Required]
        public string? Email { get; set; }

        [DisplayName("Phone Number")]
        [Required]
        public string? PhoneNumber { get; set; }

        [DisplayName("Gender")]
        [Required]
        public string? Gender { get; set; }

        [DisplayName("Date Of Birth")]
        [Required]
        public DateOnly? DateOfBirth { get; set; }

        [DisplayName("Avatar")]
        [ValidateNever]
        [Required]
        public string? Avatar { get; set; }

        [DisplayName("Licenses")]
        [Required]
        public List<DriverLicenseViewModel>? Licenses { get; set; } = [];

        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }

    public class DriverLicenseViewModel {
        [DisplayName("License ID")]
        [Required]
        public string? LicenseId { get; set; }
        
        [DisplayName("License Class")]
        [Required]
        public string? LicenseClass { get; set; }  // A1, A2, A3, A4, B1, B2, C, D, E, F

        [DisplayName("License Issue Date")]
        [Required]
        public DateOnly? LicenseIssueDate { get; set; }

        [DisplayName("License Expiration Date")]
        public DateOnly? LicenseExpirationDate { get; set; }

        public bool hasExpDate { get; set; }

        [DisplayName("License Issue Place")]
        [Required]
        public string? LicenseIssuePlace { get; set; }
        
        [DisplayName("Front Image")]
        public string? FrontImg { get; set; }

        [DisplayName("Back Image")]
        public string? BackImg { get; set; }
    }
}