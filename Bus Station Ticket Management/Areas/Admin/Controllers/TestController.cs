using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Bus_Station_Ticket_Management.Services.QRCode;
using Bus_Station_Ticket_Management.Services.Email;
using MimeKit;
using Microsoft.AspNetCore.Authorization;

namespace Bus_Station_Ticket_Management.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class TestController : Controller
    {
        private readonly IEmailBackgroundQueue _emailBackgroundQueue;
        private readonly ILogger<TestController> _logger;
        private readonly IQrCodeService _qrCodeService;

        public TestController(IEmailBackgroundQueue emailBackgroundQueue, ILogger<TestController> logger, IQrCodeService qrCodeService)
        {
            _emailBackgroundQueue = emailBackgroundQueue;
            _logger = logger;
            _qrCodeService = qrCodeService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult BasicEmail()
        {
            return View("~/Areas/Admin/Views/Test/Email/BasicEmail.cshtml");
        }

        public IActionResult HtmlEmail()
        {
            return View("~/Areas/Admin/Views/Test/Email/HtmlEmail.cshtml");
        }

        public IActionResult EmailWithImage()
        {
            return View("~/Areas/Admin/Views/Test/Email/EmailWithImage.cshtml");
        }

        public IActionResult EmailWithQRCode()
        {
            return View("~/Areas/Admin/Views/Test/Email/EmailWithQRCode.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> SendBasicEmail(string email, string subject, string message)
        {
            try
            {
                var mimeMessage = new MimeMessage();
                mimeMessage.To.Add(new MailboxAddress("", email));
                mimeMessage.Subject = subject;
                mimeMessage.Body = new TextPart("html") { Text = message };
                await _emailBackgroundQueue.QueueEmail(mimeMessage);
                TempData["Success"] = "Basic email sent successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending basic email");
                TempData["Error"] = "Failed to send email: " + ex.Message;
            }
            return RedirectToAction(nameof(BasicEmail));
        }

        [HttpPost]
        public async Task<IActionResult> SendHtmlEmail(string email, string subject, string message)
        {
            try
            {
                var mimeMessage = new MimeMessage();
                mimeMessage.To.Add(new MailboxAddress("", email));
                mimeMessage.Subject = subject;
                mimeMessage.Body = new TextPart("html") { Text = message };
                await _emailBackgroundQueue.QueueEmail(mimeMessage);
                TempData["Success"] = "HTML email sent successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending HTML email");
                TempData["Error"] = "Failed to send email: " + ex.Message;
            }
            return RedirectToAction(nameof(HtmlEmail));
        }

        [HttpPost]
        public async Task<IActionResult> SendEmailWithImage(string email, string subject, string message, IFormFile image)
        {
            try
            {
                if (image == null || image.Length == 0)
                {
                    TempData["Error"] = "Please select an image to embed.";
                    return RedirectToAction(nameof(EmailWithImage));
                }

                using var ms = new MemoryStream();
                await image.CopyToAsync(ms);
                var imageBytes = ms.ToArray();

                // Create a unique content ID for the image
                var contentId = Guid.NewGuid().ToString();

                // Add image reference to the message
                var htmlMessage = $@"
                    <div>
                        {message}
                        <br/>
                        <img src='cid:{contentId}' alt='Embedded Image' style='max-width: 100%;' />
                    </div>";

                var mimeMessage = new MimeMessage();
                mimeMessage.To.Add(new MailboxAddress("", email));
                mimeMessage.Subject = subject;
                mimeMessage.Body = new TextPart("html") { Text = htmlMessage };
                await _emailBackgroundQueue.QueueEmail(mimeMessage);
                TempData["Success"] = "Email with image sent successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email with image");
                TempData["Error"] = "Failed to send email: " + ex.Message;
            }
            return RedirectToAction(nameof(EmailWithImage));
        }

        [HttpPost]
        public async Task<IActionResult> SendEmailWithQRCode(string email, string subject, string message, string qrContent, int qrSize)
        {
            try
            {
                // Generate QR code using the service
                var qrCodeBase64 = _qrCodeService.GenerateQrCode(qrContent);
                var qrCodeBytes = Convert.FromBase64String(qrCodeBase64);

                // Create a unique content ID for the QR code
                var contentId = MimeKit.Utils.MimeUtils.GenerateMessageId();

                // Add QR code reference to the message
                var htmlMessage = $@"
                    <div>
                        {message}
                        <br/>
                        <img src='cid:{contentId}' alt='QR Code' style='max-width: 100%;' />
                    </div>";

                var mimeMessage = new MimeMessage();
                mimeMessage.To.Add(new MailboxAddress("", email));
                mimeMessage.Subject = subject;
                mimeMessage.Body = new TextPart("html") { Text = htmlMessage };

                var builder = new BodyBuilder{
                    HtmlBody = htmlMessage
                };

                var image = builder.LinkedResources.Add("qrcode.png", qrCodeBytes);
                image.ContentId = contentId;
                image.ContentDisposition = new ContentDisposition(ContentDisposition.Inline);
                image.ContentType.MediaType = "image";
                image.ContentType.MediaSubtype = "png";
                
                mimeMessage.Body = builder.ToMessageBody();

                await _emailBackgroundQueue.QueueEmail(mimeMessage);

                TempData["Success"] = "Email with QR code sent successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email with QR code");
                TempData["Error"] = "Failed to send email: " + ex.Message;
            }
            return RedirectToAction(nameof(EmailWithQRCode));
        }

        [HttpPost]
        public IActionResult GenerateQrCode(string ticketId)
        {
            if (string.IsNullOrEmpty(ticketId))
            {
                return BadRequest("Ticket ID is required");
            }

            var qrCodeBase64 = _qrCodeService.GenerateQrCode(ticketId);
            return Json(new { qrCode = qrCodeBase64 });
        }

        public IActionResult GenerateQrCode()
        {
            return View("~/Areas/Admin/Views/Test/Miscellaneous/GenerateQrCode.cshtml");
        }

        public IActionResult Vehicle()
        {
            return View("~/Areas/Admin/Views/Test/Api/VehicleApi.cshtml");
        }
        
        public IActionResult Assignments()
        {
            return View("~/Areas/Admin/Views/Test/Api/TripDriverAssignmentApi.cshtml");
        }

    }
} 