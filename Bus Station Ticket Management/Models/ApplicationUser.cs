using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bus_Station_Ticket_Management.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [DisplayName("Họ tên")]
        public string FullName { get; set; }

        [DisplayName("Địa chỉ")]
        public string? Address { get; set; }

        [DisplayName("Giới tính")]
        public string Gender { get; set; }

        [DisplayName("Ngày sinh")]
        public DateOnly? DateOfBirth { get; set; }
    }
}
