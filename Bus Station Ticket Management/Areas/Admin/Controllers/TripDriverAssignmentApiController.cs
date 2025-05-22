using Microsoft.AspNetCore.Mvc;
using Bus_Station_Ticket_Management.DataAccess;
using Microsoft.EntityFrameworkCore;
using Bus_Station_Ticket_Management.Models;
using Microsoft.AspNetCore.Authorization;

/// <summary>
/// API Controller for managing trip driver assignments
/// </summary>
[ApiController]
[Area("Admin")]
[Route("Admin/api/[controller]")]
public class TripDriverAssignmentApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TripDriverAssignmentApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Tests the API connection
    /// </summary>
    /// <returns>A success message if the connection is working</returns>
    [HttpGet("test")]
    [AllowAnonymous]
    public IActionResult TestConnection() {
        return Ok("Connection successful");
    }

    /// <summary>
    /// Gets all trip driver assignments
    /// </summary>
    /// <returns>A list of all trip driver assignments</returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetTripDriverAssignments()
    {
        var assignments = await _context.TripDriverAssignments.ToListAsync();
        return Ok(assignments);
    }

    /// <summary>
    /// Gets a specific trip driver assignment by ID
    /// </summary>
    /// <param name="id">The ID of the assignment to retrieve</param>
    /// <returns>The trip driver assignment if found</returns>
    [HttpGet("assignment/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTripDriverAssignment(int id)
    {
        var assignment = await _context.TripDriverAssignments.FindAsync(id);
        return Ok(assignment);
    }

    /// <summary>
    /// Gets a list of available drivers for a specific trip
    /// </summary>
    /// <param name="tripId">The ID of the trip to check driver availability for</param>
    /// <returns>A list of available drivers with their IDs and full names</returns>
    [HttpGet("free-drivers")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFreeDriversByTripId(int tripId)
    {
        var trip = await _context.Trips.FindAsync(tripId);
        if (trip == null) return NotFound("Trip not found");

        var busyDriverIds = await _context.TripDriverAssignments
            .Where(tda => tda.Trip.DepartureTime < trip.ArrivalTime && tda.Trip.ArrivalTime > trip.DepartureTime)
            .Select(tda => tda.DriverId)
            .Distinct()
            .ToListAsync();

        var freeDrivers = await _context.Drivers
            .Include(d => d.Account)
            .Where(d => !busyDriverIds.Contains(d.Id) && d.Account.FullName != null)
            .Select(d => new { d.Id, FullName = d.Account.FullName })
            .ToListAsync();

        return Ok(freeDrivers);
    }

    /// <summary>
    /// Checks for available trips that need driver assignments
    /// </summary>
    /// <returns>Information about available trips that are in 'Standby' status and not yet assigned</returns>
    [HttpGet("check-trips")]
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
            return Ok(new {
                success = false,
                message = "An error occurred while checking available trips." + ex.Message
            });
        }
    }
}
