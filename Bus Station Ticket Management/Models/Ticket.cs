using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bus_Station_Ticket_Management.Models
{
    public class Ticket
    {
        [Key]
        public string? Id { get; set; } = Guid.NewGuid().ToString();

        public string? UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        public int TripId { get; set; }
        [ForeignKey(nameof(TripId))] 
        [ValidateNever]
        public Trip? Trip { get; set; }

        [Required(ErrorMessage = "Seat number is required")]
        public int SeatId { get; set; }
        [ForeignKey(nameof(SeatId))]
        [ValidateNever]
        public Seat? Seat { get; set; }

        public DateTime BookingDate { get; set; }

        [Display(Name = "Final Pricing")]
        public long TotalPrice { get; set; }

        public bool IsPaid { get; set; }

        public bool IsReserved { get; set; }

        public bool IsCanceled { get; set; }

        public DateTime? CancelationTime { get; set; }
    
        public int? CouponId { get; set; }
        [ForeignKey(nameof(CouponId))]
        public Coupon? Coupon { get; set; }

        public string? GuestName { get; set; }

        public string? GuestEmail { get; set; }

        public string? GuestPhone { get; set; }

        public string? PaymentId { get; set; }
        [ForeignKey(nameof(PaymentId))] 
        [ValidateNever]
        public Payment? Payment { get; set; }

        public string? VnPaymentTransactionNo { get; set; }  // Thêm khóa ngoại vào VnPayment
        [ForeignKey(nameof(VnPaymentTransactionNo))]
        public VnPayment? VnPayment { get; set; }

    }
}