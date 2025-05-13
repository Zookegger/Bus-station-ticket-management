using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bus_Station_Ticket_Management.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [DisplayName("Full Name")]
        public string FullName { get; set; }

        [DisplayName("Address")]
        public string? Address { get; set; }

        [DisplayName("Gender")]
        public string Gender { get; set; }

        [DisplayName("Date Of Birth")]
        public DateOnly? DateOfBirth { get; set; }

        [DisplayName("Avatar")]
        public string? Avatar { get; set; }
    }
}
