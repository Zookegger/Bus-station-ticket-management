using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

public class TestEmailController : Controller
{
    private readonly IEmailSender _emailSender;

    public TestEmailController(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public async Task<IActionResult> SendTest()
    {
        await _emailSender.SendEmailAsync("nguyenductrung11122004@gmail.com", "Test Email", "<strong>Hello from Bus Station!</strong>");
        return Content("Email sent!");
    }
}
