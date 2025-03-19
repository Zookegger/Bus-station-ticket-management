using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bus_Station_Ticket_Management.Models
{
    public class Customer
    {
        [DisplayName("Mã Khách Hàng")]
        [Required]
        public int Id { get; set; }

        [DisplayName("Tên Khách Hàng")]
        public string FullName { get; set; }

        [DisplayName("Địa chỉ")]
        public string? Address { get; set; }

        [DisplayName("Giới tính")]
        public string Gender { get; set; }

        [DisplayName("Ngày sinh")]
        public DateTime? DateOfBirth { get; set; }

        [DisplayName("Địa chỉ Email")]
        public string Email { get; set; }

        [DisplayName("Số điện thoại")]
        [StringLength(maximumLength:10, MinimumLength=9)]
        public string PhoneNumber { get; set; }

    }
}
