using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bus_Station_Ticket_Management.Models;
using Bus_Station_Ticket_Management.DataAccess;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

[Route("[controller]/[action]")]
public class TicketsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public TicketsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Action để hiển thị các vé của người dùng đã đăng nhập
    [Authorize]
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

    public async Task<IActionResult> Details(string ids) {
        var ticketIds = ids.Split(",").ToList();
        var tickets = await _context.Tickets
            .Include(t => t.Trip)
                .ThenInclude(t => t.Route)
                    .ThenInclude(r => r.StartLocation)
            .Include(t => t.Trip)
                .ThenInclude(t => t.Route)
                    .ThenInclude(r => r.DestinationLocation)
            .Include(t => t.Seat)
            .Include(t => t.User)
            .Where(t => ticketIds.Contains(t.Id))
            .ToListAsync();

        if (tickets == null || tickets.Count == 0)
            return NotFound();

        return View("Details", tickets);
    }
}
