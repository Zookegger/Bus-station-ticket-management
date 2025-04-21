using Bus_Station_Ticket_Management.DataAccess;
using Bus_Station_Ticket_Management.Models;
using Bus_Station_Ticket_Management.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;
using System.Web;
using test.Models;

namespace Bus_Station_Ticket_Management.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly VnPaymentServicecs vnPayment;
        private readonly VnPaymentSetting vnPaysetting;
        private readonly ILogger<CartController> _logger;

        public CartController(VnPaymentServicecs vnPayment, ApplicationDbContext context, IOptions<VnPaymentSetting> vnPaymentSettings, ILogger<CartController> logger)
        {
            this.vnPayment = vnPayment;
            this.context = context;
            this.vnPaysetting = vnPaymentSettings.Value;
            this._logger = logger;
        }

        public IActionResult Checkout()
        {
            _logger.LogInformation("Checkout called");
            try
            {
                var paymentId = TempData["PaymentId"]?.ToString();
                if (string.IsNullOrEmpty(paymentId))
                {
                    _logger.LogWarning("No Payment ID found in TempData");
                    return BadRequest("No payment information available.");
                }

                var payment = context.Payments.FirstOrDefault(p => p.Id == paymentId);
                if (payment == null)
                {
                    _logger.LogWarning($"Payment with ID {paymentId} not found in database");
                    return BadRequest("Payment not found.");
                }

                _logger.LogInformation($"Processing payment: Id={payment.Id}, TotalAmount={payment.TotalAmount}, UserId={payment.userId}");

                var url = vnPayment.ToUrl(payment);
                _logger.LogInformation($"Redirecting to VnPay: {url}");

                TempData["PaymentId"] = paymentId;
                return Redirect(url);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Checkout error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult VnPaymentResponse([FromQuery] VnPayment obj)
        {
            _logger.LogInformation("Đã vào VnPaymentResponse");
            try
            {
                if (obj == null)
                {
                    _logger.LogError("VnPayment object is null");
                    return BadRequest(new { Message = "Dữ liệu VnPay trả về không hợp lệ" });
                }

                var queryStringRaw = HttpContext.Request.QueryString.ToString();
                _logger.LogInformation($"Raw QueryString: {queryStringRaw}");

                // Log tất cả tham số query
                var queryParamsLog = HttpContext.Request.Query
                    .Select(q => $"{q.Key}={q.Value}")
                    .ToList();
                _logger.LogInformation($"Query Parameters: {string.Join(", ", queryParamsLog)}");

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    _logger.LogWarning($"ModelState Errors: {string.Join(", ", errors)}");
                    return BadRequest(new { Message = "Dữ liệu không hợp lệ", Errors = errors });
                }

                // Tạo query string cho hash
                var queryParams = HttpContext.Request.Query
                    .Where(q => q.Key != "vnp_SecureHash" && q.Key != "vnp_SecureHashType")
                    .OrderBy(q => q.Key)
                    .Select(q => $"{q.Key}={HttpUtility.UrlEncode(q.Value, Encoding.UTF8)}")
                    .ToList();
                var queryString = string.Join("&", queryParams);
                _logger.LogInformation($"QueryString for hash: {queryString}");

                var hashSecret = vnPaysetting.HashSecret;
                var expectedHash = Helper.HashHmac512(queryString, hashSecret);
                _logger.LogInformation($"HashSecret: {hashSecret}");
                _logger.LogInformation($"ExpectedHash: {expectedHash}, ReceivedHash: {obj.SecureHash}");

                

                _logger.LogInformation("Lưu vào database...");
                context.VnPayments.Add(obj);

                var paymentId = TempData["PaymentId"]?.ToString();
                if (!string.IsNullOrEmpty(paymentId))
                {
                    var payment = context.Payments.FirstOrDefault(p => p.Id == paymentId);
                    if (payment != null)
                    {
                        // Cập nhật trạng thái thanh toán của đơn hàng
                        payment.PaymentStatus = 1; // 1 là trạng thái đã thanh toán (có thể thay đổi tùy vào hệ thống của bạn)

                        var tickets = context.Tickets.Where(t => t.PaymentId == paymentId).ToList();
                        if (tickets.Any())
                        {
                            // Cập nhật trạng thái của các vé
                            foreach (var ticket in tickets)
                            {
                                ticket.IsPaid = true; // 1 là đã thanh toán (có thể thay đổi tùy vào hệ thống của bạn)
                            }
                            _logger.LogInformation($"Đã cập nhật {tickets.Count} vé thành trạng thái đã thanh toán.");
                        }
                        else
                        {
                            _logger.LogWarning($"Không tìm thấy vé cho Payment ID {paymentId}");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Không tìm thấy đơn hàng với ID {paymentId}");
                    }
                }
                else
                {
                    _logger.LogWarning("Không có PaymentId trong TempData");
                }

                int ret = context.SaveChanges();
                _logger.LogInformation($"SaveChanges result: {ret}");

                if (ret > 0)
                {
                    _logger.LogInformation("Giao dịch thành công");
                    return RedirectToAction("MyTickets", "Tickets");
                }
                else
                {
                    _logger.LogError("Lưu giao dịch thất bại");
                    return StatusCode(500, new { Message = "Lưu giao dịch thất bại" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi VnPaymentResponse: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return StatusCode(500, new { Message = "Đã xảy ra lỗi server", Error = ex.Message });
            }
        }
    }
}