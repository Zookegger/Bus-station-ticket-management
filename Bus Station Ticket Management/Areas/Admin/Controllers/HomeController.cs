using Bus_Station_Ticket_Management.DataAccess;
using Bus_Station_Ticket_Management.Models;
using Bus_Station_Ticket_Management.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Bus_Station_Ticket_Management.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("Admin/[controller]/[action]")]

    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;


        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        
        public IActionResult Dashboard() {
            return View();
        }
        

        public async Task<IActionResult> GetDashboardStats()
        {
            var vehicleCount = await _context.Vehicles.CountAsync();

            var userRole = await _roleManager.FindByNameAsync("Customer");
            if (userRole == null)
            {
                return NotFound("Error: Cannot get customers role");
            }

            // Await the GetUsersInRoleAsync method to get the count of users in the "Customer" role
            if (string.IsNullOrEmpty(userRole.Name))
            {
                return NotFound("Error: Role name is null or empty");
            }

            var userCount = await _userManager.GetUsersInRoleAsync(userRole.Name);

            var tripCount = await _context.Trips.CountAsync();

            // Calculate total earnings
            var earnings = await _context.Tickets.SumAsync(t => t.TotalPrice);

            var now = DateTime.Now;

            // Initialize dictionary to hold revenue data for each month
            Dictionary<string, long> revenueData = new Dictionary<string, long>();

            // Loop through each month (1 to 12) and sum the total price for that month
            for (int month = 1; month <= 12; month++)
            {
                string monthName = new DateTime(now.Year, month, 1).ToString("MMMM");

                long monthlyRevenue = await _context.Tickets
                    .Where(t => t.BookingDate.Month == month && t.BookingDate.Year == now.Year)
                    .SumAsync(t => t.TotalPrice);

                revenueData.Add(monthName, monthlyRevenue);
            }

            // Convert to individual Key-Value for easy handling when data is requested
            var revenueLabels = revenueData.Keys.ToList();
            var revenueValues = revenueData.Values.ToList();

            // Create an anonymous object to return as JSON
            var stats = new
            {
                UserCount = userCount.Count,  // Ensure you're passing the correct user count
                VehicleCount = vehicleCount,
                TripCount = tripCount,
                RevenueCount = earnings,
                RevenueLabels = revenueLabels,
                RevenueValues = revenueValues
            };

            // Return the stats as JSON
            return Json(stats);
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
                return RedirectToAction(nameof(Dashboard));

            var claims = result.Principal.Identities
                .FirstOrDefault()?.Claims
                .Select(claim => new
                {
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
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

}
