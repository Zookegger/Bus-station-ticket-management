using Bus_Station_Ticket_Management.DataAccess;
using Bus_Station_Ticket_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace Bus_Station_Ticket_Management.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("Admin/[controller]/[action]")]
    public class LocationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LocationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Location
        public async Task<IActionResult> Index(string? searchString, string? sortBy, int? page)
        {
            int pageSize = 20;
            int pageNumber = page ?? 1;

            ViewBag.SortBy = sortBy;
            ViewBag.SearchString = searchString;
            ViewBag.PageNumber = pageNumber;

            searchString = searchString?.Trim().ToLower();

            var locations = _context.Locations.AsQueryable();
            if (!string.IsNullOrEmpty(searchString)) {
                locations = locations.Where(l => 
                    EF.Functions.Collate(l.Name, "Latin1_General_CI_AI").Contains(searchString) ||
                    EF.Functions.Collate(l.Address, "Latin1_General_CI_AI").Contains(searchString)
                );
            }

            locations = sortBy switch {
                "name_asc" => locations.OrderBy(l => l.Name),
                "name_desc" => locations.OrderByDescending(l => l.Name),
                "address_asc" => locations.OrderBy(l => l.Address),
                "address_desc" => locations.OrderByDescending(l => l.Address),
                _ => locations.OrderBy(l => l.Name),
            };

            
            var locationList = await locations.ToListAsync();

            return View(locationList);
            // return View(locationList.ToPagedList(pageNumber, pageSize));
        }

        // GET: Location/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) {
                return NotFound();
            }

            var location = await _context.Locations.FirstOrDefaultAsync(m => m.Id == id);
            if (location == null) {
                return NotFound();
            }

            return View(location);
        }

        public async Task<IActionResult> DetailsPartial(int? id) {
            if (id == null) {
                return NotFound();
            }

            var location = await _context.Locations.FirstOrDefaultAsync(m => m.Id == id);
            if (location == null) {
                return NotFound();
            }

            return PartialView("_DetailsPartial", location);
        }

        // GET: Location/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Location/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Address")] Location location)
        {
            if (LocationExists(location)) {
                ModelState.AddModelError("", "Location already exist! Please check the Name or the Address again");
                return View(location);
            }

            if (ModelState.IsValid) {
                _context.Add(location);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(location);
        }

        // GET: Location/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) {
                return NotFound();
            }

            var location = await _context.Locations.FindAsync(id);
            if (location == null) {
                return NotFound();
            }
            return View(location);
        }

        // POST: Location/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address")] Location location)
        {
            if (id != location.Id) {
                return NotFound();
            }

            if (LocationExists(id, location)) {
                ModelState.AddModelError("", "Location already exist! Please check the Name or the Address again");
                return View(location);
            }

            if (ModelState.IsValid) {
                try {
                    _context.Update(location);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) {
                    if (!LocationExists(location.Id)) {
                        return NotFound();
                    }
                    else {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(location);
        }

        // GET: Location/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) {
                return NotFound();
            }

            var location = await _context.Locations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (location == null) {
                return NotFound();
            }

            return View(location);
        }

        // POST: Location/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location != null) {
                _context.Locations.Remove(location);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LocationExists(int id)
        {
            return _context.Locations.Any(e => e.Id == id);
        }

        private bool LocationExists(Location location)
        {
            return _context.Locations.Any(e => 
                e.Id == location.Id ||
                e.Name == location.Name ||
                e.Address == location.Address
            );
        }
        
        private bool LocationExists(int id, Location location)
        {
            return _context.Locations.Any(e => 
                e.Id != id && 
                (e.Name == location.Name || e.Address == location.Address)
            );
        }
    }
}
