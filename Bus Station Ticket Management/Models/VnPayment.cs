using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace test.Models
{
        public class VnPayment
        {
            [BindProperty(Name = "vnp_Amount")]
            public int Amount { get; set; }
            [BindProperty(Name = "vnp_BankCode")]
            public string BankCode { get; set; } = null!;
            [BindProperty(Name = "vnp_BankTranNo")]
            public string BankTranNo { get; set; } = null!;
            [BindProperty(Name = "vnp_CardType")]
            public string CardType { get; set; } = null!;
            [BindProperty(Name = "vnp_OrderInfo")]
            public string OrderInfo { get; set; } = null!;
            [BindProperty(Name = "vnp_PayDate")]
            public long PayDate { get; set; } 
            [BindProperty(Name = "vnp_ResponseCode")]
            public string ResponseCode { get; set; } = null!;
            [BindProperty(Name = "vnp_TmnCode")]
            public string TmnCode { get; set; } = null!;
            [BindProperty(Name = "vnp_TransactionNo")]
            [Key]
            public string TransactionNo { get; set; } = null!;
            [BindProperty(Name = "vnp_TransactionStatus")]
            public string TransactionStatus { get; set; } = null!;
            [BindProperty(Name = "vnp_TxnRef")]
            public string TxnRef { get; set; } = null!;
            [BindProperty(Name = "vnp_SecureHash")]
            public string SecureHash { get; set; } = null!;

            
    }
}

//?vnp_Amount=7000000&vnp_BankCode=NCB&vnp_BankTranNo=VNP14916530&vnp_CardType=ATM&vnp_OrderInfo=Payment+for+-1024221969+with+amount+70000&vnp_PayDate=20250420150239&vnp_ResponseCode=00&vnp_TmnCode=G0NOWU5F&vnp_TransactionNo=14916530&vnp_TransactionStatus=00&vnp_TxnRef=-1024221969&vnp_SecureHash=1b8b2b5b316ed638560073e6022f00c259e8aaa664a4f2265dce566750c1076aab234487b9638f46c9b36ceed2af7df4dcd75fecca4e9b5aae4d39a97cf69a86