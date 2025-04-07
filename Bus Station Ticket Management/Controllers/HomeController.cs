using Bus_Station_Ticket_Management.DataAccess;
using Bus_Station_Ticket_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Google.Apis.Auth.AspNetCore3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Bus_Station_Ticket_Management.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var trips = await _context.Trips
                        .Include(t => t.Route)
                            .ThenInclude(r => r.StartLocation)  // Nạp StartLocation
                        .Include(t => t.Route)
                            .ThenInclude(r => r.DestinationLocation)  // Nạp DestinationLocation
                        .Take(5)
                        .Where(t => t.Status == "Stand By")
                        .ToListAsync();

        return View(trips);

    }

    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous]
    public IActionResult Login()
    {
        var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [AllowAnonymous]
    public async Task<IActionResult> GoogleResponse()
    {
        var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme); // Use Identity's scheme
        if (result?.Principal == null)
            return RedirectToAction(nameof(Index));

        var claims = result.Principal.Identities
            .FirstOrDefault()?.Claims
            .Select(claim => new {
                claim.Issuer,
                claim.OriginalIssuer,
                claim.Type,
                claim.Value
            });

        return Json(claims);
    }

    public async Task<IActionResult> SearchTrips(string departure, string destination, DateOnly departureTime) {
        departure = departure?.Trim() ?? "";
        destination = destination?.Trim() ?? "";
        
        var trips = await _context.Trips
            .Include(t => t.Route)
                .ThenInclude(r => r.StartLocation)
            .Include(t => t.Route)
                .ThenInclude(r => r.DestinationLocation)
            .Where(t => 
                t.Route.StartLocation.Name.Contains(departure) &&
                t.Route.DestinationLocation.Name.Contains(destination) &&
                t.DepartureTime >= departureTime.ToDateTime(TimeOnly.MinValue)
            ).OrderBy(t => t.DepartureTime).ToListAsync();

        return View(trips);
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
