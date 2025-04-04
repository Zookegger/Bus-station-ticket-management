using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bus_Station_Ticket_Management.Models;
using Bus_Station_Ticket_Management.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Bus_Station_Ticket_Management.Controllers;

//[Authorize]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var trips = _context.Trips
                        .Include(t => t.Route)
                            .ThenInclude(r => r.StartLocation)  // Nạp StartLocation
                        .Include(t => t.Route)
                            .ThenInclude(r => r.DestinationLocation)  // Nạp DestinationLocation
                        .Take(5)
                        .ToList();

        return View(trips);

    }

    //chức năng tìm kiếm trip
    [HttpPost]
    public IActionResult SearchTrips(string departure, string destination, DateTime? departureTime)
    {
        var trips = _context.Trips
            .Include(t => t.Route)
                .ThenInclude(r => r.StartLocation)
            .Include(t => t.Route)
                .ThenInclude(r => r.DestinationLocation)
            .Where(t => t.Route.StartLocation.Name.Contains(departure) && t.Route.DestinationLocation.Name.Contains(destination));

        if (departureTime.HasValue)
        {
            trips = trips.Where(t => t.DepartureTime.Date == departureTime.Value.Date);
        }

        // Debugging line
        Console.WriteLine($"Found {trips.Count()} trips.");

        return View("SearchResults", trips.ToList());
    }

    [HttpGet]
    public IActionResult GetLocations(string term)
    {
        // Tìm kiếm các địa điểm theo từ khóa (term)
        var locations = _context.Locations
            .Where(l => l.Name.Contains(term))
            .Select(l => l.Name) // Lấy tên địa điểm
            .ToList();

        // Trả về danh sách địa điểm dưới dạng JSON
        return Json(locations);
    }



    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
