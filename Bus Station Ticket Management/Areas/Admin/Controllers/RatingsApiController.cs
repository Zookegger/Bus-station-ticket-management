using Bus_Station_Ticket_Management.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bus_Station_Ticket_Management.Areas.Admin.ApiControllers
{
    [ApiController]
    [Area("Admin")]
    [Route("admin/api/ratings")]
    public class RatingsApiController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        [HttpGet("test-connection")]
        public IActionResult TestConnection()
        {
            try
            {
                _context.Database.CanConnect();
                return Ok(new
                {
                    success = true,
                    message = "Connection successful"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Connection failed: " + ex.Message);
            }
        }

        [HttpGet("get-ratings")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRatings()
        {
            var ratings = await _context.Ratings
                .Include(r => r.Trip)
                    .ThenInclude(t => t.Route)
                .ToListAsync();

            return Ok(new
            {
                success = true,
                message = "Ratings found",
                data = ratings
            });
        }

        [HttpGet("get-ratings-by-user-id/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRatingsByUserId(string userId)
        {
            try
            {
                var ratings = await _context.Ratings
                    .Include(r => r.Trip)
                        .ThenInclude(t => t.Route)
                    .Where(r => r.UserId == userId)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Ratings found for the given user ID",
                    data = ratings
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Connection failed: " + ex.Message);
            }
        }

        [HttpGet("get-ratings-by-trip-id/{tripId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRatingsByTripId(int tripId)
        {
            try
            {
                var ratings = await _context.Ratings
                    .Include(r => r.Trip)
                        .ThenInclude(t => t.Route)
                    .Where(r => r.TripId == tripId)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Ratings found for the given trip ID",
                    data = ratings
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Connection failed: " + ex.Message);
            }
        }

        [HttpGet("get-ratings-by-route-id/{routeId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRatingsByRouteId(int routeId)
        {
            try
            {
                var ratings = await _context.Ratings
                    .Include(r => r.Trip)
                        .ThenInclude(t => t.Route)
                    .Where(r => r.Trip.RouteId == routeId)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Ratings found for the given route ID",
                    data = ratings
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Connection failed: " + ex.Message);
            }
        }

        [HttpGet("get-ratings-by-vehicle-id/{vehicleId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRatingsByVehicleId(int vehicleId)
        {
            try
            {
                var ratings = await _context.Ratings
                    .Include(r => r.Trip)
                        .ThenInclude(t => t.Vehicle)
                    .Where(r => r.Trip.VehicleId == vehicleId)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Ratings found for the given vehicle ID",
                    data = ratings
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Connection failed: " + ex.Message);
            }
        }

        [HttpGet("get-ratings-by-driver-id/{driverId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRatingsByDriverId(string driverId)
        {
            try
            {
                var ratings = await _context.Ratings
                    .Include(r => r.Trip)
                        .ThenInclude(t => t.TripDriverAssignments)
                            .ThenInclude(td => td.Driver)
                    .Where(r => r.Trip.TripDriverAssignments.Any(td => td.Driver.Id == driverId))
                    .ToListAsync();

                if (ratings.Count == 0)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "No ratings found for the given driver ID"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Ratings found for the given driver ID",
                    data = ratings
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Connection failed: " + ex.Message);
            }
        }

        [HttpGet("get-unrated-trips/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUnratedTrips(string userId)
        {
            var unratedTrips = _context.Tickets
                .Include(t => t.Trip)
                    .ThenInclude(t => t.Route)
                .Where(t => t.UserId == userId)
                .Where(t => !_context.Ratings.Any(r => r.TripId == t.TripId && r.UserId == userId))
                .ToList();

            return Ok(new
            {
                success = true,
                message = "Unrated trips found",
                data = unratedTrips
            });
        }

        [HttpGet("has-unrated-trips/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> HasUnratedTrips(string userId)
        {
            try
            {
                var hasUnratedTrips = await _context.Tickets
                    .Include(t => t.User)
                    .Where(t => t.User.Id == userId)
                    .Where(t => !_context.Ratings.Any(r => r.TripId == t.TripId && r.UserId == userId))
                    .AnyAsync();

                return Ok(new
                {
                    success = true,
                    message = "Has unrated trips",
                    data = hasUnratedTrips
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Connection failed: " + ex.Message);
            }
        }
    }
}