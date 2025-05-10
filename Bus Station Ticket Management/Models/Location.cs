using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bus_Station_Ticket_Management.Models
{
    public class Location
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public required string Name { get; set; }

        [Required]
        public required string Address { get; set; }

        // For using map API
        public double? Latitude { get; set; }

        public double? Longitude { get; set; }
        
        [DisplayName("Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [DisplayName("Last Updated")]
        public DateTime? LastUpdated { get; set; } = DateTime.Now;
    }
}
