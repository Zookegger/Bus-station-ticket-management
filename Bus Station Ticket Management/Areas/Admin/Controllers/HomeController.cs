using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bus_Station_Ticket_Management.Models;
using Bus_Station_Ticket_Management.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Bus_Station_Ticket_Management.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var trips = _context.Trips
                            .Include(t => t.Route)
                                .ThenInclude(r => r.StartLocation.Name)  // Nạp StartLocation
                            .Include(t => t.Route)
                                .ThenInclude(r => r.DestinationLocation.Name)  // Nạp DestinationLocation
                            .Take(5)
                            .ToList();

            return View(trips);

        }

        public IActionResult GoToUserHome()
        {
            return RedirectToAction("Index", "Home", new { area = "" });
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
}
