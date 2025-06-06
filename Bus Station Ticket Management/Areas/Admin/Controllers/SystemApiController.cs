using Microsoft.AspNetCore.Mvc;
using Bus_Station_Ticket_Management.DataAccess;
using Microsoft.EntityFrameworkCore;
using Bus_Station_Ticket_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Bus_Station_Ticket_Management.Services.BackgroundProcess;

namespace Bus_Station_Ticket_Management.Areas.Admin.ApiControllers
{
    /// <summary>
    /// API Controller for managing system
    /// </summary>
    /// Address: /Admin/api/System
    [ApiController]
    [Area("Admin")]
    [Route("admin/api/system")]
    public class SystemApiController(ApplicationDbContext context, ExpiredPaymentCleanupService cleanupService) : ControllerBase
    {
        [HttpGet("check-database-connection", Name = "CheckDatabaseConnection")]
        public IActionResult CheckDatabaseConnection()
        {
            try
            {
                context.Database.CanConnect();
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

        [HttpGet("check-api-connection", Name = "CheckApiConnection")]
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

        [HttpGet("payment-cleanup-status", Name = "GetPaymentCleanupStatus")]
        public IActionResult GetPaymentCleanupStatus()
        {
            return Ok(new
            {
                LastRunTime = cleanupService.LastRunTime,
                IsHealthy = cleanupService.LastRunSuccess,
                LastError = cleanupService.LastRunError,
                TotalProcessed = cleanupService.LastRunProcessedCount,
                IsRunning = cleanupService.IsRunning,
                LastRunDuration = cleanupService.LastRunDuration
            });
        }
    }
}