using Bus_Station_Ticket_Management.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

public class TestEmailController : Controller
{
    private readonly EmailBackgroundQueue _emailBackgroundQueue;

    public TestEmailController(EmailBackgroundQueue emailBackgroundQueue)
    {
        _emailBackgroundQueue = emailBackgroundQueue;
    }

    public async Task<IActionResult> SendTest()
    {
        _emailBackgroundQueue.QueueEmail(new EmailMessage
        {
            To = "nguyenductrung11122004@gmail.com",
            Subject = "Test Email",
            HtmlContent = "<strong>Hello from Bus Station!</strong>"
        });
        return Content("Email sent!");
    }
}
