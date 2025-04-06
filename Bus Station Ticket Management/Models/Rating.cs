using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bus_Station_Ticket_Management.Models
{
    public class Rating
    {
        [Required]
        [Key]
        public int Id { get; set; }

        [Required]
        public string CustomerId { get; set; }
        [ForeignKey(nameof(CustomerId))]

        [Required]
        public ApplicationUser User { get; set; }

        [Required]
        public int TripId { get; set; }
        [ForeignKey(nameof(TripId))]
        public Trip Trip { get; set; }

        [DisplayName("Nhận xét")]
        public string? Comment { get; set; }

        [Range(1, 10, ErrorMessage = "Điểm đánh giá phải nằm trong khoảng từ 1 đến 10.")]
        [DisplayName("Điểm chuyến xe")]
        public byte TripRating { get; set; }

        [DisplayName("Ngày tạo ra")]
        public DateTime CreatedAt { get; set; }

    }
}
