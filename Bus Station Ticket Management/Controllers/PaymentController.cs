using Bus_Station_Ticket_Management.DataAccess;
using Bus_Station_Ticket_Management.Models;
using Bus_Station_Ticket_Management.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;
using System.Web;

namespace Bus_Station_Ticket_Management.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly VnPaymentService vnPayment;
        private readonly VnPaymentSetting vnPaysetting;
        private readonly ILogger<PaymentController> _logger;
        private readonly IConfiguration _configuration;

        public PaymentController(VnPaymentService vnPayment, ApplicationDbContext context, IOptions<VnPaymentSetting> vnPaymentSettings, ILogger<PaymentController> logger, IConfiguration configuration)
        {
            this.vnPayment = vnPayment;
            this.context = context;
            this.vnPaysetting = vnPaymentSettings.Value;
            this._logger = logger;
            _configuration = configuration;
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

                var userId = context.Tickets
                    .Where(t => t.PaymentId == paymentId)
                    .Select(t => t.UserId)
                    .FirstOrDefault();

                var ipAddress = Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

                var returnUrl = $"{Request.Scheme}://{Request.Host}/Payment/VnPaymentResponse";

                _logger.LogInformation($"Processing payment: Id={payment.Id}, TotalAmount={payment.TotalAmount}, UserId={userId}");

                var url = vnPayment.ToUrl(payment, returnUrl, ipAddress);
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
        public async Task<IActionResult> VnPaymentResponse([FromQuery] VnPayment paymentResponse)
        {
            _logger.LogInformation("Entered VnPaymentResponse");

            if (paymentResponse == null)
                return LogAndBadRequest("Invalid VnPay response data", "VnPayment object is null");

            if (!ModelState.IsValid)
                return LogAndModelStateErrors();

            var queryParams = HttpContext.Request.Query;
            LogQueryParams(queryParams);

            string queryString = BuildQueryString(queryParams);
            _logger.LogInformation($"Query string for hash: {queryString}");


            string? hashSecret = _configuration["Payment:HashSecret"];
            string expectedHash = Helper.HashHmac512(queryString, hashSecret);
            string receivedHash = paymentResponse.SecureHash;

            _logger.LogInformation($"ExpectedHash: {expectedHash}");
            _logger.LogInformation($"ReceivedHash: {receivedHash}");

            if (!string.Equals(expectedHash, receivedHash, StringComparison.OrdinalIgnoreCase))
                return LogAndBadRequest("Hash validation failed", "Hash does not match");

            var paymentId = TempData.Peek("PaymentId");
            if (paymentId == null)
                return LogAndBadRequest("Invalid payment Id", "Payment Id is null");

            try
            {
                await using var transaction = await context.Database.BeginTransactionAsync();

                var payment = await context.Payments.FirstOrDefaultAsync(p => p.Id == paymentId.ToString());
                
                if (payment == null)
                    return LogAndWarn($"No payment found with ID {paymentId}");

                var tickets = context.Tickets.Where(t => t.PaymentId == paymentId.ToString()).ToList();
                
                if (!IsPaymentSuccess(paymentResponse))
                {
                    _logger.LogWarning($"Payment failed. Response code: {paymentResponse.ResponseCode}");

                    if (tickets.Any())
                    {
                        var seatIds = tickets.Select(t => t.SeatId).ToList();
                        var seatsToFree = await context.Seats.Where(s => seatIds.Contains(s.Id)).ToListAsync();

                        foreach (var seat in seatsToFree)
                        {
                            seat.IsAvailable = true;
                        }
                        context.Tickets.RemoveRange(tickets);
                        _logger.LogWarning($"Removed {tickets.Count} ticket(s) due to failed payment.");
                    }
                    else
                    {
                        _logger.LogWarning($"No tickets found for Payment ID {paymentId}");
                    }  
                    return BadRequest(new { Message = "Payment failed or was canceled." });
                }

                // ✅ Payment success: finalize
                payment.PaymentStatus = 1; // Mark payment as successful

                int? vehicleId = null, tripId = null;
                foreach (var ticket in tickets)
                {
                    ticket.IsPaid = true;
                    ticket.Payment.PaymentStatus = 1;
                    if (vehicleId == null || tripId == null) {
                        tripId = ticket.TripId;
                        vehicleId = this.context.Trips.Where(t => t.Id == tripId).Select(t => t.VehicleId).FirstOrDefault();
                    }
                    ticket.Payment.VnPaymentTransactionNo = paymentResponse.TransactionNo;
                }
                _logger.LogInformation($"Vehicle Id: {vehicleId}");
                _logger.LogInformation($"Trip Id: {tripId}");
                
                context.VnPayments.Add(paymentResponse);
                _logger.LogInformation($"{tickets.Count} ticket(s) marked as paid.");
                int result = await context.SaveChangesAsync();
                if (result > 0)
                {
                    await transaction.CommitAsync();
                    _logger.LogInformation("Transaction saved successfully");
                    return RedirectToAction("SelectSeats", "Seat", new { VehicleId = vehicleId, TripId = tripId });
                }

                return LogAndServerError("Failed to save transaction");
            }
            catch (Exception ex)
            {
                var tickets = context.Tickets.Where(t => t.PaymentId == paymentId.ToString()).ToList();

                foreach (var ticket in tickets)
                {
                    ticket.IsPaid = false;
                    ticket.Payment.PaymentStatus = 2;
                }
                _logger.LogError(ex.ToString(), "Exception during VnPaymentResponse processing");
                return StatusCode(500, new { Message = "Internal server error", Error = ex.ToString() });
            }
        }

        private void LogQueryParams(IQueryCollection queryParams)
        {
            var rawQuery = HttpContext.Request.QueryString.ToString();
            _logger.LogInformation($"Raw QueryString: {rawQuery}");

            var paramList = queryParams.Select(q => $"{q.Key}={q.Value}");
            _logger.LogInformation("Query Parameters: " + string.Join(", ", paramList));
        }

        private string BuildQueryString(IQueryCollection queryParams)
        {
            return string.Join("&",
                queryParams
                    .Where(q => q.Key != "vnp_SecureHash" && q.Key != "vnp_SecureHashType")
                    .OrderBy(q => q.Key)
                    .Select(q => $"{q.Key}={HttpUtility.UrlEncode(q.Value, Encoding.UTF8)}"));
        }

        private IActionResult LogAndBadRequest(string userMessage, string logMessage)
        {
            _logger.LogError(logMessage);
            return BadRequest(new { Message = userMessage });
        }

        private IActionResult LogAndModelStateErrors()
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            _logger.LogWarning("ModelState errors: " + string.Join(", ", errors));
            return BadRequest(new { Message = "Invalid input", Errors = errors });
        }

        private IActionResult LogAndWarn(string message)
        {
            _logger.LogWarning(message);
            return BadRequest(new { Message = message });
        }

        private IActionResult LogAndServerError(string message)
        {
            _logger.LogError(message);
            return StatusCode(500, new { Message = message });
        }

        private bool IsPaymentSuccess(VnPayment payment)
        {
            return payment.ResponseCode == "00"; // Adjust this based on VNPAY's actual success code
        }
    }
}