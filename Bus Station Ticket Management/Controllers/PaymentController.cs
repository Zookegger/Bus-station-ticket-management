using Bus_Station_Ticket_Management.DataAccess;
using Bus_Station_Ticket_Management.Models;
using Bus_Station_Ticket_Management.Services;
using Bus_Station_Ticket_Management.Services.Email;
using Bus_Station_Ticket_Management.Services.QRCode;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
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
        private readonly IQrCodeService _qrCodeService;
        private readonly IEmailBackgroundQueue _emailQueue;

        public PaymentController(
            VnPaymentService vnPayment, 
            ApplicationDbContext context, 
            IOptions<VnPaymentSetting> vnPaymentSettings, 
            ILogger<PaymentController> logger, 
            IConfiguration configuration, 
            IQrCodeService qrCodeService, 
            IEmailBackgroundQueue emailQueue)
        {
            this.vnPayment = vnPayment;
            this.context = context;
            this.vnPaysetting = vnPaymentSettings.Value;
            _logger = logger;
            _configuration = configuration;
            _qrCodeService = qrCodeService;
            _emailQueue = emailQueue;
        }

        [HttpGet]
        [AllowAnonymous]
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
                    if (ticket.Payment == null) {
                        ticket.Payment = new Payment();
                    }

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

                    await SendTicketEmail(tickets);

                    return RedirectToAction("SelectSeats", "Seat", new { VehicleId = vehicleId, TripId = tripId });
                }

                return LogAndServerError("Failed to save transaction");
            }
            catch (Exception ex)
            {
                var tickets = context.Tickets.Where(t => t.PaymentId == paymentId.ToString()).ToList();

                foreach (var ticket in tickets)
                {
                    if (ticket.Payment == null) {
                        ticket.Payment = new Payment();
                    }
                    ticket.IsPaid = false;
                    ticket.Payment.PaymentStatus = 2;
                }

                await context.SaveChangesAsync();
                
                _logger.LogError(ex.ToString(), "Exception during VnPaymentResponse processing");
                return StatusCode(500, new { Message = "Internal server error", Error = ex.ToString() });
            }
        }

        // Send ticket QR code to user and guest (Can be improve in the future)
        // Right now it sends multiple emails to the same user which could be annoying or spammy
        private async Task SendTicketEmail(List<Ticket> tickets) {
            try {
                foreach (var ticket in tickets) {
                    var baseUrl = $"{Request.Scheme}://{Request.Host}";
                    var ticketUrl = $"{baseUrl}/Tickets/Details?id={ticket.Id}";
                    string qrCodeString = _qrCodeService.GenerateQrCode(ticketUrl);

                    byte[] qrCodeBytes = Convert.FromBase64String(qrCodeString);
                    
                    var builder = new BodyBuilder();
                    
                    var image = builder.LinkedResources.Add("qrcode.png", qrCodeBytes);
                    image.ContentId = MimeKit.Utils.MimeUtils.GenerateMessageId();
                    
                    var emailContent = $@"
                        <span>Your ticket has been successfully purchased. Please scan the QR code below to access your ticket:</span>
                        <br /> 
                        <img src=""cid:{image.ContentId}"" />
                        <br />
                        <p>If the QR code doesn't load, <a href='{ticketUrl}'>click here to view your ticket</a>.</p>
                        <br />
                        <p>Thank you for choosing EasyRide!</p>";
                    
                    if (ticket.GuestEmail != null) {
                        var message = new MimeMessage();
                        message.To.Add(new MailboxAddress("", ticket.GuestEmail));
                        message.Subject = "Ticket Purchase Confirmation";
                        message.Body = new TextPart("html") { Text = emailContent };
                        
                        await _emailQueue.QueueEmail(message);
                    } else {
                        if (ticket.User == null || ticket.User.Email == null) {
                            _logger.LogError("Email not found");
                            throw new Exception("Email not found");
                        }

                        var message = new MimeMessage();
                        message.To.Add(new MailboxAddress("", ticket.User.Email));
                        message.Subject = "Ticket Purchase Confirmation";
                        message.Body = new TextPart("html") { Text = emailContent };

                        await _emailQueue.QueueEmail(message);
                    }
                }
            } catch (Exception ex) {
                _logger.LogError(ex.ToString(), "Exception during SendTicketEmail processing");
                throw new Exception("Failed to send ticket email");
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