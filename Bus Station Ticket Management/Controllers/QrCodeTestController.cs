using Microsoft.AspNetCore.Mvc;
using Bus_Station_Ticket_Management.Services;

namespace Bus_Station_Ticket_Management.Controllers
{
    public class QrCodeTestController : Controller
    {
        private readonly IQrCodeService _qrCodeService;

        public QrCodeTestController(IQrCodeService qrCodeService)
        {
            _qrCodeService = qrCodeService;
        }

        public IActionResult Index()
        {
            return View();
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
    }
}