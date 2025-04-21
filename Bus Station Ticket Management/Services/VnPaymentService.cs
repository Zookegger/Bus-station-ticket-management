using Microsoft.Extensions.Options;
using System.Net;
using Bus_Station_Ticket_Management.Models;

namespace Bus_Station_Ticket_Management.Services
{
    public class VnPaymentServicecs
    {
        VnPaymentSetting setting;
        public VnPaymentServicecs(IOptions<VnPaymentSetting> options)
        {
            setting = options.Value;
        }

        public string ToUrl(Payment obj)
        {
            SortedDictionary<string, string> dict = new SortedDictionary<string, string>{
                {"vnp_Amount", (obj.TotalAmount * 100).ToString()},
                {"vnp_Command", setting.Command},
                {"vnp_CreateDate", obj.CreatedAt.ToString("yyyyMMddHHmmss")},
                {"vnp_CurrCode", setting.CurrCode},
                {"vnp_IpAddr", "127.0.0.1"},
                //{"vnp_IpAddr", Accessor.HttpContext!.Connection.RemoteIpAddress!.ToString()},
                {"vnp_Locale", setting.Locale},
                {"vnp_OrderInfo", $"Payment for {obj.Id} with amount {obj.TotalAmount.ToString()}"},
                {"vnp_OrderType", setting.OrderType},
                {"vnp_ReturnUrl", setting.ReturnUrl},
                {"vnp_TmnCode", setting.TmnCode},
                {"vnp_TxnRef", obj.Id.ToString()},
                {"vnp_Version", setting.Version},
            };
            string text = string.Join("&", dict.Select(p => $"{p.Key}={WebUtility.UrlEncode(p.Value)}"));

            string secureHash = Helper.HashHmac512(text, setting.HashSecret);

            return $"{setting.BaseUrl}?{text}&vnp_SecureHash={secureHash}";

        }
    }
}