using Microsoft.Extensions.Options;
using System.Net;
using Bus_Station_Ticket_Management.Models;

namespace Bus_Station_Ticket_Management.Services
{
    public class VnPaymentService
    {
        private readonly VnPaymentSetting setting;
        private readonly IConfiguration configuration;

        public VnPaymentService(IOptions<VnPaymentSetting> options, IConfiguration configuration)
        {
            setting = options.Value;
            this.configuration = configuration;
        }

        public string ToUrl(Payment obj, string returnUrl, string ipAddress)
        {
            SortedDictionary<string, string> dict = new SortedDictionary<string, string>{
                {"vnp_Amount", (obj.TotalAmount * 100).ToString()},
                {"vnp_Command", setting.Command},
                { "vnp_CreateDate", obj.CreatedAt.ToString("yyyyMMddHHmmss") },
                {"vnp_CurrCode", setting.CurrCode},
                {"vnp_IpAddr", ipAddress},
                {"vnp_Locale", setting.Locale},
                {"vnp_OrderInfo", $"Payment for {obj.Id} with amount {obj.TotalAmount.ToString()}"},
                {"vnp_OrderType", setting.OrderType},
                {"vnp_ReturnUrl", returnUrl},
                {"vnp_TmnCode", setting.TmnCode},
                {"vnp_TxnRef", obj.Id?.ToString() ?? throw new InvalidOperationException("Payment ID cannot be null.")},
                {"vnp_Version", setting.Version},
            };
            string queryString = string.Join("&", dict.Select(p => $"{p.Key}={WebUtility.UrlEncode(p.Value)}"));

            string hashSecret = this.configuration["Payment:HashSecret"] ?? throw new InvalidOperationException("HashSecret is not configured.");
            string secureHash = Helper.HashHmac512(queryString, hashSecret);

            return $"{setting.BaseUrl}?{queryString}&vnp_SecureHash={secureHash}";

        }
    }
}