using Microsoft.AspNetCore.Mvc;
using Bus_Station_Ticket_Management.DataAccess;
using Microsoft.EntityFrameworkCore;
using Bus_Station_Ticket_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Bus_Station_Ticket_Management.Areas.Admin.ApiControllers
{
    /// <summary>
    /// API Controller for managing trip driver assignments
    /// </summary>

    // API Controller for managing trip driver assignments
    // Address: /Admin/api/TripDriverAssignment
    [ApiController]
    [Area("Admin")]
    [Route("admin/api/assignment")]
    public class TripDriverAssignmentApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TripDriverAssignmentApiController> _logger;

        public TripDriverAssignmentApiController(ApplicationDbContext context, ILogger<TripDriverAssignmentApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Tests the API connection
        /// </summary>
        /// <returns>A success message if the connection is working</returns>
        [HttpGet("test-connection")]
        [AllowAnonymous]
        public IActionResult TestConnection()
        {
            return Ok("Connection successful");
        }

        /// <summary>
        /// Gets all trip driver assignments
        /// </summary>
        /// <returns>A list of all trip driver assignments</returns>
        [HttpGet("get-assignments")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTripDriverAssignments()
        {
            try
            {
                var assignments = await _context.TripDriverAssignments.ToListAsync();
                if (assignments.Count == 0)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "No trip driver assignments found"
                    });
                }
                return Ok(new
                {
                    success = true,
                    message = "Trip driver assignments loaded successfully",
                    data = assignments
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    success = false,
                    message = "An error occurred while getting the trip driver assignments." + ex.Message
                });
            }
        }

        /// <summary>
        /// Gets a specific trip driver assignment by ID
        /// </summary>
        /// <param name="id">The ID of the assignment to retrieve</param>
        /// <returns>The trip driver assignment if found</returns>
        [HttpGet("get-assignment")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTripDriverAssignment([FromQuery] int id)
        {
            try
            {
                var assignment = await _context.TripDriverAssignments.FindAsync(id);
                if (assignment == null)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Trip driver assignment not found"
                    });
                }
                return Ok(new
                {
                    success = true,
                    message = "Trip driver assignment loaded successfully",
                    data = assignment
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    success = false,
                    message = "An error occurred while getting the trip driver assignment." + ex.Message
                });
            }
        }

        /// <summary>
        /// Gets a list of available drivers for a specific trip
        /// </summary>
        /// <param name="tripId">The ID of the trip to check driver availability for</param>
        /// <returns>A list of available drivers with their IDs and full names</returns>
        [HttpGet("get-free-drivers")]
        public async Task<IActionResult> GetFreeDrivers([FromQuery] int tripId)
        {
            try
            {
                var trip = await _context.Trips
                    .Include(t => t.Route)
                    .FirstOrDefaultAsync(t => t.Id == tripId);

                if (trip == null)
                {
                    return NotFound("Trip not found");
                }

                var assignedDrivers = await _context.TripDriverAssignments
                    .Where(tda => tda.Trip.DepartureTime < trip.ArrivalTime && 
                                tda.Trip.ArrivalTime > trip.DepartureTime)
                    .Select(tda => tda.DriverId)
                    .Distinct()
                    .ToListAsync();

                var freeDrivers = await _context.Drivers
                    .Include(d => d.Account)
                    .Include(d => d.DriverLicenses)
                    .Where(d => !assignedDrivers.Contains(d.Id))
                    .Select(d => new
                    {
                        d.Id,
                        FullName = d.Account.FullName,
                        PhoneNumber = d.Account.PhoneNumber,
                        Licenses = d.DriverLicenses.Select(l => new
                        {
                            l.LicenseId,
                            l.LicenseClass,
                            l.LicenseExpirationDate
                        })
                    })
                    .ToListAsync();

                return Ok(freeDrivers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting free drivers for trip {TripId}", tripId);
                return StatusCode(500, "An error occurred while getting free drivers");
            }
        }

        /// <summary>
        /// Checks for available trips that need driver assignments
        /// </summary>
        /// <returns>Information about available trips that are in 'Standby' status and not yet assigned</returns>
        [HttpGet("check-available-trips")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckTrips()
        {
            try
            {
                // Get all trips that are not assigned and are in "Standby" status
                var unassignedTrips = await _context.Trips
                    .Where(t => t.Status == "Standby" &&
                           !_context.TripDriverAssignments.Any(a => a.TripId == t.Id))
                    .Include(t => t.Route)
                        .ThenInclude(r => r.StartLocation)
                    .Include(t => t.Route)
                        .ThenInclude(r => r.DestinationLocation)
                    .Include(t => t.Vehicle)
                    .ToListAsync();

                if (!unassignedTrips.Any())
                {
                    return Ok(new
                    {
                        success = false,
                        message = "No available trips to assign. All trips are either assigned or not in 'Standby' status."
                    });
                }

                // Return success with trip count
                return Ok(new
                {
                    success = true,
                    message = $"Found {unassignedTrips.Count} available trip(s) to assign.",
                    tripCount = unassignedTrips.Count
                });
            }
            catch (Exception ex)
            {
                // Log the exception here if you have logging configured
                return Ok(new
                {
                    success = false,
                    message = "An error occurred while checking available trips." + ex.Message
                });
            }
        }
    }
}