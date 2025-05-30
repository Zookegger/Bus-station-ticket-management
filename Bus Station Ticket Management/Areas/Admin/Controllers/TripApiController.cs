using Microsoft.AspNetCore.Mvc;
using Bus_Station_Ticket_Management.DataAccess;
using Microsoft.EntityFrameworkCore;
using Bus_Station_Ticket_Management.Models;
using Microsoft.AspNetCore.Authorization;

namespace Bus_Station_Ticket_Management.Areas.Admin.ApiControllers
{
    /// <summary>
    /// API Controller for managing trip driver assignments
    /// </summary>

    // API Controller for managing vehicles
    // Address: /Admin/api/VehicleApi

    [ApiController]
    [Area("Admin")]
    [Route("admin/api/trip")]
    public class TripApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TripApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("test-connection")]
        [AllowAnonymous]
        public IActionResult TestConnection()
        {
            return Ok("Connection successful");
        }

        [HttpGet("list-vehicles")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTrips() {
            try {
                var trips = await _context.Trips.ToListAsync();
                if (trips.Count == 0 || trips == null)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "No trips found"
                    });
                }
                return Ok(new
                {
                    success = true,
                    message = "Trips found",
                    data = trips
                });
            } catch (Exception ex) {
                return Ok(new
                {
                    success = false,
                    message = "An error occurred while getting the trips." + ex.Message
                });
            }
        }

        [HttpGet("get-trip-by-id/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTripById(int id) {
            try {
                var trip = await _context.Trips.FindAsync(id);
                if (trip == null)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Trip not found"
                    });
                }
                return Ok(new
                {
                    success = true,
                    message = "Trip found",
                    data = trip
                });
            } catch (Exception ex) {
                return Ok(new
                {
                    success = false,
                    message = "An error occurred while getting the trip." + ex.Message
                });
            }
        }
        
        [HttpGet("get-trip-by-vehicle-id/{vehicleId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTripByVehicleId(int vehicleId){
            var trip = await _context.Trips
                .Include(x => x.Vehicle)
                .FirstOrDefaultAsync(v => v.Id == vehicleId);

            if (trip == null)
            {
                return NotFound();
            }

            return Ok(new {
                Name = trip.Vehicle.Name,
                LicensePlate = trip.Vehicle.LicensePlate,
                VehicleType = trip.Vehicle.VehicleType?.Name,
                TotalSeats = trip.Vehicle.VehicleType?.TotalSeats,
                TotalFloors = trip.Vehicle.VehicleType?.TotalFloors,
                TotalColumns = trip.Vehicle.VehicleType?.TotalColumns,
            });
        }
    }
}