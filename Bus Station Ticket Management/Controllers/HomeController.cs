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
using Bus_Station_Ticket_Management.ViewModels;

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
                .ThenInclude(r => r.StartLocation) 
            .Include(t => t.Route)
                .ThenInclude(r => r.DestinationLocation) 
            .Where(t => t.Status == "Standby" && t.DepartureTime > DateTime.Now)
            .Take(9)
            .OrderByDescending(t => t.Route != null && t.Route.StartLocation != null ? t.Route.StartLocation.Name : string.Empty)
            .ThenByDescending(t => t.Route != null && t.Route.DestinationLocation != null ? t.Route.DestinationLocation.Name : string.Empty)
            .ToListAsync();
        
        var coupons = await _context.Coupons
            .Where(c => c.IsActive && c.StartPeriod <= DateTime.Now && c.EndPeriod >= DateTime.Now)
            .ToListAsync();

        var viewModel = new TripListViewModel {
            TripsList = trips,
            Coupons = coupons,
            IsSearchResult = false
        };

        return View(viewModel);
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
                t.Route != null && 
                t.Route.StartLocation != null && 
                t.Route.DestinationLocation != null &&
                EF.Functions.Collate(t.Route.StartLocation.Name, "Vietnamese_CI_AI").Contains(EF.Functions.Collate(departure, "Vietnamese_CI_AI")) &&
                EF.Functions.Collate(t.Route.DestinationLocation.Name, "Vietnamese_CI_AI").Contains(EF.Functions.Collate(destination, "Vietnamese_CI_AI")) &&
                t.DepartureTime >= departureTime.ToDateTime(TimeOnly.MinValue)
            )
            .OrderBy(t => t.DepartureTime)
            .ToListAsync();

        var viewModel = new TripListViewModel
        {
            TripsList = trips,
            IsSearchResult = true,
            Departure = departure,
            Destination = destination,
            DepartureTime = departureTime
        };

        ViewBag.Departure = departure.Trim();
        ViewBag.Destination = destination.Trim();
        ViewBag.DepartureTime = departureTime;

        return View("Index", viewModel);
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

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var errorViewModel = new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        };
        return View(errorViewModel);
    }
}
