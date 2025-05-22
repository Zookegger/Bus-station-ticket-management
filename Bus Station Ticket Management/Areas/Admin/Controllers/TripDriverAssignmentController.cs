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
            try
            {
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
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View();
            }
        }

        // GET: Admin/TripDriverAssignment/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
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
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        public async Task<IActionResult> DetailsPartial(int? id)
        {
            try
            {
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

                if (tripDriverAssignment == null)
                {
                    return NotFound();
                }

                return PartialView("_DetailsPartial", tripDriverAssignment);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        // GET: Admin/TripDriverAssignment/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                // Get all trips for the dropdown
                var trips = await _context.Trips
                    .Include(t => t.Route)
                        .ThenInclude(r => r.StartLocation)
                    .Include(t => t.Route)
                        .ThenInclude(r => r.DestinationLocation)
                    .Include(t => t.Vehicle)
                    .Where(t => t.Status == "Standby" && !_context.TripDriverAssignments.Any(a => a.TripId == t.Id))
                    .Select(t => new
                    {
                        t.Id,
                        Name = $"{t.Route.StartLocation.Name} → {t.Route.DestinationLocation.Name} | {t.Vehicle.Name} | {t.DepartureTime:g}"
                    })
                    .ToListAsync();

                ViewData["TripId"] = new SelectList(trips, "Id", "Name");

                return View(new TripDriverAssignment());
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return View(new TripDriverAssignment());
            }
        }

        public async Task<IActionResult> CreateWithPreselectedTrip(int? tripId)
        {
            try
            {
                if (tripId == null)
                {
                    // No preselected trip — show dropdown
                    return RedirectToAction(nameof(Create));
                }
                else
                {
                    // Get the actual Trip entity instead of anonymous object
                    var trip = await _context.Trips
                        .Include(t => t.Route)
                            .ThenInclude(r => r.StartLocation)
                        .Include(t => t.Route)
                            .ThenInclude(r => r.DestinationLocation)
                        .Include(t => t.Vehicle)
                        .Where(t => t.Id == tripId)
                        .FirstOrDefaultAsync();

                    if (trip == null)
                    {
                        return NotFound($"Cannot find trip with id: {tripId}");
                    }

                    // Now pass the actual Trip entity to FreeDrivers
                    List<Driver> totalFreeDrivers = await FreeDrivers(trip);

                    // Check if there are any free drivers
                    if (totalFreeDrivers.Count == 0)
                    {
                        ModelState.AddModelError("", "No drivers available.");
                        return View();
                    }

                    ViewBag.DriverId = new SelectList(
                        totalFreeDrivers.Select(d => new
                        {
                            Id = d.Id,
                            FullName = d.Account.FullName
                        }), "Id", "FullName"
                    );

                    ViewData["TripId"] = tripId;
                    ViewData["TripDisplayName"] = $"{trip.Route.StartLocation?.Name ?? "N/A"} → {trip.Route.DestinationLocation?.Name ?? "N/A"}";
                    ViewData["TripVehicle"] = $"{trip.Vehicle?.Name} : {trip.Vehicle?.LicensePlate}";
                    ViewData["TripTime"] = $"{trip.DepartureTime:g} → {trip.ArrivalTime:g}"; // Fixed: was showing DepartureTime twice

                    return View("CreateWithPreselectedTrip", new TripDriverAssignment { TripId = tripId.Value });
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return View();
            }
        }

        // Fix the FreeDrivers method - remove the invalid generic syntax
        public async Task<List<Driver>> FreeDrivers(Trip trip)
        {
            // Get busy drivers at that time frame
            var busyDrivers = await _context.TripDriverAssignments
                .Include(tda => tda.Trip)
                .Where(tda =>
                    tda != null &&
                    tda.Trip != null &&
                    // tda.Trip: Existing Trip
                    // trip: New Trip
                    tda.Trip.DepartureTime < trip.ArrivalTime && tda.Trip.ArrivalTime > trip.DepartureTime
                )
                .Select(tda => tda.DriverId)
                .ToListAsync();

            // free drivers are queried by checking if there are no assignments at that time frame
            var freeDrivers = await _context.Drivers
                .Include(d => d.Account)
                .Where(d =>
                    d.Account != null &&
                    d.Account.FullName != null &&
                    !busyDrivers.Contains(d.Id)
                )
                .ToListAsync();

            return freeDrivers;
        }

        // POST: Admin/TripDriverAssignment/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TripId,DriverId")] TripDriverAssignment tripDriverAssignment)
        {
            try
            {
                var trip = await _context.Trips.FirstOrDefaultAsync(t => t.Id == tripDriverAssignment.TripId);

                if (trip == null)
                {
                    ModelState.AddModelError("TripId", "Trip not found");
                    return View(tripDriverAssignment);
                }

                tripDriverAssignment.DateAssigned = DateTime.Now;
                tripDriverAssignment.LastUpdated = DateTime.Now;

                var hasOverlap = await _context.TripDriverAssignments
                    .Include(tda => tda.Trip)
                    .AnyAsync(tda =>
                        tda.DriverId == tripDriverAssignment.DriverId &&
                        tda.TripId == tripDriverAssignment.TripId &&
                        tda.DateAssigned >= trip.DepartureTime &&
                        tda.DateAssigned <= trip.ArrivalTime);

                if (hasOverlap)
                {
                    ModelState.AddModelError("TripId", "The selected trip is already assigned at that time frame.");
                    return View(tripDriverAssignment);
                }

                var scheduleOverlap = await _context.TripDriverAssignments
                    .Include(tda => tda.Trip)
                    .AnyAsync(tda =>
                        tda.DriverId == tripDriverAssignment.DriverId &&
                        tda.DateAssigned >= trip.DepartureTime &&
                        tda.DateAssigned <= trip.ArrivalTime);

                if (scheduleOverlap)
                {
                    ModelState.AddModelError("DriverId", "The selected driver is already assigned to another trip at that time frame.");
                    return View(tripDriverAssignment);
                }

                if (ModelState.IsValid)
                {
                    _context.Add(tripDriverAssignment);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }


                // If failed to create, show the form again with the preselected trip and driver
                var trips = await _context.Trips
                    .Include(t => t.Route)
                        .ThenInclude(r => r.StartLocation)
                    .Include(t => t.Route)
                        .ThenInclude(r => r.DestinationLocation)
                    .Include(t => t.Vehicle)
                    .Where(t => t.Route != null && t.Route.StartLocation != null && t.Route.DestinationLocation != null)
                    .ToListAsync();

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
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return View(tripDriverAssignment);
            }
        }

        // GET: Admin/TripDriverAssignment/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound("Id is null");
                }

                var tripDriverAssignment = await _context.TripDriverAssignments.FindAsync(id);
                if (tripDriverAssignment == null)
                {
                    return NotFound("Assignment not found");
                }

                var trip = await _context.Trips.FindAsync(tripDriverAssignment.TripId);
                if (trip == null)
                {
                    return NotFound("Trip not found");
                }

                var newTrips = await _context.Trips
                    .Include(t => t.Route)
                        .ThenInclude(r => r.StartLocation)
                    .Include(t => t.Route)
                        .ThenInclude(r => r.DestinationLocation)
                    .Include(t => t.Vehicle)
                    .Where(t => t.Status == "Standby" && !_context.TripDriverAssignments.Any(a => a.TripId == t.Id))
                    .ToListAsync();

                // Now pass the actual Trip entity to FreeDrivers
                List<Driver> totalFreeDrivers = await FreeDrivers(trip);

                // Check if there are any free drivers
                if (totalFreeDrivers.Count == 0)
                {
                    ModelState.AddModelError("", "No drivers available.");
                    return View();
                }

                var tripSelectList = newTrips.Select(t => new
                {
                    t.Id,
                    Name = $"{t.Route?.StartLocation?.Name} → {t.Route?.DestinationLocation?.Name} | {t.Vehicle?.Name} | {t.DepartureTime:g}"
                }).ToList();

                ViewData["TripId"] = new SelectList(tripSelectList, "Id", "Name", tripDriverAssignment.TripId);
                ViewData["DriverId"] = new SelectList(_context.Drivers, "Id", "FullName", tripDriverAssignment.DriverId);

                return View(tripDriverAssignment);
            }
            catch (Exception ex)
            {
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
            try
            {
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
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        // POST: Admin/TripDriverAssignment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var tripDriverAssignment = await _context.TripDriverAssignments.FindAsync(id);
                if (tripDriverAssignment != null)
                {
                    _context.TripDriverAssignments.Remove(tripDriverAssignment);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        private bool TripDriverAssignmentExists(int id)
        {
            return _context.TripDriverAssignments.Any(e => e.Id == id);
        }
    }
}
