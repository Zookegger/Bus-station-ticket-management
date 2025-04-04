using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bus_Station_Ticket_Management.Models;
using Bus_Station_Ticket_Management.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Bus_Station_Ticket_Management.Controllers;

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
