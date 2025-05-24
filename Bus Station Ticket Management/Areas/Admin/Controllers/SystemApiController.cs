using Microsoft.AspNetCore.Mvc;
using Bus_Station_Ticket_Management.DataAccess;
using Microsoft.EntityFrameworkCore;
using Bus_Station_Ticket_Management.Models;
using Microsoft.AspNetCore.Authorization;

namespace Bus_Station_Ticket_Management.Areas.Admin.ApiControllers
{
    
    [ApiController]
    [Area("Admin")]
    [Route("admin/api/system")]
    public class SystemApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SystemApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("check-database-connection")]
        public IActionResult CheckDatabaseConnection()
        {
            try
            {
                _context.Database.CanConnect();
                return Ok(new {
                    status = "success",
                    message = "Connection successful" 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    status = "error",
                    message = $"Connection failed: {ex.Message}" 
                });
            }
        }

        [HttpGet("check-api-connection")]
        public IActionResult CheckApiConnection()
        {
            try
            {
                return Ok(new { 
                    status = "success",
                    message = "API connection successful" 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    status = "error",
                    message = $"API connection failed: {ex.Message}" 
                });
            }
        }
    }
}