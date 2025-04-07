using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bus_Station_Ticket_Management.Models
{
    public class VehicleType
    {
        [Required]
        [Key]
        [DisplayName("Mã loại xe")]
        public int Id { get; set; }

        [DisplayName("Tên loại xe")]
        public string Name { get; set; }

        [DisplayName("Giá loại xe")] [DisplayFormat(DataFormatString = "{0:N0} VNĐ")]
        public int Price { get; set; }

        [DisplayName("Tổng số ghế")]
        public int TotalSeats { get; set; }

        [DisplayName("Tổng số tầng")]
        public int TotalFlooring { get; set; }

        [DisplayName("Tổng số hàng")]
        public int TotalRow { get; set; }

        [DisplayName("Tổng số cột")]
        public int TotalColumn { get; set; }
    }
}
