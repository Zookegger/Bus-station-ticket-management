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
    [Route("Admin/[controller]/[action]")]
    public class VehicleController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VehicleController> _logger;

        public VehicleController(ApplicationDbContext context, ILogger<VehicleController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Vehicle
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var vehicles = _context.Vehicles.Include(v => v.VehicleType).AsQueryable();

                // Update vehicle statuses based on trips
                var vehiclesWithTrips = await _context.Trips.Include(t => t.Vehicle).ToListAsync();
                await UpdateVehicleStatuses(vehiclesWithTrips);

                // Pass values back to view
                ViewBag.VehicleTypes = new SelectList(_context.VehicleTypes, "Id", "Name");

                return View(await vehicles.ToListAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting index");
                return View(new List<Vehicle>());
            }
        }

        public async Task UpdateVehicleStatuses(List<Trip> vehiclesWithTrips) 
        {
            try
            {
                var now = DateTime.Now;

                foreach (var trip in vehiclesWithTrips)
                {
                    if (trip == null || trip.Vehicle == null)
                    {
                        continue;
                    }
                    if (trip.DepartureTime <= now && trip.ArrivalTime > now)
                    {
                        trip.Vehicle.Status = "In-Progress";
                    }
                    else if (trip.ArrivalTime <= now)
                    {
                        trip.Vehicle.Status = "Standby";
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle statuses");
            }
        }

        // GET: Vehicle/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var vehicle = await _context.Vehicles
                    .Include(v => v.VehicleType)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (vehicle == null)
                {
                    return NotFound("No vehicle found");
                }

                return View(vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting details");
                return NotFound($"Error getting data: {ex.Message}");
            }
        }

        public async Task<IActionResult> DetailsPartial(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound("Id is null");
                }

                var vehicle = await _context.Vehicles
                    .Include(v => v.VehicleType)
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (vehicle == null)
                {
                    return NotFound("No vehicle found");
                }

                return PartialView("_DetailsPartial", vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting details");
                return NotFound($"Error getting data: {ex.Message}");
            }
        }

        // GET: Vehicle/Create
        public IActionResult Create()
        {
            try
            {
                ViewData["VehicleTypeId"] = new SelectList(_context.VehicleTypes, "Id", "Name");
                return View();
            }
            catch (Exception ex)
            {
                ViewData["VehicleTypeId"] = new SelectList(_context.VehicleTypes, "Id", "Name");
                _logger.LogError(ex, "Error getting create");
                return View(new Vehicle());
            }
        }

        // POST: Vehicle/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,AcquiredDate,LicensePlate,Status,VehicleTypeId")] Vehicle vehicle)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(vehicle);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                ViewData["VehicleTypeId"] = new SelectList(_context.VehicleTypes, "Id", "Name", vehicle.VehicleTypeId);
                return View(vehicle);
            }
            catch (DbUpdateConcurrencyException ex) {
                _logger.LogError(ex, "Error adding vehicle");
                return View(vehicle);
            }
            catch (Exception ex)
            {
                ViewData["VehicleTypeId"] = new SelectList(_context.VehicleTypes, "Id", "Name", vehicle.VehicleTypeId);
                _logger.LogError(ex, "Error getting create");
                return View(vehicle);
            }
        }

        // GET: Vehicle/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound("Id is null");
                }

                var vehicle = await _context.Vehicles.FindAsync(id);
                if (vehicle == null)
                {
                    return NotFound("No vehicle found");
                }
                ViewData["VehicleTypeId"] = new SelectList(_context.VehicleTypes, "Id", "Name", vehicle.VehicleTypeId);
                return View(vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting edit");
                return NotFound("Error getting data");
            }
        }

        // POST: Vehicle/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,AcquiredDate,LicensePlate,Status,VehicleTypeId")] Vehicle vehicle)
        {
            try
            {
                if (id != vehicle.Id)
                {
                    return NotFound("Id does not match");
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        vehicle.LastUpdated = DateTime.Now;
                        _context.Update(vehicle);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        _logger.LogError(ex, "Error updating vehicle");
                        if (!VehicleExists(vehicle.Id))
                        {
                            return NotFound("No vehicle found");
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
                ViewData["VehicleTypeId"] = new SelectList(_context.VehicleTypes, "Id", "Name", vehicle.VehicleTypeId);
                return View(vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting edit");
                return NotFound("Error getting data");
            }
        }

        // GET: Vehicle/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound("Id is null");
                }

                var vehicle = await _context.Vehicles
                    .Include(v => v.VehicleType)
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (vehicle == null)
                {
                    return NotFound("No vehicle found");
                }

                return View(vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting delete");
                return NotFound("Error getting data");
            }
        }

        // POST: Vehicle/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var vehicle = await _context.Vehicles.FindAsync(id);
                if (vehicle != null)
                {
                    _context.Vehicles.Remove(vehicle);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting delete");
                return NotFound("Error getting data");
            }
        }

        private bool VehicleExists(int id)
        {
            return _context.Vehicles.Any(e => e.Id == id);
        }
    }
}
