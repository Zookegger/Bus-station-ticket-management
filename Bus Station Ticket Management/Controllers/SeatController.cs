    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;
    using Bus_Station_Ticket_Management.DataAccess;
    using Bus_Station_Ticket_Management.Models;
    using Microsoft.AspNetCore.Authorization;
    using System.Security.Claims;

    namespace Bus_Station_Ticket_Management.Controllers
    {
        public class SeatController : Controller
        {
            private readonly ApplicationDbContext _context;

            public SeatController(ApplicationDbContext context)
            {
                _context = context;
            }

            // GET: Seat
            public async Task<IActionResult> Index()
            {
                var applicationDbContext = _context.Seats.Include(s => s.Vehicle);
                return View(await applicationDbContext.ToListAsync());
            }

        public async Task<IActionResult> SelectSeats(int busId, int tripId)
        {
            // Kiểm tra Trip và Bus có tồn tại không
            var trip = await _context.Trips
                .Include(t => t.Vehicle)
                .FirstOrDefaultAsync(t => t.Id == tripId && t.VehicleId == busId);

            if (trip == null)
            {
                return NotFound("Chuyến xe không tồn tại hoặc không khớp với xe được chọn.");
            }

            // Lấy danh sách ghế
            var seats = await _context.Seats
                .Where(s => s.VehicleId == busId)
                .OrderBy(s => s.Floor)
                .ThenBy(s => s.Row)
                .ThenBy(s => s.Column)
                .ToListAsync();

            if (!seats.Any())
            {
                return NotFound("Xe này chưa có ghế nào được tạo. Vui lòng kiểm tra lại.");
            }

            // Truyền dữ liệu vào ViewData hoặc ViewBag
            ViewData["TripId"] = tripId;
            ViewData["BusId"] = busId;
            ViewData["Seats"] = seats;
            ViewData["TripDeparture"] = trip.Route.StartLocation.Name;
            ViewData["TripDestination"] = trip.Route.DestinationLocation.Name;
            ViewData["DepartureTime"] = trip.DepartureTime;
            ViewData["BusType"] = trip.Vehicle?.VehicleType.ToString()?? "Sleeper";
            ViewData["Price"] = trip.TotalPrice;
            ViewData["LicensePlate"] = trip.Vehicle?.LicensePlate;

            return View();
        }




        [HttpPost]
            [Authorize]
            public async Task<IActionResult> BookSeats(int tripId, int busId, string selectedSeatIds, int numberOfTickets, Trip trip)
            {
                if (string.IsNullOrEmpty(selectedSeatIds) || numberOfTickets <= 0)
                {
                    TempData["Error"] = "Vui lòng chọn ít nhất một ghế và nhập số lượng vé hợp lệ.";
                    return RedirectToAction(nameof(SelectSeats), new { busId, tripId });
                }

                var seatIds = selectedSeatIds.Split(',').Select(int.Parse).ToList();
                if (seatIds.Count != numberOfTickets)
                {
                    TempData["Error"] = "Số ghế chọn không khớp với số lượng vé.";
                    return RedirectToAction(nameof(SelectSeats), new { busId, tripId });
                }

                var seats = await _context.Seats
                    .Where(s => seatIds.Contains(s.Id) && s.VehicleId == busId)
                    .ToListAsync();

                var bookedSeats = seats.Where(s => s.IsAvailable).ToList();
                if (bookedSeats.Any())
                {
                    TempData["Error"] = "Một số ghế đã được đặt: " + string.Join(", ", bookedSeats.Select(s => s.Number));
                    return RedirectToAction(nameof(SelectSeats), new { busId, tripId });
                }

                foreach (var seat in seats)
                {
                    seat.IsAvailable = true;
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Sử dụng ClaimTypes và FindFirstValue
                foreach (var seat in seats)
                {
                    var ticket = new Ticket
                    {
                        TripId = tripId,
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
                return RedirectToAction("MyTickets", "Ticket");
            }




            // GET: Seat/Details/5
            public async Task<IActionResult> Details(int? id)
            {
                if (id == null)
                {
                    return NotFound();
                }

                var seat = await _context.Seats
                    .Include(s => s.Vehicle)
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (seat == null)
                {
                    return NotFound();
                }

                return View(seat);
            }

            // GET: Seat/Create
            public IActionResult Create()
            {
                ViewData["VehicleId"] = new SelectList(_context.Vehicles, "Id", "Id");
                return View();
            }

            // POST: Seat/Create
            // To protect from overposting attacks, enable the specific properties you want to bind to.
            // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Create([Bind("Id,Number,IsAvailable,VehicleId")] Seat seat)
            {
                if (ModelState.IsValid)
                {
                    _context.Add(seat);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                ViewData["VehicleId"] = new SelectList(_context.Vehicles, "Id", "Id", seat.VehicleId);
                return View(seat);
            }

            // GET: Seat/Edit/5
            public async Task<IActionResult> Edit(int? id)
            {
                if (id == null)
                {
                    return NotFound();
                }

                var seat = await _context.Seats.FindAsync(id);
                if (seat == null)
                {
                    return NotFound();
                }
                ViewData["VehicleId"] = new SelectList(_context.Vehicles, "Id", "Id", seat.VehicleId);
                return View(seat);
            }

            // POST: Seat/Edit/5
            // To protect from overposting attacks, enable the specific properties you want to bind to.
            // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(int id, [Bind("Id,Number,IsAvailable,VehicleId")] Seat seat)
            {
                if (id != seat.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(seat);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!SeatExists(seat.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
                ViewData["VehicleId"] = new SelectList(_context.Vehicles, "Id", "Id", seat.VehicleId);
                return View(seat);
            }

            // GET: Seat/Delete/5
            public async Task<IActionResult> Delete(int? id)
            {
                if (id == null)
                {
                    return NotFound();
                }

                var seat = await _context.Seats
                    .Include(s => s.Vehicle)
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (seat == null)
                {
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
                if (seat != null)
                {
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
