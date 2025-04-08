using Bus_Station_Ticket_Management.DataAccess;
using Bus_Station_Ticket_Management.Models;
using Microsoft.AspNetCore.Authorization;
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

        public DriverController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Driver
        [Route("Admin/Driver/Index")]
        public async Task<IActionResult> Index(string? searchString, int? page, string? sortBy, string? filterByStatus, string? filterByGender)
        {
            int pageSize = 15;
            int pageNumber = page ?? 1;

            var driversQuery = _context.Drivers.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                driversQuery = driversQuery.Where(d =>
                    d.FullName.Contains(searchString) ||
                    d.Email.Contains(searchString) ||
                    d.LicenseId.Contains(searchString)
                );
            }

            // Filter by type
            if (!string.IsNullOrEmpty(filterByGender))
            {
                driversQuery = driversQuery.Where(d => d.Gender == filterByGender);
            }

            driversQuery = sortBy switch
            {
                "name_asc" => driversQuery.OrderBy(d => d.FullName),
                "name_desc" => driversQuery.OrderByDescending(d => d.FullName),
                "email_asc" => driversQuery.OrderBy(d => d.Email),
                "email_desc" => driversQuery.OrderByDescending(d => d.Email),
                "license_asc" => driversQuery.OrderBy(d => d.LicenseId),
                "license_desc" => driversQuery.OrderByDescending(d => d.LicenseId),
                _ => driversQuery.OrderBy(d => d.Id) // Default sort
            };

            ViewBag.SortBy = sortBy;
            ViewBag.SearchString = searchString;
            ViewBag.FilterByGender = filterByGender;

            var drivers = await driversQuery.ToListAsync();

            return View(drivers.ToPagedList(pageNumber, pageSize));
        }

        // GET: Driver/Details/5
        public async Task<IActionResult> Details(int? id)
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
                _context.Add(driver);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(driver);
        }

        // GET: Driver/Edit/5
        public async Task<IActionResult> Edit(int? id)
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Address,LicenseId,Gender,DateOfBirth,Email,PhoneNumber")] Driver driver)
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
        public async Task<IActionResult> Delete(int? id)
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

        private bool DriverExists(int id)
        {
            return _context.Drivers.Any(e => e.Id == id);
        }
    }
}
