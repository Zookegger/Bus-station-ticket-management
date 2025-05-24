namespace Bus_Station_Ticket_Management.Services.Email
{
    public class EmailMessage
    {
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string HtmlMessage { get; set; } = string.Empty;

        // Optional
        public byte[]? InlineImage { get; set; }
        public string? ImageContentId { get; set; }
    }
}