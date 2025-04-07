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

        public TicketsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> MyTickets()
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
                    .ThenInclude(trip => trip.Route) // Nạp thông tin về Route
                    .ThenInclude(route => route.StartLocation) // Nạp thông tin về StartLocation
                .Include(t => t.Trip)
                    .ThenInclude(trip => trip.Route)
                    .ThenInclude(route => route.DestinationLocation) // Nạp thông tin về DestinationLocation
                .Include(t => t.Seat) // Nạp thông tin về ghế ngồi
                .Where(t => t.UserId == user.Id) // Lọc theo UserId của người dùng hiện tại
                .ToListAsync();

            return View(tickets);
        }

        public async Task<IActionResult> Revenue(DateTime? fromDate, DateTime? toDate)
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
                    .ThenInclude(trip => trip.Route)
                            .ThenInclude(route => route.StartLocation)
                .Include(t => t.Trip)
                    .ThenInclude(trip => trip.Route)
                            .ThenInclude(route => route.DestinationLocation)
                .Include(t => t.Trip.Vehicle)  // Bao gồm thông tin xe trong chuyến đi
                .Include(t => t.Seat);  // Bao gồm thông tin ghế trong vé

            // Tính tổng doanh thu và số lượng vé
            var totalRevenue = await query.SumAsync(t => t.Trip.TotalPrice); // Tính tổng giá trị vé
            var totalTickets = await query.CountAsync();  // Đếm số lượng vé

            // Lưu giá trị vào ViewBag để hiển thị trên view
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalTickets = totalTickets;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");  // Gán lại giá trị từDate cho ViewBag
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");  // Gán lại giá trị toDate cho ViewBag

            // Lấy danh sách vé sau khi đã lọc
            var tickets = await query.ToListAsync();

            // Trả về View với danh sách vé và doanh thu
            return View("Revenue", tickets);
        }

        // GET: Admin/Tickets
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Tickets.Include(t => t.Seat).Include(t => t.Trip).Include(t => t.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Admin/Tickets/Details/5
        public async Task<IActionResult> Details(string id)
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

        // GET: Admin/Tickets/Create
        public IActionResult Create()
        {
            ViewData["SeatId"] = new SelectList(_context.Seats, "Id", "Id");
            ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Id");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Admin/Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,TripId,SeatId,BookingDate,IsPaid,IsCanceled,CancelationTime")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SeatId"] = new SelectList(_context.Seats, "Id", "Id", ticket.SeatId);
            ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Id", ticket.TripId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", ticket.UserId);
            return View(ticket);
        }

        // GET: Admin/Tickets/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            ViewData["SeatId"] = new SelectList(_context.Seats, "Id", "Id", ticket.SeatId);
            ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Id", ticket.TripId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", ticket.UserId);
            return View(ticket);
        }

        // POST: Admin/Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,UserId,TripId,SeatId,BookingDate,IsPaid,IsCanceled,CancelationTime")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
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

        // GET: Admin/Tickets/Delete/5
        public async Task<IActionResult> Delete(string id)
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

        // POST: Admin/Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(string id)
        {
            return _context.Tickets.Any(e => e.Id == id);
        }
    }
}
