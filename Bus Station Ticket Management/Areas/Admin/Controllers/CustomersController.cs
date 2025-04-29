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
    [Route("Admin/[Controller]/[Action]")]
    public class CustomersController : Controller {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public CustomersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<List<ApplicationUser>> GetCustomers() {
            var customerIds = await (from user in _context.Users
                                    join userRole in _context.UserRoles on user.Id equals userRole.UserId
                                    join role in _context.Roles on userRole.RoleId equals role.Id
                                    where role.Name == "Customer"
                                    select user.Id)
                                    .Distinct()
                                    .ToListAsync();

            return await _context.Users.Where(u => customerIds.Contains(u.Id)).ToListAsync();
        }


        public async Task<List<ApplicationUser>> GetCustomers(string? id) {
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

            return await _context.Users
                                .Where(u => customerIds.Contains(u.Id) && u.Id.Contains(id))
                                .ToListAsync();
        }

        public async Task<IActionResult> Index() {
            return View(await GetCustomers());
        }

        public async Task<IActionResult> DetailsPartial(string? id) {
            var customer = await GetCustomers(id);
            if (customer == null) {
                return NotFound($"Cannot find customer with id {id}");
            }
            return PartialView("_DetailsPartial", customer);
        }
    }
}