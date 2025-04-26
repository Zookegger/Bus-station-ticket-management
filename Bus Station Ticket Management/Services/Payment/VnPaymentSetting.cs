namespace Bus_Station_Ticket_Management.Services
{

    public class VnPaymentSetting
    {
        public string Version { get; set; } = null!;
        public string Command { get; set; } = null!;
        public string TmnCode { get; set; } = null!;
        public string HashSecret { get; set; } = null!;
        public string CurrCode { get; set; } = null!;
        public string Locale { get; set; } = null!;
        public string OrderType { get; set; } = null!;
        public string ReturnUrl { get; set; } = null!;
        public string BaseUrl { get; set; } = null!;
    }
}
