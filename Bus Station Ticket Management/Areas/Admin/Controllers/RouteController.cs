using System.Data;
using Bus_Station_Ticket_Management.DataAccess;
using Bus_Station_Ticket_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Newtonsoft.Json;
using X.PagedList.EF;
using X.PagedList.Extensions;

namespace Bus_Station_Ticket_Management.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("Admin/[controller]/[action]")]

    public class RouteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RouteController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Route
        public async Task<IActionResult> Index()
        {
            var routes = await _context.Routes.Include(r => r.StartLocation).Include(r => r.DestinationLocation).ToListAsync();
            return View(routes);
        }

        // GET: Route/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var routes = await _context.Routes
                .Include(r => r.DestinationLocation)
                .Include(r => r.StartLocation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (routes == null)
            {
                return NotFound();
            }

            return View(routes);
        }

        public async Task<IActionResult> DetailsPartial(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var routes = await _context.Routes
                .Include(r => r.DestinationLocation)
                .Include(r => r.StartLocation)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (routes == null)
            {
                return NotFound();
            }

            return PartialView("_DetailsPartial", routes);
        }

        // GET: Route/Create
        public IActionResult Create()
        {
            ViewData["DestinationLocation"] = new SelectList(_context.Locations, "Id", "Name");
            ViewData["StartLocation"] = new SelectList(_context.Locations, "Id", "Name");
            return View();
        }

        // POST: Route/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StartId,DestinationId,Price,Distance")] Routes routes)
        {
            try {
                if (routes.StartId == routes.DestinationId)
                {
                    ModelState.AddModelError(string.Empty, "Start location and destination can't be the same");
                }

                if (RouteExists(routes.StartId, routes.DestinationId))
                {
                    ModelState.AddModelError("StartId", "Route already exists");
                }

                if (ModelState.IsValid)
                {
                    _context.Add(routes);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }   
                ViewData["StartLocation"] = new SelectList(_context.Locations, "Id", "Name", routes.StartId);
                ViewData["DestinationLocation"] = new SelectList(_context.Locations, "Id", "Name", routes.DestinationId);

                return View(routes);
            } 
            catch(DBConcurrencyException ex) {
                ModelState.AddModelError($"Error: {ex.Message}", ex.ToString());
            
                ViewData["StartLocation"] = new SelectList(_context.Locations, "Id", "Name", routes.StartId);
                ViewData["DestinationLocation"] = new SelectList(_context.Locations, "Id", "Name", routes.DestinationId);
                return View(routes);
            } 
            catch (Exception ex) {
                ModelState.AddModelError($"Error: {ex.Message}", ex.ToString());

                ViewData["StartLocation"] = new SelectList(_context.Locations, "Id", "Name", routes.StartId);
                ViewData["DestinationLocation"] = new SelectList(_context.Locations, "Id", "Name", routes.DestinationId);
                return View(routes);
            }
        }

        // GET: Route/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var routes = await _context.Routes.FindAsync(id);
            if (routes == null)
            {
                return NotFound();
            }
            ViewData["StartLocation"] = new SelectList(_context.Locations, "Id", "Name", routes.StartId);
            ViewData["DestinationLocation"] = new SelectList(_context.Locations, "Id", "Name", routes.DestinationId);
            return View(routes);
        }

        // POST: Route/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StartId,DestinationId,Price,Distance")] Routes routes)
        {
            
            try
            {
                if (id != routes.Id)
                {
                    return NotFound();
                }

                if (routes.StartId == routes.DestinationId)
                {
                    ModelState.AddModelError("ModelOnly", "Start location and destination can't be the same");
                }

                var base_route = await _context.Routes.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);

                if (base_route == null)
                {
                    return NotFound();
                }

                if (base_route.StartId != routes.StartId || base_route.DestinationId != routes.DestinationId)
                {
                    if (RouteExists(routes.StartId, routes.DestinationId))
                    {
                        ModelState.AddModelError("StartId", "Route already exists");
                    }
                }

                if (ModelState.IsValid)
                {
                    // routes.StartLocation = _context.Locations.FirstOrDefault(r => r.Id == routes.StartId);
                    // routes.DestinationLocation = _context.Locations.FirstOrDefault(r => r.Id == routes.DestinationId);

                    _context.Update(routes);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.StartLocation = new SelectList(_context.Locations, "Id", "Name", routes.StartId);
                ViewBag.DestinationLocation = new SelectList(_context.Locations, "Id", "Name", routes.DestinationId);

                return View(routes);
            }
            catch (DbUpdateConcurrencyException)
            {
                ViewBag.StartLocation = new SelectList(_context.Locations, "Id", "Name", routes.StartId);
                ViewBag.DestinationLocation = new SelectList(_context.Locations, "Id", "Name", routes.DestinationId);
                if (!RouteExists(routes.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            } catch (Exception ex) {
                ViewBag.StartLocation = new SelectList(_context.Locations, "Id", "Name", routes.StartId);
                ViewBag.DestinationLocation = new SelectList(_context.Locations, "Id", "Name", routes.DestinationId);
                ViewBag.ErrorMessage = ex.Message;
                return View(routes);
            }
        }

        // GET: Route/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var routes = await _context.Routes
                .Include(r => r.DestinationLocation)
                .Include(r => r.StartLocation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (routes == null)
            {
                return NotFound();
            }

            return View(routes);
        }

        // POST: Route/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var routes = await _context.Routes.FindAsync(id);
            if (routes != null)
            {
                _context.Routes.Remove(routes);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RouteExists(int id)
        {
            return _context.Routes.Any(e => e.Id == id);
        }

        private bool RouteExists(int startId, int destinationId)
        {
            return _context.Routes.Any(e => e.StartId == startId && e.DestinationId == destinationId);
        }
    }
}
