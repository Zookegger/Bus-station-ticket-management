using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bus_Station_Ticket_Management.Models
{
    public class Vehicle
    {
        [Required]
        public int Id { get; set; }
        public string? Name { get; set; }
        public DateTime AquiredDate { get; set; }
        public string LicensePlate { get; set; }
        public string Status { get; set; }
       
        public int? VehicleTypeId { get; set; }
        [ForeignKey("VehicleTypeId")]

        public VehicleType VehicleType { get; set; }
    }
}
