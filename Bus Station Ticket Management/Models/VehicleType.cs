using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bus_Station_Ticket_Management.Models
{
    public class VehicleType
    {
        [Required]
        [Key]
        [DisplayName("Id")]
        public int Id { get; set; }

        [DisplayName("Type Name")]
        public string Name { get; set; }

        [DisplayName("Based Fare")] [DisplayFormat(DataFormatString = "{0:N0} VNĐ")]
        public int Price { get; set; }

        [DisplayName("Total Seats")]
        public int TotalSeats { get; set; }

        [DisplayName("Total Floorings")]
        public int TotalFlooring { get; set; }

        [DisplayName("Total Rows")]
        public int TotalRow { get; set; }

        [DisplayName("Total Columns")]
        public int TotalColumn { get; set; }

        [DisplayName("Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [DisplayName("Last Updated")]
        public DateTime? LastUpdated { get; set; } = DateTime.Now;
    }
}
