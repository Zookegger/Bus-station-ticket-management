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
            // Kiểm tra Trip và Bus có tồn tại không
            var trip = await _context.Trips
                .Include(t => t.Vehicle)
                    .ThenInclude(v => v.VehicleType)
                .Include(t => t.Route)
                    .ThenInclude(r => r.StartLocation)
                .Include(t => t.Route)
                    .ThenInclude(r => r.DestinationLocation)
                .FirstOrDefaultAsync(t => t.Id == TripId && t.VehicleId == VehicleId);

            if (trip == null) {
                return NotFound("Trip not found or doesn't match with vehicle.");
            }

            // Lấy danh sách ghế
            var seats = await _context.Seats
                .Where(s => s.Trip.VehicleId == VehicleId)
                .OrderBy(s => s.Floor)
                .ThenBy(s => s.Row)
                .ThenBy(s => s.Column)
                .ToListAsync();

            if (!seats.Any()) {
                return NotFound("Seat hasn't been generated.");
            }

            SelectSeatsViewModel viewModel = new SelectSeatsViewModel {
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
                TotalSeats = trip.Vehicle?.VehicleType.TotalSeats ?? 0,
                TotalColumns = trip.Vehicle?.VehicleType.TotalColumn ?? 0,
                TotalRows = trip.Vehicle?.VehicleType.TotalRow ?? 0,
                TotalFloors = trip.Vehicle?.VehicleType.TotalFlooring ?? 0,
            };
            
            if (User.Identity.IsAuthenticated == true) {
                try {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!string.IsNullOrEmpty(userId)) {
                        var user = await _userManager.FindByIdAsync(userId);
                        if (user != null) viewModel.User = user;
                    }
                } catch (Exception ex) {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> BookSeats(int TripId, int VehicleId, string selectedSeatIds, int numberOfTickets, string? guestName, string? guestEmail, string? guestPhone, int couponId)
        {
            if (string.IsNullOrEmpty(selectedSeatIds) || numberOfTickets <= 0) {
                TempData["Error"] = "Please input a valid seat amount and select at least 1 seat.";
                return RedirectToAction(nameof(SelectSeats), new { VehicleId, TripId });
            }

            if (string.IsNullOrEmpty(guestName) || string.IsNullOrEmpty(guestEmail) || string.IsNullOrEmpty(guestPhone)) {
                return RedirectToAction(nameof(SelectSeats), new { VehicleId, TripId });
            }

            var seatIds = selectedSeatIds.Split(',').Select(int.Parse).ToList();
            if (seatIds.Count != numberOfTickets) {
                TempData["Error"] = "Number of seats doesn't match number of tickets.";
                return RedirectToAction(nameof(SelectSeats), new { VehicleId, TripId });
            }

            var seats = await _context.Seats
                .Where(s => seatIds.Contains(s.Id) && s.Trip.VehicleId == VehicleId)
                .ToListAsync();

            var bookedSeats = seats.Where(s => !s.IsAvailable).ToList();
            if (bookedSeats.Any()) {
                TempData["Error"] = "These seats is already booked: " + string.Join(", ", bookedSeats.Select(s => s.Number));
                return RedirectToAction(nameof(SelectSeats), new { VehicleId, TripId });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get logged-in user ID
            if (!string.IsNullOrEmpty(userId) && User.Identity.IsAuthenticated) {
                userId = _userManager.GetUserId(User);
            }
            
            List<string> ticketIds = new List<string>();

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                foreach (var seat in seats) {
                    seat.IsAvailable = false;
                    
                    var ticket = new Ticket {
                        Id = Guid.NewGuid().ToString(),
                        TripId = TripId,
                        SeatId = seat.Id,
                        UserId = userId,
                        BookingDate = DateTime.Now,
                        IsCanceled = false,
                        IsPaid = false,
                        CouponId = couponId,
                        GuestName = guestName,
                        GuestEmail = guestEmail,
                        GuestPhone = guestPhone
                    };

                    _context.Tickets.Add(ticket);
                    ticketIds.Add(ticket.Id);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();   
            }
                
            TempData["Success"] = "Successfully booked seat!";
            string ticketIdsList = string.Join(",", ticketIds);
            return RedirectToAction("Details", "Tickets", new { ids = ticketIdsList});
        }

        private bool SeatExists(int id)
        {
            return _context.Seats.Any(e => e.Id == id);
        }
    }
}
