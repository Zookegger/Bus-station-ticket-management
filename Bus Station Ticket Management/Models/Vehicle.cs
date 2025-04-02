using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bus_Station_Ticket_Management.Models
{
    public class Vehicle
    {
        [Required]
        [Key]
        [DisplayName("Mã xe")]
        public int Id { get; set; }
        [DisplayName("Tên xe")]
        public string? Name { get; set; }
        [DisplayName("Ngày nhập vào")]
        public DateTime AcquiredDate { get; set; } = DateTime.Now;  // Default to now

        [DisplayName("Biển số xe")]
        public string LicensePlate { get; set; }
        [DisplayName("Trạng thái")]
        public string Status { get; set; }
        
        [DisplayName("Loại xe")]        
        public int? VehicleTypeId { get; set; }

        [ForeignKey(nameof(VehicleTypeId))]
        public VehicleType? VehicleType { get; set; }
    }
}
