using Bus_Station_Ticket_Management.DataAccess;
using Bus_Station_Ticket_Management.ViewModels;
using Bus_Station_Ticket_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace Bus_Station_Ticket_Management.Controllers
{
    [Route("[controller]/[action]")]
    public class SeatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SeatController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Seat
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Seats.Include(s => s.Trip);
            return View(await applicationDbContext.ToListAsync());
        }

        [Route("Seat/SelectSeats", Name = "DefaultSelectSeats")]
        public async Task<IActionResult> SelectSeats(int VehicleId, int TripId)
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
                DestinationLocation = trip.Route?.DestinationLocation?.Name ?? "N/A",
                DepartureTime = trip.DepartureTime,
                VehicleName = trip.Vehicle?.Name ?? "",
                LicensePlate = trip.Vehicle?.LicensePlate ?? "",
                Price = trip.TotalPrice,
                Seats = seats,
                TotalSeats = trip.Vehicle?.VehicleType?.TotalSeats ?? 0,
                TotalColumns = trip.Vehicle?.VehicleType?.TotalColumn ?? 0,
                TotalRows = trip.Vehicle?.VehicleType?.TotalRow ?? 0,
                TotalFloors = trip.Vehicle?.VehicleType?.TotalFlooring ?? 0,
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


        // Need to implement failsafe when vnpayment fail 
        [HttpPost]
        public async Task<IActionResult> BookSeats(int TripId, int VehicleId, string selectedSeatIds, int numberOfTickets, string? guestName, string? guestEmail, string? guestPhone, int totalPrice, int? couponId, string paymentMethod)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get userId

            if (!string.IsNullOrEmpty(userId) && User.Identity?.IsAuthenticated == true)
            {
                userId = _userManager.GetUserId(User);
            }

            if (!IsBookingValid(TripId, VehicleId, selectedSeatIds, numberOfTickets, userId, guestName, guestEmail, guestPhone, totalPrice, couponId, paymentMethod, out var seats, out var bookedSeats))
            {
                return RedirectToAction(nameof(SelectSeats), new { VehicleId, TripId, selectedSeatIds, numberOfTickets, guestName, guestEmail, guestPhone, couponId });
            }

            try
            {
                var ticketIds = new List<string>();

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    var payment = new Payment
                    {
                        Id = Guid.NewGuid().ToString(),
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
                            Id = Guid.NewGuid().ToString(),
                            TripId = TripId,
                            SeatId = seat.Id,
                            UserId = userId ?? string.Empty,
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

                        if(paymentMethod != "Cash") {
                            ticket.IsReserved = true;
                        }

                        ticketIds.Add(ticket.Id);
                        ticketList.Add(ticket);
                    }
                    await _context.Tickets.AddRangeAsync(ticketList);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["TicketIds"] = string.Join(",", ticketIds);
                    TempData["TotalPrice"] = totalPrice;
                    TempData["Success"] = "Successfully booked seat!";
                    TempData["RedirectAfterDelay"] = true; // Flag to trigger the delay in the view

                    // Redirect to the SelectSeats or Payment checkout view with TempData
                    return paymentMethod != "Cash" ? RedirectToAction("Checkout", "Cart") : RedirectToAction(nameof(SelectSeats), new { VehicleId, TripId });
                }
            }
            catch (DbUpdateException ex)
            {
                System.Diagnostics.Debug.WriteLine($"\nError: {ex}\n");
                TempData["Error"] = "An unexpected error occurred while booking your seats. Please try again.";
                return RedirectToAction(nameof(SelectSeats), new { VehicleId, TripId, selectedSeatIds, numberOfTickets, guestName, guestEmail, guestPhone, couponId });
            }
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

        private bool SeatExists(int id)
        {
            return _context.Seats.Any(e => e.Id == id);
        }
    }
}