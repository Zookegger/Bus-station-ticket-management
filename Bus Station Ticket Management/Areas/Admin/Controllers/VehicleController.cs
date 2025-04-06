using Bus_Station_Ticket_Management.DataAccess;
using Bus_Station_Ticket_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace Bus_Station_Ticket_Management.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class VehicleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VehicleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Vehicle
        public async Task<IActionResult> Index(string? searchString, int? page, string? sortBy)
        {
            int pageSize = 15; // Number of items per page
            int pageNumber = page ?? 1; // Default to page 1

            var vehicles = _context.Vehicles.Include(v => v.VehicleType).AsQueryable();

            if (!string.IsNullOrEmpty(searchString)) {
                vehicles = _context.Vehicles.Where(v =>
                    v.Name.Contains(searchString) ||
                    v.LicensePlate.Contains(searchString) ||
                    v.VehicleType.Name.Contains(searchString)
                );
            }

            switch(sortBy) {
                case "name_asc":
                    vehicles = vehicles.OrderBy(v => v.Name);
                    break;
                case "name_desc":
                    vehicles = vehicles.OrderByDescending(v => v.Name);
                    break;
                case "licenseplate_asc":
                    vehicles = vehicles.OrderBy(v => v.LicensePlate);
                    break;
                case "licenseplate_desc":
                    vehicles = vehicles.OrderByDescending(v => v.LicensePlate);
                    break;
                case "type_asc":
                    vehicles = vehicles.OrderBy(v => v.VehicleType.Name);
                    break;
                case "type_desc":
                    vehicles = vehicles.OrderByDescending(v => v.VehicleType.Name);
                    break;
            }

            ViewBag.SortBy = sortBy;
            ViewBag.SearchString = searchString;

            var vehicleList = await vehicles.ToListAsync();
            return View(vehicleList.ToPagedList(pageNumber, pageSize));
        }

        // GET: Vehicle/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) {
                return NotFound();
            }

            var vehicle = await _context.Vehicles
                .Include(v => v.VehicleType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vehicle == null) {
                return NotFound();
            }

            return View(vehicle);
        }

        // GET: Vehicle/Create
        public IActionResult Create()
        {
            ViewData["VehicleTypeId"] = new SelectList(_context.VehicleTypes, "Id", "Name");
            return View();
        }

        // POST: Vehicle/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,AcquiredDate,LicensePlate,Status,VehicleTypeId")] Vehicle vehicle)
        {
            if (ModelState.IsValid) {
                _context.Add(vehicle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["VehicleTypeId"] = new SelectList(_context.VehicleTypes, "Id", "Name", vehicle.VehicleTypeId);
            return View(vehicle);
        }

        // GET: Vehicle/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) {
                return NotFound();
            }

            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) {
                return NotFound();
            }
            ViewData["VehicleTypeId"] = new SelectList(_context.VehicleTypes, "Id", "Name", vehicle.VehicleTypeId);
            return View(vehicle);
        }

        // POST: Vehicle/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,AcquiredDate,LicensePlate,Status,VehicleTypeId")] Vehicle vehicle)
        {
            if (id != vehicle.Id) {
                return NotFound();
            }

            if (ModelState.IsValid) {
                try {
                    vehicle.LastUpdated = DateTime.Now;
                    _context.Update(vehicle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) {
                    if (!VehicleExists(vehicle.Id)) {
                        return NotFound();
                    }
                    else {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["VehicleTypeId"] = new SelectList(_context.VehicleTypes, "Id", "Name", vehicle.VehicleTypeId);
            return View(vehicle);
        }

        // GET: Vehicle/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) {
                return NotFound();
            }

            var vehicle = await _context.Vehicles
                .Include(v => v.VehicleType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vehicle == null) {
                return NotFound();
            }

            return View(vehicle);
        }

        // POST: Vehicle/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle != null) {
                _context.Vehicles.Remove(vehicle);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VehicleExists(int id)
        {
            return _context.Vehicles.Any(e => e.Id == id);
        }
    }
}
