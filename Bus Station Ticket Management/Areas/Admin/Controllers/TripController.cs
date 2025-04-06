using Bus_Station_Ticket_Management.DataAccess;
using Bus_Station_Ticket_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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
        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var trips = await _context.Trips.Include(t => t.Route).Include(t => t.Vehicle).ToListAsync();

            foreach (var trip in trips)
            {
                if (trip.DepartureTime > now)
                {
                    trip.Status = "Stand By";
                }
                else if (trip.DepartureTime <= now && trip.ArrivalTime > now)
                {
                    trip.Status = "In Progress";
                }
                else if (trip.DepartureTime < now && trip.ArrivalTime <= now)
                {
                    trip.Status = "Completed";
                }
            }

            ViewBag.Routes = await _context.Routes
                .Include(r => r.StartLocation)
                .Include(r => r.DestinationLocation)
                .ToListAsync();

            await _context.SaveChangesAsync();

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
                .Include(t => t.Vehicle)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (trip == null)
            {
                return NotFound();
            }

            return View(trip);
        }

        // GET: Trip/Create
        public IActionResult Create()
        {
            var routes = _context.Routes
                .Include(r => r.StartLocation)
                .Include(r => r.DestinationLocation).
                Select(r => new
                {
                    Id = r.Id,
                    Name = r.StartLocation.Name + " - " + r.DestinationLocation.Name
                }).ToList();

            ViewData["RouteId"] = new SelectList(routes, "Id", "Name");
            ViewData["VehicleId"] = new SelectList(_context.Vehicles, "Id", "Name");
            return View();
        }

        // POST: Trip/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DepartureTime,ArrivalTime,RouteId,VehicleId")] Trip trip)
        {
            if (ModelState.IsValid)
            {
                trip.Status = "StandBy";

                var vehicle = await _context.Vehicles
                    .Include(v => v.VehicleType)
                    .FirstOrDefaultAsync(v => v.Id == trip.VehicleId);

                var route = await _context.Routes
                    .FirstOrDefaultAsync(r => r.Id == trip.RouteId);

                if (vehicle == null || route == null)
                    return NotFound();

                int vehiclePrice = vehicle.VehicleType.Price;
                int routePrice = route.Price;

                int total = (trip.IsTwoWay) ? vehiclePrice + (routePrice * 2) : vehiclePrice + routePrice;

                trip.TotalPrice = total;

                int rows = vehicle.VehicleType.TotalRow;
                int columns = vehicle.VehicleType.TotalColumn;
                int floor = vehicle.VehicleType.TotalFlooring;

                _context.Add(trip);
                await _context.SaveChangesAsync();

                for (int r = 1; r <= rows; r++) {
                    for (int c = 1; c <= columns; c++) {
                        _context.Seats.Add(new Seat {
                            Row = r,
                            Column = c,
                            Floor = floor,
                            Number = $"R{r}C{c}",
                            IsAvailable = true,
                            TripId = trip.Id
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["RouteId"] = new SelectList(_context.Routes, "Id", "Name", trip.RouteId);
            ViewData["VehicleId"] = new SelectList(_context.Vehicles, "Id", "Name", trip.VehicleId);
            return View(trip);
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
            ViewData["RouteId"] = new SelectList(_context.Routes, "Id", "Name", trip.RouteId);
            ViewData["VehicleId"] = new SelectList(_context.Vehicles, "Id", "Name", trip.VehicleId);
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
            ViewData["RouteId"] = new SelectList(_context.Routes, "Id", "Name", trip.RouteId);
            ViewData["VehicleId"] = new SelectList(_context.Vehicles, "Id", "name", trip.VehicleId);
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
            var trip = await _context.Trips.FindAsync(id);
            if (trip != null)
            {
                _context.Trips.Remove(trip);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TripExists(int id)
        {
            return _context.Trips.Any(e => e.Id == id);
        }
    }
}
