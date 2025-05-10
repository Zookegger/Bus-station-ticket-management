using Bus_Station_Ticket_Management.DataAccess;
using Bus_Station_Ticket_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace Bus_Station_Ticket_Management.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("Admin/[controller]/[action]")]

    public class DriverController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DriverController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Driver
        public async Task<IActionResult> Index()
        {
            try {
                var drivers = await _context.Drivers.ToListAsync();
                return View(drivers);
            }
            catch (Exception ex) {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // GET: Driver/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null) {
                return NotFound();
            }

            var driver = await _context.Drivers.FirstOrDefaultAsync(m => m.Id == id);
            if (driver == null) {
                return NotFound();
            }

            return View(driver);
        }
        
        public async Task<IActionResult> DetailsPartial(string? id)
        {
            if (id == null) {
                return NotFound();
            }

            var driver = await _context.Drivers.FirstOrDefaultAsync(m => m.Id == id);
            if (driver == null) {
                return NotFound();
            }

            return PartialView("_DetailsPartial", driver);
        }

        // GET: Driver/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Driver/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FullName,Address,LicenseId,Gender,DateOfBirth,Email,PhoneNumber")] Driver driver)
        {
            if (ModelState.IsValid) {
                var user = new ApplicationUser {
                    UserName = driver.FullName,
                    Email = driver.Email,
                    FullName = driver.FullName,
                    DateOfBirth = driver.DateOfBirth,
                    Gender = driver.Gender,
                    PhoneNumber = driver.PhoneNumber,
                };

                var result = await _userManager.CreateAsync(user, password: "defaultpassword123");
                if (result.Succeeded) {
                    driver.Id = user.Id;
                    _context.Add(driver);
                    await _userManager.AddToRoleAsync(driver, "Driver");
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(driver);
        }

        // GET: Driver/Edit/5
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null) {
                return NotFound();
            }

            var driver = await _context.Drivers.FindAsync(id);
            if (driver == null) {
                return NotFound();
            }
            return View(driver);
        }

        // POST: Driver/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,FullName,Address,LicenseId,Gender,DateOfBirth,Email,PhoneNumber")] Driver driver)
        {
            if (id != driver.Id) {
                return NotFound();
            }

            if (ModelState.IsValid) {
                try {
                    _context.Update(driver);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) {
                    if (!DriverExists(driver.Id)) {
                        return NotFound();
                    }
                    else {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(driver);
        }

        // GET: Driver/Delete/5
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null) {
                return NotFound();
            }

            var driver = await _context.Drivers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (driver == null) {
                return NotFound();
            }

            return View(driver);
        }

        // POST: Driver/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var driver = await _context.Drivers.FindAsync(id);
            if (driver != null) {
                _context.Drivers.Remove(driver);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DriverExists(string id)
        {
            return _context.Drivers.Any(e => e.Id == id);
        }
    }
}
