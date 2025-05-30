using Bus_Station_Ticket_Management.DataAccess;
using Bus_Station_Ticket_Management.ViewModels;
using Bus_Station_Ticket_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Bus_Station_Ticket_Management.Services;
using System.Data.Common;

namespace Bus_Station_Ticket_Management.Controllers
{
    [Route("[controller]/[action]")]
    public class SeatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<SeatController> _logger;

        public SeatController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<SeatController> logger
        )
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Seat
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Seats.Include(s => s.Trip);
            return View(await applicationDbContext.ToListAsync());
        }

        public async Task<IActionResult> SelectSeats(int VehicleId, int TripId)
        {
            try
            {
                TempData.Keep();

                var trip = await _context.Trips
                    .Include(t => t.Vehicle)
                        .ThenInclude(vt => vt.VehicleType)
                    .Include(t => t.Route)
                        .ThenInclude(r => r.StartLocation)
                    .Include(t => t.Route)
                        .ThenInclude(r => r.DestinationLocation)
                    .FirstOrDefaultAsync(t => t.Id == TripId && t.VehicleId == VehicleId);

                if (trip == null)
                {
                    return NotFound("Trip not found or doesn't match with vehicle.");
                }

                if (_context.Seats == null)
                {
                    return NotFound("Seats data is not available.");
                }

                var seats = await _context.Seats
                    .Where(s => s.Trip != null && s.Trip.VehicleId == VehicleId && s.Trip.Id == TripId)
                    .OrderBy(s => s.Floor)
                    .ThenBy(s => s.Row)
                    .ThenBy(s => s.Column)
                    .ToListAsync();

                if (seats.Count == 0)
                {
                    return NotFound("Seat hasn't been generated.");
                }

                SelectSeatsViewModel viewModel = new SelectSeatsViewModel
                {
                    TripId = trip.Id,
                    VehicleId = trip.VehicleId,
                    RouteName = trip.Route != null ? $"{trip.Route.StartLocation?.Name} → {trip.Route.DestinationLocation?.Name}" : "N/A",
                    DepartureLocation = trip.Route?.StartLocation?.Name ?? "N/A",
                    DepartureLocationAddress = trip.Route?.StartLocation?.Address ?? "N/A",
                    DestinationLocation = trip.Route?.DestinationLocation?.Name ?? "N/A",
                    DestinationLocationAddress = trip.Route?.DestinationLocation?.Address ?? "N/A",
                    DepartureTime = trip.DepartureTime,
                    VehicleName = trip.Vehicle?.Name ?? "",
                    LicensePlate = trip.Vehicle?.LicensePlate ?? "",
                    Price = trip.TotalPrice,
                    Seats = seats,
                    TotalSeats = trip.Vehicle?.VehicleType?.TotalSeats ?? 0,
                    TotalColumns = trip.Vehicle?.VehicleType?.TotalColumns ?? 0,
                    RowsPerFloor = trip.Vehicle?.VehicleType?.RowsPerFloor ?? [],
                    TotalFloors = trip.Vehicle?.VehicleType?.TotalFloors ?? 0,
                };

                if (User.Identity?.IsAuthenticated == true)
                {
                    try
                    {
                        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        if (!string.IsNullOrEmpty(userId))
                        {
                            var user = await _userManager.FindByIdAsync(userId);
                            if (user != null)
                                viewModel.User = user;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                }

                return View(viewModel);
            }
            catch (DbException ex)
            {
                _logger.LogError("Database error while selecting seats: {Message}", ex.Message);
                return RedirectToAction("Error", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError("Unexpected error while selecting seats: {Message}", ex.Message);
                return RedirectToAction("Error", "Home");
            }
        }

        // Need to implement failsafe when vnpayment fail 
        [HttpPost]
        public async Task<IActionResult> BookSeats(int TripId, int VehicleId, string selectedSeatIds, int numberOfTickets, string? guestName, string? guestEmail, string? guestPhone, int totalPrice, int? couponId, string paymentMethod)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get userId

            if (!string.IsNullOrEmpty(userId) && User.Identity?.IsAuthenticated == true)
            {
                userId = _userManager.GetUserId(User);
            }

            _logger.LogInformation("Booking seats for user: {UserId}", userId);

            if (!IsBookingValid(TripId, VehicleId, selectedSeatIds, numberOfTickets, userId, guestName, guestEmail, guestPhone, totalPrice, couponId, paymentMethod, out var seats, out var bookedSeats))
            {
                _logger.LogWarning("Invalid booking request: {Error}", TempData["Error"]);
                return RedirectToAction(nameof(SelectSeats), new { VehicleId, TripId, selectedSeatIds, numberOfTickets, guestName, guestEmail, guestPhone, couponId });
            }

            var ticketIds = new List<string>();

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var payment = new Payment
                    {
                        TotalAmount = totalPrice,
                        CreatedAt = DateTime.Now,
                        PaymentMethod = paymentMethod,
                        PaymentStatus = 0 // pending
                    };

                    _context.Payments.Add(payment);
                    TempData["PaymentId"] = payment.Id;
                    List<Ticket> ticketList = new List<Ticket>();

                    foreach (var seat in seats)
                    {
                        seat.IsAvailable = false;

                        var ticket = new Ticket
                        {
                            TripId = TripId,
                            SeatId = seat.Id,
                            UserId = userId,
                            BookingDate = DateTime.Now,
                            IsCanceled = false,
                            IsPaid = false,
                            CouponId = couponId,
                            GuestName = guestName,
                            GuestEmail = guestEmail,
                            GuestPhone = guestPhone,
                            TotalPrice = totalPrice / numberOfTickets, // Price for each ticket
                            PaymentId = payment.Id
                        };

                        if (paymentMethod != "Cash")
                        {
                            ticket.IsReserved = true;
                        }

                        if (ticket.Id == null)
                        {
                            _logger.LogError("Ticket ID is null for seat: {SeatId}", seat.Id);
                            throw new Exception("Ticket ID is null for seat: " + seat.Id);
                        }

                        ticketIds.Add(ticket.Id);
                        ticketList.Add(ticket);
                    }
                    await _context.Tickets.AddRangeAsync(ticketList);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (DbUpdateException ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError("Database error while booking seats: {Message}", ex.Message);
                    TempData["Error"] = "An unexpected error occurred while booking your seats. Please try again.";
                    return RedirectToAction(nameof(SelectSeats), new { VehicleId, TripId, selectedSeatIds, numberOfTickets, guestName, guestEmail, guestPhone, couponId });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError("Unexpected error while booking seats: {Message}", ex.Message);
                    TempData["Error"] = "An unexpected error occurred while booking your seats. Please try again.";
                    return RedirectToAction(nameof(SelectSeats), new { VehicleId, TripId, selectedSeatIds, numberOfTickets, guestName, guestEmail, guestPhone, couponId });
                }
            }

            TempData["TicketIds"] = string.Join(",", ticketIds);
            TempData["TotalPrice"] = totalPrice;
            TempData["Success"] = "Successfully booked seat!";
            TempData["RedirectAfterDelay"] = true; // Flag to trigger the delay in the view

            // Redirect to the SelectSeats or Payment checkout view with TempData
            return paymentMethod != "Cash" ? RedirectToAction("Checkout", "Payment") : RedirectToAction(nameof(SelectSeats), new { VehicleId, TripId });
        }

        private bool IsBookingValid(int TripId, int VehicleId, string selectedSeatIds, int numberOfTickets, string? userId, string? guestName, string? guestEmail, string? guestPhone, int totalPrice, int? couponId, string paymentMethod, out List<Seat> seats, out List<Seat> bookedSeats)
        {
            seats = new List<Seat>();
            bookedSeats = new List<Seat>();

            if (string.IsNullOrEmpty(paymentMethod))
            {
                TempData["Error"] = "Invalid payment method.";
                return false;
            }

            if (string.IsNullOrEmpty(selectedSeatIds) || numberOfTickets <= 0)
            {
                TempData["Error"] = "Please input a valid seat amount and select at least 1 seat.";
                return false;
            }

            var seatIds = selectedSeatIds.Split(',').Select(int.Parse).ToList();

            if (seatIds.Count != numberOfTickets)
            {
                TempData["Error"] = "Number of seats doesn't match the number of tickets.";
                return false;
            }

            if (string.IsNullOrEmpty(userId))
            {
                if (string.IsNullOrEmpty(guestName) || string.IsNullOrEmpty(guestEmail) || string.IsNullOrEmpty(guestPhone))
                {
                    TempData["Error"] = "Please fill out your information.";
                    return false;
                }
            }
            try
            {
                seats = _context.Seats
                    .Include(s => s.Trip)
                    .Where(s =>
                        seatIds.Contains(s.Id) &&
                        s.TripId == TripId &&
                        s.Trip != null &&
                        s.Trip.VehicleId == VehicleId)
                    .ToList();

                bookedSeats = seats.Where(s => !s.IsAvailable).ToList();

                if (bookedSeats.Any())
                {
                    TempData["Error"] = "These seats are already booked: " + string.Join(", ", bookedSeats.Select(s => s.Number));
                    return false;
                }

                return true;
            }
            catch (DbException ex)
            {
                _logger.LogError("Database error while validating booking: {Message}", ex.Message);
                return false;
            }
        }

        private bool SeatExists(int id)
        {
            return _context.Seats.Any(e => e.Id == id);
        }
    }
}