namespace Bus_Station_Ticket_Management.Services
{
    public class EmailMessage
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string HtmlContent { get; set; }
    }
}