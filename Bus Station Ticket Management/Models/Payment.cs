using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using test.Models;

namespace Bus_Station_Ticket_Management.Models
{
    public class Payment
    {
        [Key]
        public string Id { get; set; }
        public string userId { get; set; }
        [ForeignKey("userId")]
        public ApplicationUser? User { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int TotalAmount { get; set; } // Tổng tiền để thanh toán

        public string PaymentMethod { get; set; } = "VNPay"; // Mặc định là VNPay

        public byte PaymentStatus { get; set; } = 0; // 0: Chờ thanh toán, 1: Đã thanh toán, 2: Thất bại

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

        public string? VnPaymentTransactionNo { get; set; }  // Thêm khóa ngoại vào VnPayment
        [ForeignKey(nameof(VnPaymentTransactionNo))]
        public VnPayment? VnPayment { get; set; }
    }
}
