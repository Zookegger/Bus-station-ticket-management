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

    public class TripController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TripController> _logger;

        public TripController(ApplicationDbContext context, ILogger<TripController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Trip
        public async Task<IActionResult> Index()
        {
            var trips = await _context.Trips
                .Include(t => t.Route)
                    .ThenInclude(r => r.StartLocation)
                .Include(t => t.Route)
                    .ThenInclude(r => r.DestinationLocation)
                .Include(t => t.Vehicle).ToListAsync();

            return View(trips);
        }

        // GET: Trip/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trip = await _context.Trips
                .Include(t => t.Route)
                    .ThenInclude(r => r.StartLocation)
                .Include(t => t.Route)
                    .ThenInclude(r => r.DestinationLocation)
                .Include(t => t.Vehicle)
                    .ThenInclude(v => v.VehicleType)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (trip == null)
            {
                return NotFound();
            }

            return View(trip);
        }

        // GET: Trip/Details/5
        public async Task<IActionResult> DetailsPartial(int? id)
        {
            if (id == null)
            {
                return NotFound("Id is null!");
            }

            var trip = await _context.Trips
                .Include(t => t.Route)
                    .ThenInclude(r => r.StartLocation)
                .Include(t => t.Route)
                    .ThenInclude(r => r.DestinationLocation)
                .Include(t => t.Vehicle)
                    .ThenInclude(v => v.VehicleType)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (trip == null)
            {
                return NotFound("Trip not found!");
            }

            return PartialView("_DetailsPartial", trip);
        }

        // GET: Trip/Create
        public async Task<IActionResult> Create()
        {
            var routes = await _context.Routes
                .Include(r => r.StartLocation)
                .Include(r => r.DestinationLocation)
                .Select(r => new
                {
                    Id = r.Id,
                    Name = r.StartLocation.Name + " - " + r.DestinationLocation.Name
                }).ToListAsync();

            if (!routes.Any())
            {
                ModelState.AddModelError("", "No routes available. Please add routes before editing a trip.");
            }

            // Ensure Vehicles are properly loaded
            var vehicles = await _context.Vehicles.ToListAsync();
            if (!vehicles.Any())
            {
                ModelState.AddModelError("", "No vehicles available. Please add vehicles before editing a trip.");
            }

            ViewBag.RouteId = new SelectList(routes, "Id", "Name");
            ViewBag.VehicleId = new SelectList(_context.Vehicles, "Id", "Name");
            return View();
        }

        public async Task<bool> ValidateTrip(Trip trip, int? tripId = null)
        {
            if (trip.DepartureTime > trip.ArrivalTime)
            {
                ModelState.AddModelError(string.Empty, "Invalid Time!");
            }

            var existingTripQuery = _context.Trips.Where(t =>
                t.IsTwoWay == trip.IsTwoWay &&
                t.DepartureTime == trip.DepartureTime &&
                t.ArrivalTime == trip.ArrivalTime &&
                t.RouteId == trip.RouteId
            );

            // If we're editing, exclude the current trip from the check
            if (tripId.HasValue)
            {
                existingTripQuery = existingTripQuery.Where(t => t.Id != tripId.Value);
            }

            var existingTrip = await existingTripQuery.FirstOrDefaultAsync();

            if (existingTrip != null)
            {
                ModelState.AddModelError(string.Empty, "Trip already exists!");
            }

            var overlappingTripsQuery = _context.Trips.Where(t =>
                t.Vehicle != null && trip.Vehicle != null &&
                t.Vehicle.Id == trip.Vehicle.Id &&
                t.DepartureTime < trip.ArrivalTime &&
                t.ArrivalTime > trip.DepartureTime
            );

            // If we're editing, exclude the current trip from the overlap check
            if (tripId.HasValue)
            {
                overlappingTripsQuery = overlappingTripsQuery.Where(t => t.Id != tripId.Value);
            }

            bool isOverlapping = await overlappingTripsQuery.AnyAsync();

            if (isOverlapping)
            {
                ModelState.AddModelError(string.Empty, "Trip is overlapping another trip!");
            }

            if (!ModelState.IsValid)
            {
                return false;
            }

            return true;
        }

        // POST: Trip/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IsTwoWay,DepartureTime,ArrivalTime,RouteId,VehicleId")] Trip trip)
        {
            if (!await ValidateTrip(trip))
            {
                return View(trip);
            }

            var routes = await _context.Routes
                .Include(r => r.StartLocation)
                .Include(r => r.DestinationLocation)
                .Select(r => new { r.Id, Name = r.StartLocation.Name + " - " + r.DestinationLocation.Name })
                .ToListAsync();

            ViewBag.RouteId = new SelectList(routes, "Id", "Name", trip.RouteId);
            ViewBag.VehicleId = new SelectList(_context.Vehicles, "Id", "Name", trip.VehicleId);

            if (!ModelState.IsValid)
            {
                return View(trip);
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _logger.LogInformation("Starting trip creation process for vehicle {VehicleId} and route {RouteId}", trip.VehicleId, trip.RouteId);

                    var vehicle = await _context.Vehicles
                        .Include(v => v.VehicleType)
                        .FirstOrDefaultAsync(v => v.Id == trip.VehicleId);

                    var route = await _context.Routes.FirstOrDefaultAsync(r => r.Id == trip.RouteId);

                    if (vehicle == null)
                    {
                        _logger.LogWarning("Vehicle not found with ID: {VehicleId}", trip.VehicleId);
                        return NotFound("Error: Cannot find vehicle!");
                    }

                    if (route == null)
                    {
                        _logger.LogWarning("Route not found with ID: {RouteId}", trip.RouteId);
                        return NotFound("Error: Cannot find route!");
                    }

                    trip.Status = "Standby";
                    var vehicleType = vehicle.VehicleType;
                    if (vehicleType == null)
                    {
                        _logger.LogWarning("Vehicle type not found for vehicle ID: {VehicleId}", vehicle.Id);
                        return NotFound("Error: Cannot find vehicle type for the equivalent vehicle!");
                    }

                    trip.TotalPrice = trip.IsTwoWay ? vehicleType.Price + (route.Price * 2) : vehicleType.Price + route.Price;

                    _context.Trips.Add(trip);
                    _logger.LogInformation("Added trip to context. Attempting to save changes...");
                    
                    var result = await _context.SaveChangesAsync();
                    if (result <= 0)
                    {
                        _logger.LogError("Failed to save trip to database. SaveChanges returned {Result}", result);
                        await transaction.RollbackAsync();
                        ModelState.AddModelError(string.Empty, "Failed to save trip to database.");
                        return View(trip);
                    }

                    _logger.LogInformation("Trip saved successfully. Generating seats...");
                    
                    // Generate seats after saving trip
                    var seats = GenerateSeats(vehicleType, trip.Id);
                    if (seats == null || seats.Count == 0)
                    {
                        _logger.LogError("Failed to generate seats for trip {TripId}", trip.Id);
                        await transaction.RollbackAsync();
                        ModelState.AddModelError(string.Empty, "Error: Cannot generate seats!");
                        return View(trip);
                    }

                    await _context.Seats.AddRangeAsync(seats);
                    result = await _context.SaveChangesAsync();
                    
                    if (result <= 0)
                    {
                        _logger.LogError("Failed to save seats to database. SaveChanges returned {Result}", result);
                        await transaction.RollbackAsync();
                        ModelState.AddModelError(string.Empty, "Error: Cannot save seats!");
                        return View(trip);
                    }

                    _logger.LogInformation("Successfully created trip {TripId} with {SeatCount} seats", trip.Id, seats.Count);
                    await transaction.CommitAsync();

                    TempData["Success"] = "Trip created successfully!";
                    return RedirectToAction("CreateWithPreselectedTrip", "TripDriverAssignment", new { tripId = trip.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating trip: {Message}", ex.Message);
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, $"An error occurred while creating the trip: {ex.Message}");
                    return View(trip);
                }
            }
        }

        // Optimized version of GenerateSeats
        public List<Seat> GenerateSeats(VehicleType vehicleType, int tripId)
        {
            var seats = new List<Seat>();

            for (int floor = 0; floor < vehicleType.TotalFloors; floor++)
            {
                int seatsForFloor = vehicleType.SeatsPerFloor[floor];
                int columns = vehicleType.TotalColumns;
                string prefix = floor == 0 ? "A" : "B";

                for (int seatIndex = 0; seatIndex < seatsForFloor; seatIndex++)
                {
                    seats.Add(new Seat
                    {
                        Row = (seatIndex / columns) + 1,
                        Column = (seatIndex % columns) + 1,
                        Floor = floor + 1,
                        Number = $"{prefix}{seatIndex + 1}",
                        IsAvailable = true,
                        TripId = tripId
                    });
                }
            }

            return seats;
        }

        // GET: Trip/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound("Id is null!");
            }

            var trip = await _context.Trips.FindAsync(id);
            if (trip == null)
            {
                return NotFound("Trip not found!");
            }

            // Ensure Routes are properly loaded
            var routes = await _context.Routes
                .Include(r => r.StartLocation)
                .Include(r => r.DestinationLocation)
                .Select(r => new
                {
                    Id = r.Id,
                    Name = r.StartLocation.Name + " - " + r.DestinationLocation.Name
                }).ToListAsync();

            if (routes.Count == 0 || routes == null)
            {
                ModelState.AddModelError("", "No routes available. Please add routes before editing a trip.");
                return View(trip);
            }

            // Ensure Vehicles are properly loaded
            var vehicles = await _context.Vehicles.ToListAsync();
            if (vehicles.Count == 0 || vehicles == null)
            {
                ModelState.AddModelError("", "No vehicles available. Please add vehicles before editing a trip.");
                return View(trip);
            }

            ViewBag.RouteId = new SelectList(routes, "Id", "Name", trip.RouteId);
            ViewBag.VehicleId = new SelectList(vehicles, "Id", "Name", trip.VehicleId);

            return View(trip);
        }

        // POST: Trip/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DepartureTime,ArrivalTime,Status,TotalPrice,RouteId,VehicleId")] Trip trip)
        {
            if (id != trip.Id)
            {
                return NotFound("Id is not valid!");
            }

            if (!await ValidateTrip(trip, id))
            {
                return View(trip);
            }

            // Need to handle the case when the trip is already assigned to a driver or customers already booked a ticket

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trip);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TripExists(trip.Id))
                    {
                        return NotFound("Trip not found!");
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.RouteId = new SelectList(_context.Routes, "Id", "Name", trip.RouteId);
            ViewBag.VehicleId = new SelectList(_context.Vehicles, "Id", "name", trip.VehicleId);
            return View(trip);
        }

        // GET: Trip/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound("Id is null!");
            }

            try
            {
                var trip = await _context.Trips
                    .Include(t => t.Route)
                        .ThenInclude(r => r.StartLocation)
                .Include(t => t.Route)
                    .ThenInclude(r => r.DestinationLocation)
                .Include(t => t.Vehicle)
                .FirstOrDefaultAsync(m => m.Id == id);

                if (trip == null)
                {
                    return NotFound("Trip not found!");
                }

                return View(trip);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting trip");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
                return View("Error");
            }
        }

        // POST: Trip/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var trip = await _context.Trips.FindAsync(id);
                if (trip != null)
                {
                    _context.Trips.Remove(trip);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dbEx)
            {
                var trip = await _context.Trips
                    .Include(t => t.Route)
                    .Include(t => t.Vehicle)
                    .FirstOrDefaultAsync(t => t.Id == id);
                _logger.LogError(dbEx, "Error deleting trip");
                ModelState.AddModelError(string.Empty, "Cannot delete this trip because it has related data such as tickets.");
                return View(trip);
            }
            catch (Exception ex)
            {
                var trip = await _context.Trips
                    .Include(t => t.Route)
                    .Include(t => t.Vehicle)
                    .FirstOrDefaultAsync(t => t.Id == id);

                // Optional: Log ex.ToString() for diagnostics
                _logger.LogError(ex, "Error deleting trip");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
                return View(trip);
            }
        }

        private bool TripExists(int id)
        {
            return _context.Trips.Any(e => e.Id == id);
        }
    }
}
