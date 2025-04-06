using Bus_Station_Ticket_Management.DataAccess;
using Bus_Station_Ticket_Management.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Bus_Station_Ticket_Management.Areas.Admin.Controllers
{
    [Area("Admin")]
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
            var applicationDbContext = _context.TripDriverAssignments.Include(t => t.Driver).Include(t => t.Trip);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Admin/TripDriverAssignment/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) {
                return NotFound();
            }

            var tripDriverAssignment = await _context.TripDriverAssignments
                .Include(t => t.Driver)
                .Include(t => t.Trip)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tripDriverAssignment == null) {
                return NotFound();
            }

            return View(tripDriverAssignment);
        }

        // GET: Admin/TripDriverAssignment/Create
        public async Task<IActionResult> Create()
        {
            var trips = await _context.Trips
                .Include(t => t.Route)
                    .ThenInclude(r => r.StartLocation)
                .Include(t => t.Route)
                    .ThenInclude(r => r.DestinationLocation)
                .Include(t => t.Vehicle)
                .ToListAsync();

            var tripSelectList = trips.Select(t => new {
                t.Id,
                Name = $"{t.Route?.StartLocation?.Name} → {t.Route?.DestinationLocation?.Name} | {t.Vehicle?.Name} | {t.DepartureTime:g}"
            }).ToList();

            ViewData["TripId"] = new SelectList(tripSelectList, "Id", "Name");
            ViewData["DriverId"] = new SelectList(_context.Drivers, "Id", "FullName");
            return View();
        }

        // POST: Admin/TripDriverAssignment/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TripId,DriverId,DateAssigned")] TripDriverAssignment tripDriverAssignment)
        {
            var trips = await _context.Trips
                .Include(t => t.Route)
                    .ThenInclude(r => r.StartLocation)
                .Include(t => t.Route)
                    .ThenInclude(r => r.DestinationLocation)
                .Include(t => t.Vehicle)
                .ToListAsync();

            if (ModelState.IsValid) {
                _context.Add(tripDriverAssignment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            var tripSelectList = trips.Select(t => new {
                t.Id,
                Name = $"{t.Route?.StartLocation?.Name} → {t.Route?.DestinationLocation?.Name} | {t.Vehicle?.Name} | {t.DepartureTime:g}"
            }).ToList();

            ViewData["TripId"] = new SelectList(tripSelectList, "Id", "Name");
            ViewData["DriverId"] = new SelectList(_context.Drivers, "Id", "FullName", tripDriverAssignment.DriverId);
            
            return View(tripDriverAssignment);
        }

        // GET: Admin/TripDriverAssignment/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) {
                return NotFound();
            }

            var tripDriverAssignment = await _context.TripDriverAssignments.FindAsync(id);
            if (tripDriverAssignment == null) {
                return NotFound();
            }
            ViewData["DriverId"] = new SelectList(_context.Drivers, "Id", "FullName", tripDriverAssignment.DriverId);
            ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Name", tripDriverAssignment.TripId);
            return View(tripDriverAssignment);
        }

        // POST: Admin/TripDriverAssignment/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TripId,DriverId,DateAssigned")] TripDriverAssignment tripDriverAssignment)
        {
            if (id != tripDriverAssignment.Id) {
                return NotFound();
            }

            if (ModelState.IsValid) {
                try {
                    _context.Update(tripDriverAssignment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) {
                    if (!TripDriverAssignmentExists(tripDriverAssignment.Id)) {
                        return NotFound();
                    }
                    else {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["DriverId"] = new SelectList(_context.Drivers, "Id", "FullName", tripDriverAssignment.DriverId);
            ViewData["TripId"] = new SelectList(_context.Trips, "Id", "Name", tripDriverAssignment.TripId);
            return View(tripDriverAssignment);
        }

        // GET: Admin/TripDriverAssignment/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) {
                return NotFound();
            }

            var tripDriverAssignment = await _context.TripDriverAssignments
                .Include(t => t.Driver)
                .Include(t => t.Trip)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tripDriverAssignment == null) {
                return NotFound();
            }

            return View(tripDriverAssignment);
        }

        // POST: Admin/TripDriverAssignment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tripDriverAssignment = await _context.TripDriverAssignments.FindAsync(id);
            if (tripDriverAssignment != null) {
                _context.TripDriverAssignments.Remove(tripDriverAssignment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TripDriverAssignmentExists(int id)
        {
            return _context.TripDriverAssignments.Any(e => e.Id == id);
        }
    }
}
