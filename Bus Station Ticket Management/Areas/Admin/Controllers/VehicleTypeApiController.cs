using Bus_Station_Ticket_Management.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bus_Station_Ticket_Management.Areas.Admin.ApiControllers
{
    public class VehicleTypeApiController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        [HttpGet("test-connection")]
        [AllowAnonymous]
        public IActionResult TestConnection()
        {
            return Ok("Connection successful");
        }

        [HttpGet("list-vehicle-types")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVehicleTypes()
        {
            try
            {
                var vehicleTypes = await _context.VehicleTypes.ToListAsync();
                if (vehicleTypes.Count == 0 || vehicleTypes == null)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "No vehicle types found"
                    });
                }
                return Ok(new
                {
                    success = true,
                    message = "Vehicle types found",
                    data = vehicleTypes
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    success = false,
                    message = "An error occurred while getting the vehicle types." + ex.Message
                });
            }
        }

        [HttpGet("get-vehicle-type-by-id/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVehicleTypeById(int id)
        {
            try
            {
                var vehicleType = await _context.VehicleTypes.FindAsync(id);
                if (vehicleType == null)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Vehicle type not found"
                    });
                }
                return Ok(new
                {
                    success = true,
                    message = "Vehicle type found",
                    data = vehicleType
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    success = false,
                    message = "An error occurred while getting the vehicle type." + ex.Message
                });
            }
        }
    }
}