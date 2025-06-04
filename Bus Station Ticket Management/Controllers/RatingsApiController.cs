using Bus_Station_Ticket_Management.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bus_Station_Ticket_Management.ApiControllers
{
    [ApiController]
    [Route("api/ratings")]
    public class RatingsApiController(ApplicationDbContext context) : ControllerBase
    {
        [HttpGet("test-connection")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                await context.Database.CanConnectAsync();
                return Ok(new
                {
                    success = true,
                    message = "Connection successful"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error: " + ex.Message
                });
            }
        }

        [HttpGet("get-ratings")]
        public async Task<IActionResult> GetRatings()
        {
            try
            {
                var ratings = await context.Ratings.ToListAsync();
                return Ok(new
                {
                    success = true,
                    message = "Ratings found",
                    data = ratings
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error: " + ex.Message
                });
            }
        }

        [HttpGet("get-ratings-by-user-id/{userId}")]
        public async Task<IActionResult> GetRatingsByUserId(string userId)
        {
            try
            {
                var ratings = await context.Ratings.Where(r => r.UserId == userId).ToListAsync();
                if (ratings == null || ratings.Count == 0)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "No ratings found for the given user ID"
                    });
                }
                return Ok(new
                {
                    success = true,
                    message = "Ratings found for the given user ID",
                    data = ratings
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error: " + ex.Message
                });
            }
        }

        [HttpGet("has-unrated-trips/{userId}")]
        public async Task<IActionResult> HasUnratedTrips(string userId)
        {
            try
            {
                var user = await context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "User not found"
                    });
                }
                // To get the unrated trips, we need to get all the trips and then filter out the ones that have a rating

                var trips = await context.Trips.ToListAsync();
                var ratings = await context.Ratings.ToListAsync();

                var unratedTrips = trips.Where(t => !ratings.Any(r => r.TripId == t.Id)).ToList();
                if (unratedTrips.Count > 0)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "User has unrated trips",
                        data = unratedTrips
                    });
                }
                return Ok(new
                {
                    success = true,
                    message = "User has no unrated trips",
                    data = unratedTrips
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error: " + ex.Message
                });
            }
        }
    }
}