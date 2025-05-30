using System.Threading.Tasks;
using Bus_Station_Ticket_Management.DataAccess;
using Bus_Station_Ticket_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bus_Station_Ticket_Management.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("Admin/[controller]/[action]")]
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<CustomersController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<List<ApplicationUser>> GetCustomers()
        {
            var customerIds = await (from user in _context.Users
                                     join userRole in _context.UserRoles on user.Id equals userRole.UserId
                                     join role in _context.Roles on userRole.RoleId equals role.Id
                                     where role.Name == "Customer"
                                     select user.Id)
                                    .Distinct()
                                    .ToListAsync();

            return await _context.Users.Where(u => customerIds.Contains(u.Id)).ToListAsync();
        }


        public async Task<ApplicationUser> GetCustomer(string? id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return new(); // Return empty list instead of null to avoid null checks downstream
                }

                var customerIds = await (from user in _context.Users
                                         join userRole in _context.UserRoles on user.Id equals userRole.UserId
                                         join role in _context.Roles on userRole.RoleId equals role.Id
                                         where role.Name == "Customer"
                                         select user.Id)
                                        .Distinct()
                                        .ToListAsync();

                var totalTickets = await _context.Tickets.Where(t => t.UserId == id).CountAsync();
                var totalSpent = await _context.Tickets.Where(t => t.UserId == id).SumAsync(t => t.TotalPrice);

                ViewBag.TotalTickets = totalTickets;
                ViewBag.TotalSpent = totalSpent;

                var customer = await _context.Users
                    .Where(u => customerIds.Contains(u.Id) && u.Id == id)
                    .FirstOrDefaultAsync();

                if (customer == null)
                {
                    return new();
                }

                return customer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customers");
                return new();
            }
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                return View(await GetCustomers());
            }
            catch (Exception ex)
            {
                return NotFound($"Error getting customers: {ex.Message}");
            }
        }

        public async Task<IActionResult> Edit(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound($"Invalid id: {id}");
            }
            var customer = await GetCustomer(id);
            if (customer == null)
            {
                return NotFound($"Cannot find customer with id {id}");
            }
            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
            ViewBag.UserRoles = await _userManager.GetRolesAsync(customer);
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string? id, ApplicationUser customer)
        {
            if (id != customer.Id)
            {
                return NotFound($"Invalid id: {id}");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!CustomerExists(customer.Id))
                    {
                        return NotFound($"Cannot find customer with id {customer.Id}");
                    }
                    else
                    {
                        _logger.LogError(ex, "Error updating customer");
                        ModelState.AddModelError("", "Unable to save changes. Please try again.");
                    }
                }
            }
            return View(customer);
        }

        public async Task<IActionResult> Delete(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound($"Invalid id: {id}");
            }
            var customer = await GetCustomer(id);
            if (customer == null)
            {
                return NotFound($"Cannot find customer with id {id}");
            }
            return View(customer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound($"Invalid id: {id}");
            }
            var customer = await GetCustomer(id);
            if (customer == null)
            {
                return NotFound($"Cannot find customer with id {id}");
            }
            try
            {
                _context.Users.Remove(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer");
                ModelState.AddModelError("", "Unable to delete customer. Please try again.");
            }
            return View(customer);
        }

        public async Task<IActionResult> DetailsPartial(string? id)
        {
            try
            {
                var customer = await GetCustomer(id);
                if (customer == null)
                {
                    return NotFound($"Cannot find customer with id {id}");
                }
                return PartialView("_DetailsPartial", customer);
            }
            catch (Exception ex)
            {
                return NotFound($"Error getting details partial: {ex.Message}");
            }
        }

        private bool CustomerExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}