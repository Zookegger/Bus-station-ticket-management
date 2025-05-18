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

    public class TripDriverAssignmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TripDriverAssignmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/TripDriverAssignment
        public async Task<IActionResult> Index()
        {
            try {
                var assignments = await _context.TripDriverAssignments
                    .Include(t => t.Driver)
                        .ThenInclude(d => d.Account)
                    .Include(t => t.Trip)
                        .ThenInclude(t => t.Route)
                            .ThenInclude(r => r.StartLocation)
                    .Include(t => t.Trip)
                        .ThenInclude(t => t.Route)
                            .ThenInclude(r => r.DestinationLocation)
                    .ToListAsync();

                return View(assignments);
            } catch (Exception ex) {
                ViewBag.ErrorMessage = ex.Message;
                return View();
            }
        }

        // GET: Admin/TripDriverAssignment/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            try {
                if (id == null)
                {
                    return NotFound();
                }

                var tripDriverAssignment = await _context.TripDriverAssignments
                    .Include(t => t.Driver)
                        .ThenInclude(d => d.Account)
                    .Include(t => t.Trip)
                        .ThenInclude(t => t.Route)
                            .ThenInclude(r => r.StartLocation)
                    .Include(t => t.Trip)
                        .ThenInclude(t => t.Route)
                            .ThenInclude(r => r.DestinationLocation)
                    .Include(t => t.Trip)
                        .ThenInclude(t => t.Vehicle)
                            .ThenInclude(v => v.VehicleType)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (tripDriverAssignment == null)
                {
                    return NotFound();
                }

                return View(tripDriverAssignment);
            } catch (Exception ex) {
                return NotFound(ex.Message);
            }
        }

        public async Task<IActionResult> DetailsPartial(int? id) {
            try {
                var tripDriverAssignment = await _context.TripDriverAssignments
                    .Include(t => t.Driver)
                        .ThenInclude(d => d.Account)
                    .Include(t => t.Trip)
                    .ThenInclude(t => t.Route)
                        .ThenInclude(r => r.StartLocation)
                .Include(t => t.Trip)
                    .ThenInclude(t => t.Route)
                        .ThenInclude(r => r.DestinationLocation)
                .Include(t => t.Trip)
                    .ThenInclude(t => t.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                .FirstOrDefaultAsync(tda => tda.Id == id);

                if (tripDriverAssignment == null) {
                    return NotFound();
                }

                return PartialView("_DetailsPartial", tripDriverAssignment);
            } catch (Exception ex) {
                return NotFound(ex.Message);
            }
        }

        // GET: Admin/TripDriverAssignment/Create or Create?tripId=5
        public async Task<IActionResult> Create(int? tripId)
        {
            try {
                // Get available drivers with their account information
                var drivers = await _context.Drivers
                    .Include(d => d.Account)
                    .Where(d => d.Account != null && d.Account.FullName != null)
                    .Select(d => new { d.Id, FullName = d.Account.FullName })
                    .ToListAsync();

                if (!drivers.Any())
                {
                    ModelState.AddModelError("", "No drivers available. Please add drivers before creating an assignment.");
                    return View();
                }

                ViewData["DriverId"] = new SelectList(drivers, "Id", "FullName");

                if (tripId != null)
                {
                    var selectedTrip = await _context.Trips
                        .Include(t => t.Route).ThenInclude(r => r.StartLocation)
                        .Include(t => t.Route).ThenInclude(r => r.DestinationLocation)
                        .Include(t => t.Vehicle)
                        .FirstOrDefaultAsync(t => t.Id == tripId);

                    if (selectedTrip == null)
                        return NotFound($"Cannot find trip with id: {tripId}");

                    ViewData["TripId"] = tripId;
                    ViewData["TripDisplayName"] = $"{selectedTrip.Route?.StartLocation?.Name ?? "N/A"} → {selectedTrip.Route?.DestinationLocation?.Name ?? "N/A"}";
                    ViewData["TripVehicle"] = $"{selectedTrip.Vehicle?.Name ?? "N/A"} : {selectedTrip.Vehicle?.LicensePlate ?? "N/A"}";
                    ViewData["TripTime"] = $"{selectedTrip.DepartureTime:g} → {selectedTrip.DepartureTime:g}";
                    return View("CreateWithPreselectedTrip", new TripDriverAssignment { TripId = tripId.Value });
                }

                // No preselected trip — show dropdown
                var trips = await _context.Trips
                    .Include(t => t.Route).ThenInclude(r => r.StartLocation)
                    .Include(t => t.Route).ThenInclude(r => r.DestinationLocation)
                    .Include(t => t.Vehicle)
                    .Where(t => t.Route != null && t.Route.StartLocation != null && t.Route.DestinationLocation != null)
                    .Select(t => new
                    {
                        t.Id,
                        Name = 
                            (t.Route.StartLocation.Name ?? "N/A") + " → " + (t.Route.DestinationLocation.Name ?? "N/A") + 
                            " | " + (t.Vehicle != null ? t.Vehicle.Name : "No Vehicle") + 
                            " | " + t.DepartureTime.ToString("g")
                    })
                    .ToListAsync();

                if (!trips.Any())
                {
                    ModelState.AddModelError("", "No trips available. Please add trips before creating an assignment.");
                    return View();
                }

                ViewData["TripSelectList"] = new SelectList(trips, "Id", "Name");
                return View();   
            } catch (Exception ex) {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return View();
            }
        }

        // POST: Admin/TripDriverAssignment/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TripId,DriverId")] TripDriverAssignment tripDriverAssignment)
        {
            try {
                var trips = await _context.Trips
                    .Include(t => t.Route)
                        .ThenInclude(r => r.StartLocation)
                    .Include(t => t.Route)
                        .ThenInclude(r => r.DestinationLocation)
                    .Include(t => t.Vehicle)
                    .Where(t => t.Route != null && t.Route.StartLocation != null && t.Route.DestinationLocation != null)
                    .ToListAsync();

                tripDriverAssignment.DateAssigned = DateTime.Now;
                tripDriverAssignment.LastUpdated = DateTime.Now;

                if (ModelState.IsValid)
                {
                    _context.Add(tripDriverAssignment);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                var tripSelectList = trips.Select(t => new
                {
                    t.Id,
                    Name = $"{t.Route?.StartLocation?.Name ?? "N/A"} → {t.Route?.DestinationLocation?.Name ?? "N/A"} | {t.Vehicle?.Name ?? "No Vehicle"} | {t.DepartureTime:g}"
                }).ToList();

                // Get available drivers with their account information
                var drivers = await _context.Drivers
                    .Include(d => d.Account)
                    .Where(d => d.Account != null && d.Account.FullName != null)
                    .Select(d => new { d.Id, FullName = d.Account.FullName })
                    .ToListAsync();

                ViewData["TripId"] = new SelectList(tripSelectList, "Id", "Name");
                ViewData["DriverId"] = new SelectList(drivers, "Id", "FullName", tripDriverAssignment.DriverId);

                return View(tripDriverAssignment);   
            } catch (Exception ex) {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return View(tripDriverAssignment);
            }
        }

        // GET: Admin/TripDriverAssignment/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            try {
                if (id == null)
                {
                    return NotFound();
                }

                var tripDriverAssignment = await _context.TripDriverAssignments.FindAsync(id);
                if (tripDriverAssignment == null)
                {
                    return NotFound();
                }

                var trips = await _context.Trips
                    .Include(t => t.Route)
                        .ThenInclude(r => r.StartLocation)
                    .Include(t => t.Route)
                        .ThenInclude(r => r.DestinationLocation)
                    .Include(t => t.Vehicle)
                    .ToListAsync();

                var tripSelectList = trips.Select(t => new
                {
                    t.Id,
                    Name = $"{t.Route?.StartLocation?.Name} → {t.Route?.DestinationLocation?.Name} | {t.Vehicle?.Name} | {t.DepartureTime:g}"
                }).ToList();

                ViewData["TripId"] = new SelectList(tripSelectList, "Id", "Name", tripDriverAssignment.TripId);
                ViewData["DriverId"] = new SelectList(_context.Drivers, "Id", "FullName", tripDriverAssignment.DriverId);

                return View(tripDriverAssignment);
            } catch (Exception ex) {
                return NotFound(ex.Message);
            }
        }

        // POST: Admin/TripDriverAssignment/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TripId,DriverId")] TripDriverAssignment tripDriverAssignment)
        {
            try
            {
                if (id != tripDriverAssignment.Id)
                {
                    return NotFound();
                }

                tripDriverAssignment.LastUpdated = DateTime.Now;

                if (ModelState.IsValid)
                {
                    _context.Update(tripDriverAssignment);
                    await _context.SaveChangesAsync();
                    
                    return RedirectToAction(nameof(Index));
                }

                ViewData["DriverId"] = new SelectList(_context.Drivers, "Id", "FullName", tripDriverAssignment.DriverId);
                ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Name", tripDriverAssignment.TripId);
                
                return View(tripDriverAssignment);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!TripDriverAssignmentExists(tripDriverAssignment.Id))
                {
                    return NotFound(ex.Message);
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        // GET: Admin/TripDriverAssignment/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            try {
                if (id == null)
                {
                    return NotFound();
                }

                var tripDriverAssignment = await _context.TripDriverAssignments
                    .Include(t => t.Driver)
                        .ThenInclude(d => d.Account)
                    .Include(t => t.Trip)
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (tripDriverAssignment == null)
                {
                    return NotFound();
                }

                return View(tripDriverAssignment);
            } catch (Exception ex) {
                return NotFound(ex.Message);
            }
        }

        // POST: Admin/TripDriverAssignment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try {
                var tripDriverAssignment = await _context.TripDriverAssignments.FindAsync(id);
                if (tripDriverAssignment != null)
                {
                    _context.TripDriverAssignments.Remove(tripDriverAssignment);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            } catch (Exception ex) {
                return NotFound(ex.Message);
            }
        }

        private bool TripDriverAssignmentExists(int id)
        {
            return _context.TripDriverAssignments.Any(e => e.Id == id);
        }

        public async Task<IActionResult> CheckTrips() {
            try {
                // Get all trips that are not assigned and are in "Standby" status
                var unassignedTrips = await _context.Trips
                    .Where(t => t.Status == "Standby" && 
                           !_context.TripDriverAssignments.Any(a => a.TripId == t.Id))
                    .Include(t => t.Route)
                        .ThenInclude(r => r.StartLocation)
                    .Include(t => t.Route)
                        .ThenInclude(r => r.DestinationLocation)
                    .Include(t => t.Vehicle)
                    .ToListAsync();

                if (!unassignedTrips.Any()) {
                    return Json(new {
                        success = false,
                        message = "No available trips to assign. All trips are either assigned or not in 'Standby' status."
                    });
                }

                // Return success with trip count
                return Json(new {
                    success = true,
                    message = $"Found {unassignedTrips.Count} available trip(s) to assign.",
                    tripCount = unassignedTrips.Count
                });
            }
            catch (Exception ex) {
                // Log the exception here if you have logging configured
                return Json(new {
                    success = false,
                    message = "An error occurred while checking available trips." + ex.Message
                });
            }
        }
    }
}
