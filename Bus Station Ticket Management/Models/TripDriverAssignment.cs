using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Bus_Station_Ticket_Management.Models
{
    public class TripDriverAssignment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TripId { get; set; }
        [ForeignKey(nameof(TripId))]
        public Trip? Trip { get; set; }

        [Required]
        public int DriverId { get; set; }
        [ForeignKey(nameof(DriverId))]
        public Driver? Driver { get; set; }

        [Required] [ValidateNever] [DisplayName("Date Assigned")]
        public DateTime DateAssigned { get; set; }
    }
}
