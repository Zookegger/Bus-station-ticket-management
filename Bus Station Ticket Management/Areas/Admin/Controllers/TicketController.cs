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
using Microsoft.AspNetCore.Identity;

namespace Bus_Station_Ticket_Management.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("Admin/[controller]/[action]")] // Updated route to match Admin area conventions
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<TicketsController> _logger;

        public TicketsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<TicketsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> MyTickets()
        {
            try
            {
                // Lấy thông tin người dùng hiện tại
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    return RedirectToAction("Login", "Account"); // Chuyển hướng đến trang đăng nhập nếu người dùng chưa đăng nhập
                }

                // Lấy danh sách vé của người dùng từ cơ sở dữ liệu, bao gồm thông tin về chuyến đi và tuyến đường
                var tickets = await _context.Tickets
                    .Include(t => t.Trip)
                        .ThenInclude(trip => trip != null ? trip.Route : null)
                            .ThenInclude(route => route != null ? route.StartLocation : null)
                    .Include(t => t.Trip)
                        .ThenInclude(trip => trip != null ? trip.Route : null)
                            .ThenInclude(route => route != null ? route.DestinationLocation : null)
                    .Include(t => t.Seat)
                    .Where(t => t.UserId == user.Id)
                    .ToListAsync();

                return View(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MyTickets");
                return View(new List<Ticket>());
            }
        }

        public async Task<IActionResult> Revenue(DateTime? fromDate, DateTime? toDate)
        {
            try
            {

                // Khởi tạo truy vấn cơ bản
                var query = _context.Tickets.AsQueryable();

                // Lọc theo ngày nếu có
                if (fromDate.HasValue)
                {
                    query = query.Where(t => t.BookingDate.Date >= fromDate.Value.Date);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(t => t.BookingDate.Date <= toDate.Value.Date);
                }

                // Sau khi áp dụng điều kiện lọc, ta mới thực hiện Include
                query = query
                    .Include(t => t.User)  // Bao gồm thông tin User
                    .Include(t => t.Trip)
                        .ThenInclude(trip => trip != null ? trip.Route : null)
                            .ThenInclude(route => route != null ? route.StartLocation : null)
                    .Include(t => t.Trip)
                        .ThenInclude(trip => trip != null ? trip.Route : null)
                            .ThenInclude(route => route != null ? route.DestinationLocation : null)
                    .Include(t => t.Trip)
                        .ThenInclude(trip => trip != null ? trip.Vehicle : null)
                    .Include(t => t.Seat);

                // Tính tổng doanh thu và số lượng vé
                var totalRevenue = await query.SumAsync(t => t.TotalPrice); // Tính tổng giá trị vé
                var totalTickets = await query.CountAsync();  // Đếm số lượng vé

                // Lưu giá trị vào ViewBag để hiển thị trên view
                ViewBag.TotalRevenue = totalRevenue;
                ViewBag.TotalTickets = totalTickets;
                ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
                ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

                // Lấy danh sách vé sau khi đã lọc
                var tickets = await query.ToListAsync();

                // Trả về View với danh sách vé và doanh thu
                return View("Revenue", tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Revenue");
                return View(new List<Ticket>());
            }
        }

        // GET: Admin/Tickets
        public async Task<IActionResult> Index()
        {
            try
            {
                var tickets = await _context.Tickets
                    .Include(t => t.Trip)
                        .ThenInclude(trip => trip != null ? trip.Route : null)
                            .ThenInclude(route => route != null ? route.StartLocation : null)
                    .Include(t => t.Trip)
                        .ThenInclude(trip => trip != null ? trip.Route : null)
                            .ThenInclude(route => route != null ? route.DestinationLocation : null)
                    .Include(t => t.Seat)
                    .Include(t => t.User)
                    .OrderBy(t => t.Id)
                    .ToListAsync();

                return View(tickets);
            }
            catch (Exception ex)
            {
                return View(new List<Ticket>());
            }
        }

        // GET: Admin/Tickets/Details/5
        public async Task<IActionResult> Details(string id)
        {
            try
            {

                if (id == null)
                {
                    return NotFound();
                }

                var ticket = await _context.Tickets.Include(t => t.Trip)
                    .Include(t => t.Trip)
                        .ThenInclude(trip => trip.Route)
                            .ThenInclude(route => route.StartLocation)
                    .Include(t => t.Trip)
                        .ThenInclude(trip => trip.Route)
                            .ThenInclude(route => route.DestinationLocation)
                    .Include(t => t.Seat)
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (ticket == null)
                {
                    return NotFound();
                }

                return View(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Details");
                return View(new Ticket());
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> DetailsPartial(string id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var ticket = await _context.Tickets
                    .AsNoTracking()
                    .Include(t => t.Trip)
                    .Include(t => t.Trip)
                        .ThenInclude(trip => trip.Route!)
                            .ThenInclude(route => route.StartLocation!)
                    .Include(t => t.Trip)
                        .ThenInclude(trip => trip.Route!)
                            .ThenInclude(route => route.DestinationLocation!)
                    .Include(t => t.Trip)
                        .ThenInclude(trip => trip.TripDriverAssignments!)
                            .ThenInclude(tda => tda.Driver!)
                    .Include(t => t.Seat)
                    .Include(t => t.Trip)
                        .ThenInclude(trip => trip.Vehicle)
                            .ThenInclude(vehicle => vehicle.VehicleType)
                    .Include(t => t.User)
                    .Include(t => t.Coupon)
                    .Include(t => t.Payment)
                        .ThenInclude(p => p.VnPayment)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (ticket == null)
                {
                    return NotFound();
                }

                return PartialView("_DetailsPartial", ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DetailsPartial");
                return PartialView("_DetailsPartial", new Ticket());
            }
        }

        // GET: Admin/Tickets/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            try
            {

                if (id == null)
                {
                    return NotFound();
                }

                var ticket = await _context.Tickets.Include(t => t.Trip)
                    .Include(t => t.Trip)
                        .ThenInclude(trip => trip.Route)
                            .ThenInclude(route => route.StartLocation)
                    .Include(t => t.Trip)
                        .ThenInclude(trip => trip.Route)
                            .ThenInclude(route => route.DestinationLocation)
                    .Include(t => t.Seat)
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (ticket == null)
                {
                    return NotFound();
                }

                return View(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Edit");
                return View(new Ticket());
            }
        }

        // POST: Admin/Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,UserId,TripId,SeatId,BookingDate,IsPaid")] Ticket ticket)
        {
            try
            {
                if (id != ticket.Id)
                {
                    return NotFound("Id do not match");
                }

                var fetchTicket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticket.Id);
                if (fetchTicket == null)
                {
                    return NotFound("Cannot Find Ticket");
                }

                if (ModelState.IsValid)
                {
                    if (fetchTicket.IsPaid == false && fetchTicket.IsPaid != ticket.IsPaid && fetchTicket.IsReserved)
                    {
                        ticket.IsPaid = true;
                    }

                    try
                    {
                        _context.Update(ticket);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!TicketExists(ticket.Id))
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

                ViewData["SeatId"] = new SelectList(_context.Seats, "Id", "Id", ticket.SeatId);
                ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Id", ticket.TripId);
                ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", ticket.UserId);
                return View(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Edit");
                return View(new Ticket());
            }
        }

        // GET: Admin/Tickets/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var ticket = await _context.Tickets
                    .Include(t => t.Seat)
                    .Include(t => t.Trip)
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (ticket == null)
                {
                    return NotFound();
                }

                return View(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Delete");
                return View(new Ticket());
            }
        }

        // POST: Admin/Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(id);
                if (ticket != null)
                {
                    _context.Tickets.Remove(ticket);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteConfirmed");
                return RedirectToAction(nameof(Index));
            }
        }

        private bool TicketExists(string id)
        {
            return _context.Tickets.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<JsonResult> GetFilteredRevenue(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var query = _context.Tickets.AsQueryable();

                // Apply date filters
                if (fromDate.HasValue)
                {
                    query = query.Where(t => t.BookingDate.Date >= fromDate.Value.Date);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(t => t.BookingDate.Date <= toDate.Value.Date);
                }


                // Group by date and calculate daily revenue
                var revenueData = await query
                    .GroupBy(t => t.BookingDate.Date)
                    .Select(g => new
                    {
                        Date = g.Key.ToString("dd/MM/yyyy"),
                        Revenue = g.Sum(t => t.TotalPrice)
                    })
                    .OrderBy(x => DateTime.ParseExact(x.Date, "dd/MM/yyyy", null))
                    .ToListAsync();

                return Json(revenueData);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}
