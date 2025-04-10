using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bus_Station_Ticket_Management.Models
{
    public enum DiscountType
    {
        Percentage,
        FixedAmount
    }

    public class Coupon
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string CouponString { get; set; }

        [Required]
        public DiscountType DiscountType { get; set; }

        [Required]
        public decimal DiscountAmount { get; set; }

        [Required]
        public DateTime? StartPeriod { get; set; }
        
        [Required]
        public DateTime? EndPeriod { get; set; }

        public bool IsActive { get; set; }
    }
}