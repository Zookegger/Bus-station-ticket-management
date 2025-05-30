﻿using Bus_Station_Ticket_Management.DataAccess;
using Bus_Station_Ticket_Management.ViewModels;
using Bus_Station_Ticket_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace Bus_Station_Ticket_Management.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("Admin/[controller]/[action]")]
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

        [Route("Admin/Seat/SelectSeats", Name = "AdminSelectSeats")]
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
                return NotFound("Chuyến xe không tồn tại hoặc không khớp với xe được chọn.");
            }

            // Lấy danh sách ghế
            var seats = await _context.Seats
                .Where(s => s.TripId == trip.Id && s.Trip.VehicleId == VehicleId)
                .OrderBy(s => s.Floor)
                .ThenBy(s => s.Row)
                .ThenBy(s => s.Column)
                .ToListAsync();

            if (!seats.Any()) {
                return NotFound("Xe này chưa có ghế nào được tạo. Vui lòng kiểm tra lại.");
            }

            var routeName = trip.Route != null 
                ? $"{trip.Route.StartLocation?.Name ?? "N/A"} → {trip.Route.DestinationLocation?.Name ?? "N/A"}" 
                : "N/A";

            SelectSeatsViewModel viewModel = new SelectSeatsViewModel {
                TripId = trip.Id,
                VehicleId = trip.VehicleId,
                RouteName = routeName,
                DepartureLocation = trip.Route?.StartLocation?.Name ?? "N/A",
                DestinationLocation = trip.Route?.DestinationLocation?.Name ?? "N/A",
                DepartureTime = trip.DepartureTime,
                VehicleName = trip.Vehicle?.Name ?? "N/A",
                LicensePlate = trip.Vehicle?.LicensePlate ?? "N/A",
                Price = trip.TotalPrice,
                Seats = seats,
                TotalSeats = trip.Vehicle?.VehicleType?.TotalSeats ?? 0,
                TotalColumns = trip.Vehicle?.VehicleType?.TotalColumns ?? 0,
                RowsPerFloor = trip.Vehicle?.VehicleType?.RowsPerFloor ?? [],
                TotalFloors = trip.Vehicle?.VehicleType?.TotalFloors ?? 0,
            };
            
            if (User.Identity.IsAuthenticated) {
                try {
                    var user = await _userManager.GetUserAsync(User);
                    if (user != null) viewModel.User = user;
                } catch (Exception ex) {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> BookSeats(int TripId, int VehicleId, string selectedSeatIds, int numberOfTickets, Trip trip)
        {
            if (string.IsNullOrEmpty(selectedSeatIds) || numberOfTickets <= 0) {
                TempData["Error"] = "Vui lòng chọn ít nhất một ghế và nhập số lượng vé hợp lệ.";
                return RedirectToAction(nameof(SelectSeats), new { VehicleId, TripId });
            }

            var seatIds = selectedSeatIds.Split(',').Select(int.Parse).ToList();
            if (seatIds.Count != numberOfTickets) {
                TempData["Error"] = "Số ghế chọn không khớp với số lượng vé.";
                return RedirectToAction(nameof(SelectSeats), new { VehicleId, TripId });
            }

            var seats = await _context.Seats
                .Where(s => seatIds.Contains(s.Id) && s.Trip.VehicleId == VehicleId)
                .ToListAsync();

            var bookedSeats = seats.Where(s => !s.IsAvailable).ToList();
            if (bookedSeats.Any()) {
                TempData["Error"] = "Một số ghế đã được đặt: " + string.Join(", ", bookedSeats.Select(s => s.Number));
                return RedirectToAction(nameof(SelectSeats), new { VehicleId, TripId });
            }

            foreach (var seat in seats) {
                seat.IsAvailable = false;
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Sử dụng ClaimTypes và FindFirstValue
            
            foreach (var seat in seats) {
                var ticket = new Ticket {
                    TripId = TripId,
                    SeatId = seat.Id,
                    UserId = userId ?? string.Empty,
                    Id = Guid.NewGuid().ToString(),
                    BookingDate = DateTime.Now,
                    IsCanceled = false,
                    IsPaid = false
                };
                _context.Tickets.Add(ticket);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Đặt vé thành công!";
            return RedirectToAction("MyTickets", "Tickets");
        }

        // GET: Seat/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) {
                return NotFound();
            }

            var seat = await _context.Seats
                .Include(s => s.Trip)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (seat == null) {
                return NotFound();
            }

            return View(seat);
        }

        // GET: Seat/Create
        public IActionResult Create()
        {
            ViewData["TripId"] = new SelectList(_context.Vehicles, "Id", "Name");
            return View();
        }

        // POST: Seat/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Number,IsAvailable,VehicleId")] Seat seat)
        {
            if (ModelState.IsValid) {
                _context.Add(seat);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TripId"] = new SelectList(_context.Vehicles, "Id", "Name", seat.TripId);
            return View(seat);
        }

        // GET: Seat/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) {
                return NotFound();
            }

            var seat = await _context.Seats.FindAsync(id);
            if (seat == null) {
                return NotFound();
            }

            ViewData["TripId"] = new SelectList(_context.Vehicles, "Id", "Name", seat.TripId);
            return View(seat);
        }

        // POST: Seat/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Number,IsAvailable,VehicleId")] Seat seat)
        {
            if (id != seat.Id) {
                return NotFound();
            }

            if (ModelState.IsValid) {
                try {
                    _context.Update(seat);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) {
                    if (!SeatExists(seat.Id)) {
                        return NotFound();
                    }
                    else {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["TripId"] = new SelectList(_context.Vehicles, "Id", "Name", seat.TripId);
            return View(seat);
        }

        // GET: Seat/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) {
                return NotFound();
            }

            var seat = await _context.Seats
                .Include(s => s.Trip)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (seat == null) {
                return NotFound();
            }

            return View(seat);
        }

        // POST: Seat/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var seat = await _context.Seats.FindAsync(id);
            if (seat != null) {
                _context.Seats.Remove(seat);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SeatExists(int id)
        {
            return _context.Seats.Any(e => e.Id == id);
        }
    }
}
