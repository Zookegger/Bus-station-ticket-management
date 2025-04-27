using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bus_Station_Ticket_Management.Models
{
    public class Payment
    {
        [Key]
        [Required]
        public string? Id { get; set; } = Guid.NewGuid().ToString();

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime ExpiredAt { get; set; } = DateTime.Now.AddMinutes(15);

        [Required] [DisplayFormat(DataFormatString = "{0:N0} VNĐ")]
        public int TotalAmount { get; set; } // Tổng tiền để thanh toán

        [Required]  
        public string? PaymentMethod { get; set; } = string.Empty;

        public byte PaymentStatus { get; set; } = 0; // 0: Chờ thanh toán, 1: Đã thanh toán, 2: Thất bại

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

        public string? VnPaymentTransactionNo { get; set; }  // Thêm khóa ngoại vào VnPayment
        
        [ForeignKey(nameof(VnPaymentTransactionNo))]
        public VnPayment? VnPayment { get; set; }
    }
}
