using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Bus_Station_Ticket_Management.DataAccess;
using Bus_Station_Ticket_Management.Models;

namespace Bus_Station_Ticket_Management.Controllers
{
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
            var applicationDbContext = _context.Trips
                .Include(t => t.Route)
                .ThenInclude(r => r.StartId)  // Load Start Location
                .Include(t => t.Route)
                .ThenInclude(r => r.DestinationId); // Load Destination Location

            return View(await applicationDbContext.ToListAsync());
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
                .ThenInclude(r => r.StartId)
                .Include(t => t.Route)
                .ThenInclude(r => r.DestinationId)
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
            ViewData["RouteId"] = new SelectList(
                _context.Routes
                    .Include(r => r.StartId)
                    .Include(r => r.DestinationId)
                    .Select(r => new {
                        Id = r.Id,
                        Name = r.StartId.Name + " → " + r.DestinationId.Name
                    }).ToList(),
                "Id", "Name"
            );

            return View();
        }

        // POST: Trip/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DepartureTime,ArrivalTime,Status,TotalPrice,RouteId")] Trip trip)
        {
            if (!ModelState.IsValid)
            {
                // Hiển thị lỗi ModelState để debug
                foreach (var error in ModelState)
                {
                    foreach (var subError in error.Value.Errors)
                    {
                        Console.WriteLine($"Lỗi tại {error.Key}: {subError.ErrorMessage}");
                    }
                }

                // Load lại danh sách Route để tránh lỗi View
                ViewData["RouteId"] = new SelectList(
                    _context.Routes.Include(r => r.StartId).Include(r => r.DestinationId)
                    .Select(r => new { Id = r.Id, Name = r.StartId.Name + " → " + r.DestinationId.Name }),
                    "Id", "Name",
                    trip.RouteId
                );

                return View(trip);
            }

            _context.Add(trip);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }



        // GET: Trip/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trip = await _context.Trips
              .Include(t => t.Route)
              .ThenInclude(r => r.StartId)
              .Include(t => t.Route)
              .ThenInclude(r => r.DestinationId)
              .FirstOrDefaultAsync(t => t.Id == id);
            if (trip == null)
            {
                return NotFound();
            }
            ViewData["RouteId"] = new SelectList(
           _context.Routes
           .Include(r => r.StartId)
           .Include(r => r.DestinationId)
           .Select(r => new {
           Id = r.Id,
           Name = r.StartId.Name + " → " + r.DestinationId.Name}),
             "Id", "Name", trip.RouteId);
            return View(trip);
        }

        // POST: Trip/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DepartureTime,ArrivalTime,Status,TotalPrice,RouteId")] Trip trip)
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

            // Load lại danh sách Route khi có lỗi
            ViewData["RouteId"] = new SelectList(
                _context.Routes
                .Include(r => r.StartId)
                .Include(r => r.DestinationId)
                .Select(r => new {
                    Id = r.Id,
                    Name = r.StartId.Name + " → " + r.DestinationId.Name
                }),
                "Id", "Name", trip.RouteId);

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
