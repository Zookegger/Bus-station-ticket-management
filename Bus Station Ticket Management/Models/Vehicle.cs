using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bus_Station_Ticket_Management.Models
{
    public class Vehicle
    {
        [Key]
        [DisplayName("Id")]
        public int Id { get; set; }

        [DisplayName("Codename")] [Required]
        public string? Name { get; set; }

        [DisplayName("Acquired Date")]
        public DateTime AcquiredDate { get; set; } = DateTime.Now;

        [DisplayName("Last Updated")]
        public DateTime? LastUpdated { get; set; }

        [DisplayName("License Plate")] [Required]
        public string? LicensePlate { get; set; }

        [DisplayName("Status")] [Required]
        public string? Status { get; set; }

        [DisplayName("Vehicle Type")] [Required]
        public int? VehicleTypeId { get; set; }

        [ForeignKey(nameof(VehicleTypeId))]
        public VehicleType? VehicleType { get; set; }
    }
}
