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

        public TripController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Trip
        public async Task<IActionResult> Index(string? searchString, int? page, string? sortBy, string? filterBy)
        {
            int pageSize = 30;
            int pageNumber = page ?? 1;
            var now = DateTime.Now;

            // Bulk update trip statuses directly in the database
            await _context.Trips
                .Where(t => t.DepartureTime > now && t.Status != "Stand By")
                .ExecuteUpdateAsync(t => t.SetProperty(t => t.Status, "Stand By"));

            await _context.Trips
                .Where(t => t.DepartureTime <= now && t.ArrivalTime > now && t.Status != "In Progress")
                .ExecuteUpdateAsync(t => t.SetProperty(t => t.Status, "In Progress"));

            await _context.Trips
                .Where(t => t.ArrivalTime <= now && t.Status != "Completed")
                .ExecuteUpdateAsync(t => t.SetProperty(t => t.Status, "Completed"));

            // Start with the base query
            var tripsQuery = _context.Trips
                .Include(t => t.Route)
                    .ThenInclude(r => r.StartLocation)
                .Include(t => t.Route)
                    .ThenInclude(r => r.DestinationLocation)
                .Include(t => t.Vehicle)
                .AsQueryable();

            // Apply search filter if searchString is provided
            if (!string.IsNullOrEmpty(searchString))
            {
                tripsQuery = tripsQuery.Where(t =>
                    t.Route != null && t.Route.StartLocation != null && t.Route.StartLocation.Name.Contains(searchString) ||
                    t.Route != null && t.Route.DestinationLocation != null && t.Route.DestinationLocation.Name.Contains(searchString) ||
                    t.Vehicle != null && t.Vehicle.Name != null && t.Vehicle.Name.Contains(searchString)
                );
            }

            // Apply sorting based on sortBy parameter
            tripsQuery = sortBy switch
            {
                "route_asc" => tripsQuery.OrderBy(t => t.Route.StartLocation.Name),
                "route_desc" => tripsQuery.OrderByDescending(t => t.Route.StartLocation.Name),
                "vehicle_asc" => tripsQuery.OrderBy(t => t.Vehicle.Name),
                "vehicle_desc" => tripsQuery.OrderByDescending(t => t.Vehicle.Name),
                "departure_asc" => tripsQuery.OrderBy(t => t.DepartureTime),
                "departure_desc" => tripsQuery.OrderByDescending(t => t.DepartureTime),
                "arrival_asc" => tripsQuery.OrderBy(t => t.ArrivalTime),
                "arrival_desc" => tripsQuery.OrderByDescending(t => t.ArrivalTime),
                _ => tripsQuery.OrderBy(t => t.DepartureTime),
            };

            // Apply filtering based on filterBy parameter
            tripsQuery = filterBy switch
            {
                "All" => tripsQuery,
                "Stand By" => tripsQuery.Where(t => t.Status == "Stand By"),
                "In Progress" => tripsQuery.Where(t => t.Status == "In Progress"),
                "Completed" => tripsQuery.Where(t => t.Status == "Completed"),
                _ => tripsQuery,
            };

            // Retrieve total count for pagination
            int totalCount = await tripsQuery.CountAsync();

            // Apply pagination
            var trips = await tripsQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Set ViewBag properties for use in the view
            ViewBag.Routes = await _context.Routes
                .Include(r => r.StartLocation)
                .Include(r => r.DestinationLocation)
                .ToListAsync();

            ViewBag.SortBy = sortBy;
            ViewBag.SearchString = searchString;
            ViewBag.FilterBy = filterBy;
            ViewBag.PageNumber = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return View(trips.ToPagedList(pageNumber, pageSize));
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

        // POST: Trip/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IsTwoWay,DepartureTime,ArrivalTime,RouteId,VehicleId")] Trip trip)
        {
            if (trip.DepartureTime > trip.ArrivalTime)
            {
                ModelState.AddModelError(string.Empty, "Invalid Time!");
            }

            var existingTrip = await _context.Trips.FirstOrDefaultAsync(t =>
                t.IsTwoWay == trip.IsTwoWay &&
                t.DepartureTime == trip.DepartureTime &&
                t.ArrivalTime == trip.ArrivalTime &&
                t.RouteId == trip.RouteId
            );

            if (existingTrip != null)
            {
                ModelState.AddModelError(string.Empty, "Trip already exists!");
            }
            bool isOverlapping = await _context.Trips.AnyAsync(t => 
                t.Vehicle != null && trip.Vehicle != null &&
                t.Vehicle.Id == trip.Vehicle.Id &&
                t.DepartureTime < trip.ArrivalTime &&
                t.ArrivalTime > trip.DepartureTime
            );

            if (isOverlapping) {
                ModelState.AddModelError(string.Empty, "Trip is overlapping another trip!");
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

            var vehicle = await _context.Vehicles
                .Include(v => v.VehicleType)
                .FirstOrDefaultAsync(v => v.Id == trip.VehicleId);

            var route = await _context.Routes.FirstOrDefaultAsync(r => r.Id == trip.RouteId);

            if (vehicle == null)
                return NotFound("Error: Cannot find vehicle!");
            
            if (route == null)
                return NotFound("Error: Cannot find route!");

            trip.Status = "Stand By";
            var vehicleType = vehicle.VehicleType;
            if (vehicleType == null) {
                return NotFound("Error: Cannot find vehicle type for the equivalent vehicle!");
            }
            trip.TotalPrice = trip.IsTwoWay ? vehicleType.Price + (route.Price * 2) : vehicleType.Price + route.Price;

            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();

            // Generate seats after saving trip
            var seats = new List<Seat>();
            for (int floor = 1; floor <= vehicleType.TotalFlooring; floor++) {
                for (int r = 1; r <= vehicleType.TotalRow; r++)
                {
                    for (int c = 1; c <= vehicleType.TotalColumn; c++)
                    {
                        seats.Add(new Seat
                        {
                            Row = r,
                            Column = c,
                            Floor = floor,
                            Number = $"R{r}C{c}",
                            IsAvailable = true,
                            TripId = trip.Id
                        });
                    }
                }
            }

            await _context.Seats.AddRangeAsync(seats);
            await _context.SaveChangesAsync();

            return RedirectToAction("Create", "TripDriverAssignment", new { tripId = trip.Id });
        }

        // GET
        [HttpGet]
        public async Task<IActionResult> GetVehicleInfo(int vehicleId){
            var vehicle = await _context.Vehicles
            .Include(x => x.VehicleType)
            .FirstOrDefaultAsync(v => v.Id == vehicleId);

            if (vehicle == null)
            {
                return NotFound();
            }

            return Json(new {
                Name = vehicle.Name,
                LicensePlate = vehicle.LicensePlate,
                VehicleType = vehicle.VehicleType?.Name,
                TotalSeats = vehicle.VehicleType?.TotalSeats,
                TotalFloors = vehicle.VehicleType?.TotalFlooring,
                TotalColumns = vehicle.VehicleType?.TotalColumn,
                TotalRows = vehicle.VehicleType?.TotalRow,
            });
        }

        // GET: Trip/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trip = await _context.Trips.FindAsync(id);
            if (trip == null)
            {
                return NotFound();
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

            if (!routes.Any())
            {
                ModelState.AddModelError("", "No routes available. Please add routes before editing a trip.");
                return View(trip);
            }

            // Ensure Vehicles are properly loaded
            var vehicles = await _context.Vehicles.ToListAsync();
            if (!vehicles.Any())
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
                return NotFound();
            }

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
                        return NotFound();
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
                return NotFound();
            }

            var trip = await _context.Trips
                .Include(t => t.Route)
                    .ThenInclude(r => r.StartLocation)
                .Include(t => t.Route)
                    .ThenInclude(r => r.DestinationLocation)
                .Include(t => t.Vehicle)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (trip == null)
            {
                return NotFound();
            }

            return View(trip);
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
                System.Diagnostics.Debug.WriteLine($"Trip Deletion Database Error: {dbEx}");
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
                System.Diagnostics.Debug.WriteLine($"Trip Deletion Exception: {ex}");
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
