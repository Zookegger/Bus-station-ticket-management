using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bus_Station_Ticket_Management.Models
{
    public class Location
    {
        [DisplayName("Location ID")]
        [Required]
        public int Id { get; set; }

        [DisplayName("Location Name")]
        [Required]
        public required string Name { get; set; }

        [DisplayName("Location Address")]
        [Required]
        public required string Address { get; set; }
    }
}
