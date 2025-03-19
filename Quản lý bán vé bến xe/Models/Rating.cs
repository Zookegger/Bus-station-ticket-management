using System.ComponentModel.DataAnnotations;

namespace Bus_Station_Ticket_Management.Models
{
    public class Rating
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public string? Suggestion { get; set; }

        [Range(1, 10)]
        public byte TripRating { get; set; }
        [Range(1, 10)]
        public byte DriverRating { get; set; }
        [Range(1, 10)]
        public byte ServiceRating { get; set; }

        public DateTime RecordedDate { get; set; }
    }
}
