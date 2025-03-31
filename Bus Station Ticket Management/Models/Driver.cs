using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bus_Station_Ticket_Management.Models
{
    public class Driver
    {
        [DisplayName("Mã Tài Xế")]
        [Key]
        public int Id { get; set; }

        [DisplayName("Tên Tài Xế")]
        [Required]
        public string FullName { get; set; }

        [DisplayName("Địa chỉ")]
        public string? Address { get; set; }

        [DisplayName("Mã GPLX")]
        [Required]
        public string LicenseId { get; set; }

        [DisplayName("Giới tính")]
        [Required]
        public string Gender { get; set; }

        [DisplayName("Ngày sinh")]
        [Required]
        public DateTime? DateOfBirth { get; set; }

        [DisplayName("Địa chỉ Email")]
        [Required]
        public string Email { get; set; }

        [DisplayName("Số điện thoại")]
        [StringLength(maximumLength: 10, MinimumLength = 9)]
        [Required]
        public string PhoneNumber { get; set; }

        public ICollection<TripDriverAssignment>? TripDriverAssignments { get; set; }
    }
}
