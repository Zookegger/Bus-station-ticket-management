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
    [Route("[controller]/[action]")]
    public class RatingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RatingsController> _logger;

        public RatingsController(ApplicationDbContext context, ILogger<RatingsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> MyRatings()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var ratings = await _context.Ratings
                    .Include(r => r.Trip)
                        .ThenInclude(t => t.Route)
                            .ThenInclude(r => r.StartLocation)
                    .Include(r => r.Trip)
                        .ThenInclude(t => t.Route)
                            .ThenInclude(r => r.DestinationLocation)
                    .Include(r => r.User)
                    .Where(r => r.UserId == userId)
                    .ToListAsync();

                return View(ratings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting your ratings");
                return View(new List<Rating>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var rating = await _context.Ratings
                    .Include(r => r.Trip)
                        .ThenInclude(t => t.Route)
                            .ThenInclude(r => r.StartLocation)
                    .Include(r => r.Trip)
                        .ThenInclude(t => t.Route)
                            .ThenInclude(r => r.DestinationLocation)
                    .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

                if (rating == null)
                {
                    return NotFound();
                }

                return View(rating);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting details");
                return NotFound("Error getting data");
            }
        }

        public async Task<IActionResult> Create(int tripId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(userId) || userId == null || userId == "0")
                {
                    return BadRequest("User not found");
                }

                var ratedTripIds = await _context.Ratings
                    .Where(r => r.UserId == userId)
                    .Select(r => r.TripId)
                    .ToListAsync();

                var trips = await _context.Trips
                    .Include(t => t.Route)
                        .ThenInclude(r => r.StartLocation)
                    .Include(t => t.Route)
                        .ThenInclude(r => r.DestinationLocation)
                    .Where(t => !ratedTripIds.Contains(t.Id))
                    .ToListAsync();

                var tripOptions = trips.Select(t => new
                {
                    Id = t.Id,
                    Display = $"{t.Route?.StartLocation?.Name} → {t.Route?.DestinationLocation?.Name} | {t.DepartureTime:dd/MM/yyyy HH:mm}"
                }).ToList();

                ViewBag.TripList = new SelectList(tripOptions, "Id", "Display", tripId);

                var rating = new Rating
                {
                    UserId = userId,
                    TripId = tripId
                };

                return View(rating);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting create");
                return View(new Rating());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TripId,UserId,TripRating,Comment")] Rating rating)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                rating.UserId = userId ?? throw new Exception("Unable to get UserId");

                // Check if already rated
                var alreadyRated = await _context.Ratings
                    .AnyAsync(r => r.UserId == userId && r.TripId == rating.TripId);

                if (alreadyRated)
                {
                    ModelState.AddModelError(string.Empty, "You have already rated this trip.");
                }

                if (ModelState.IsValid && !alreadyRated)
                {
                    rating.CreatedAt = DateTime.Now;
                    _context.Add(rating);
                    await _context.SaveChangesAsync();

                    // Stay on the same page with a clean model
                    ModelState.Clear();
                    rating = new Rating { UserId = userId };
                }

                // Get updated trip list for the form
                var ratedTripIds = await _context.Ratings
                    .Where(r => r.UserId == userId)
                    .Select(r => r.TripId)
                    .ToListAsync();

                var trips = await _context.Trips
                    .Include(t => t.Route)
                        .ThenInclude(r => r.StartLocation)
                    .Include(t => t.Route)
                        .ThenInclude(r => r.DestinationLocation)
                    .Where(t => !ratedTripIds.Contains(t.Id))
                    .ToListAsync();

                var tripOptions = trips.Select(t => new
                {
                    Id = t.Id,
                    Display = $"{t.Route?.StartLocation?.Name} → {t.Route?.DestinationLocation?.Name} | {t.DepartureTime:dd/MM/yyyy HH:mm}"
                }).ToList();

                ViewBag.TripList = new SelectList(tripOptions, "Id", "Display", rating.TripId);
                TempData["Success"] = true;
                TempData["Message"] = "Rating created successfully";
                return View(rating);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating rating");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the rating.");
                return View(rating);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var rating = await _context.Ratings
                    .Include(r => r.Trip)
                        .ThenInclude(t => t.Route)
                            .ThenInclude(r => r.StartLocation)
                    .Include(r => r.Trip)
                        .ThenInclude(t => t.Route)
                            .ThenInclude(r => r.DestinationLocation)
                    .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

                if (rating == null)
                {
                    return NotFound();
                }

                return View(rating);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting edit");
                return NotFound("Error getting data");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TripId,UserId,TripRating,Comment")] Rating rating)
        {
            try
            {
                if (id != rating.Id)
                {
                    return NotFound();
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (rating.UserId != userId)
                {
                    return Forbid();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(rating);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!await _context.Ratings.AnyAsync(r => r.Id == id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                return View(rating);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing rating");
                ModelState.AddModelError(string.Empty, "An error occurred while editing the rating.");
                return View(rating);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var rating = await _context.Ratings
                    .Include(r => r.Trip)
                    .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

                if (rating == null)
                {
                    return NotFound();
                }

                return View(rating);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting data");
                return NotFound("Error getting data");
            }
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var rating = await _context.Ratings
                    .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

                if (rating != null)
                {
                    _context.Ratings.Remove(rating);
                    var result = await _context.SaveChangesAsync();
                    if (result > 0)
                    {
                        TempData["Success"] = true;
                        TempData["Message"] = "Rating deleted successfully";
                    }
                    else
                    {
                        TempData["Error"] = true;
                        TempData["Message"] = "Failed to delete rating";
                    }
                }

                return RedirectToAction(nameof(MyRatings));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting rating");
                return RedirectToAction(nameof(MyRatings));
            }
        }
    }
}
