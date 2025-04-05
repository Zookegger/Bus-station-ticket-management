using Bus_Station_Ticket_Management.DataAccess;
using Bus_Station_Ticket_Management.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Bus_Station_Ticket_Management.Controllers
{
    [Authorize]
    public class RatingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RatingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var ratings = _context.Ratings
                .Include(r => r.Trip)  
                .ThenInclude(t => t.Route)
                .ThenInclude(r => r.StartLocation)
                .Include(r => r.User)  
                .ToList();

            return View(ratings);
        }

        public IActionResult Details(int id)
        {
            var rating = _context.Ratings
                .Include(r => r.Trip)
                .ThenInclude(t => t.Route)
                .ThenInclude(r => r.StartLocation)
                .FirstOrDefault(r => r.Id == id);

            if (rating == null)
            {
                return NotFound();
            }

            return View(rating);
        }


        public IActionResult Create(int tripId)
        {
            var trips = _context.Trips
        .Include(t => t.Route)
            .ThenInclude(r => r.StartLocation)
        .Include(t => t.Route)
            .ThenInclude(r => r.DestinationLocation)
        .ToList();

            ViewBag.TripList = new SelectList(trips, "Id", "TripDisplayName");

            var rating = new Rating
            {
                CustomerId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                TripId = tripId,
                Trip = _context.Trips.FirstOrDefault(t => t.Id == tripId) ?? throw new InvalidOperationException("Không tìm thấy chuyến đi"),
                User = _context.Users.FirstOrDefault(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier)) ?? throw new InvalidOperationException("Không tìm thấy người dùng")
            };

            if (rating.Trip == null || rating.User == null)
            {
                return NotFound(); 
            }

            return View(rating);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Rating rating)
        {
            if (ModelState.IsValid)
            {
                rating.CreatedAt = DateTime.Now;
                rating.CustomerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("Không lấy được ID người dùng.");
                _context.Add(rating);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Trips", new { id = rating.TripId });
            }

            return View(rating);
        }
        public IActionResult Edit(int id)
        {
            var rating = _context.Ratings
                .FirstOrDefault(r => r.Id == id);

            if (rating == null)
            {
                return NotFound();
            }

            
            ViewBag.TripList = new SelectList(_context.Trips, "Id", "TripDisplayName", rating.TripId);

            return View(rating);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Rating rating)
        {
            if (id != rating.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rating);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Ratings.Any(r => r.Id == id))
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

            ViewBag.TripList = new SelectList(_context.Trips, "Id", "TripDisplayName", rating.TripId);
            return View(rating);
        }
        public IActionResult Delete(int id)
        {
            var rating = _context.Ratings
                .Include(r => r.Trip)
                .FirstOrDefault(r => r.Id == id);

            if (rating == null)
            {
                return NotFound();
            }

            return View(rating);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var rating = _context.Ratings.Find(id);
            if (rating != null)
            {
                _context.Ratings.Remove(rating);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }


    }
}
