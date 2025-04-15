using Bus_Station_Ticket_Management.DataAccess;
using Bus_Station_Ticket_Management.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Bus_Station_Ticket_Management.Areas.Admin.Controllers
{
    [Authorize]
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
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
                .Include(r => r.Trip)  
                    .ThenInclude(t => t.Route)
                        .ThenInclude(r => r.DestinationLocation)
                .Include(r => r.User)  
                .ToList();

            return View(ratings);
        }

        public IActionResult MyRatings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var ratings = _context.Ratings
                .Include(r => r.Trip)  
                    .ThenInclude(t => t.Route)
                        .ThenInclude(r => r.StartLocation)
                .Include(r => r.Trip)  
                    .ThenInclude(t => t.Route)
                        .ThenInclude(r => r.DestinationLocation)
                .Where(r => r.UserId == userId) 
                .ToList();

            var unratedTrips = _context.Tickets
            .Include(t => t.Trip)
            .Where(t => t.UserId == userId)
            .Where(t => !_context.Ratings.Any(r => r.TripId == t.TripId && r.UserId == userId))
            .ToList();

            ViewBag.HasUnratedTrips = unratedTrips.Any();

            return View(ratings);
        }

        public IActionResult Details(int id)
        {
            var rating = _context.Ratings
                .Include(r => r.Trip)
                    .ThenInclude(t => t.Route)
                        .ThenInclude(r => r.StartLocation)
                .Include(r => r.Trip)
                    .ThenInclude(t => t.Route)
                        .ThenInclude(r => r.DestinationLocation)
                .FirstOrDefault(r => r.Id == id);

            if (rating == null)
            {
                return NotFound();
            }

            return View(rating);
        }


        public IActionResult Create(int tripId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get list of TripIds that the user has already rated
            var ratedTripIds = _context.Ratings
                .Where(r => r.UserId == userId)
                .Select(r => r.TripId)
                .ToList();

            // Get trips that the user hasn't rated yet
            var trips = _context.Trips
                .Include(t => t.Route)
                    .ThenInclude(r => r.StartLocation)
                .Include(t => t.Route)
                    .ThenInclude(r => r.DestinationLocation)
                .Where(t => !ratedTripIds.Contains(t.Id))
                .ToList();

            var tripOptions = trips.Select(t => new {
                Id = t.Id,
                Display = $"{t.Route.StartLocation.Name} → {t.Route.DestinationLocation.Name}"
            }).ToList();

            ViewBag.TripList = new SelectList(tripOptions, "Id", "Display", tripId);

            var rating = new Rating
            {
                UserId = userId,
                TripId = tripId
            };

            return View(rating);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TripId,UserId,TripRating,Comment")] Rating rating)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            rating.UserId = userId ?? throw new Exception("Unable to get UserId");

            var alreadyRated = _context.Ratings.Any(r => r.UserId == userId && r.TripId == rating.TripId);
    
            if (alreadyRated) {
                ModelState.AddModelError(string.Empty, "Bạn đã đánh giá chuyến đi này rồi.");
            }

            if (ModelState.IsValid)
            {
                rating.CreatedAt = DateTime.Now;
                _context.Add(rating);
                await _context.SaveChangesAsync();
                
                return RedirectToAction("Details", "Trips", new { id = rating.TripId });
            }
            var ratedTripIds = _context.Ratings
                .Where(r => r.UserId == userId)
                .Select(r => r.TripId)
                .ToList();

            var trips = _context.Trips
                .Include(t => t.Route)
                    .ThenInclude(r => r.StartLocation)
                .Include(t => t.Route)
                    .ThenInclude(r => r.DestinationLocation)
                .Where(t => !ratedTripIds.Contains(t.Id))
                .ToList();

            var tripOptions = trips.Select(t => new {
                Id = t.Id,
                Display = $"{t.Route.StartLocation.Name} → {t.Route.DestinationLocation.Name}"
            }).ToList();

            ViewBag.TripList = new SelectList(tripOptions, "Id", "Display", rating.TripId);

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

            var trips = _context.Trips
                .Include(t => t.Route)
                    .ThenInclude(r => r.StartLocation)
                .Include(t => t.Route)
                    .ThenInclude(r => r.DestinationLocation)
                .ToList();

            var tripOptions = trips.Select(t => new {
                Id = t.Id,
                Display = $"{t.Route.StartLocation.Name} → {t.Route.DestinationLocation.Name}"
            }).ToList();

            ViewBag.TripList = new SelectList(tripOptions, "Id", "Display", rating.TripId);

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
