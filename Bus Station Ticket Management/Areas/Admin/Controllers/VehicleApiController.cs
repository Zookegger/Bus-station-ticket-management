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
    [Route("admin/api/vehicle")]
    public class VehicleApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VehicleApiController(ApplicationDbContext context)
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
        public async Task<IActionResult> GetVehicles()
        {
            try
            {
                var vehicles = await _context.Vehicles
                    .Include(v => v.VehicleType)
                    .ToListAsync();

                if (vehicles.Count == 0)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "No vehicles found"
                    });
                }
                return Ok(new {
                    success = true,
                    message = "Vehicles loaded successfully",
                    data = vehicles.OrderBy(v => v.Id)
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    success = false,
                    message = "An error occurred while getting the vehicles." + ex.Message
                });
            }
        }

        [HttpGet("get-vehicle-by-id/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVehicle(int id)
        {
            try
            {
                var vehicle = await _context.Vehicles
                    .Include(v => v.VehicleType)
                    .FirstOrDefaultAsync(v => v.Id == id);
                
                if (vehicle == null)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Vehicle not found"
                    });
                }
                return Ok(new
                {
                    success = true,
                    message = "Vehicle loaded successfully",
                    data = vehicle
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    success = false,
                    message = "An error occurred while getting the vehicle." + ex.Message
                });
            }
        }

        [HttpGet("get-vehicle-by-license-plate/{licensePlate}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVehicleByLicensePlate(string licensePlate)
        {
            try
            {
                var vehicle = await _context.Vehicles
                    .Include(v => v.VehicleType)
                    .FirstOrDefaultAsync(v => v.LicensePlate == licensePlate);
                
                if (vehicle == null)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Vehicle not found"
                    });
                }
                return Ok(new
                {
                    success = true,
                    message = "Vehicle loaded successfully",
                    data = vehicle
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    success = false,
                    message = "An error occurred while getting the vehicle." + ex.Message
                });
            }
        }
    }
}