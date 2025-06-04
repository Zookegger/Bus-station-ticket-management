using Bus_Station_Ticket_Management.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bus_Station_Ticket_Management.ApiControllers
{
    [ApiController]
    [Route("api/account")]
    public class AccountApiController(ApplicationDbContext dbContext) : ControllerBase
    {
        [HttpGet("test-connection")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                var connection = await dbContext.Database.CanConnectAsync();
                if (!connection)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Database connection failed"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Connection successful"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet("get-user-by-email")]
        public async Task<IActionResult> GetUserByEmail([FromQuery] string email)
        {
            try
            {
                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "User not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "User found",
                    data = user
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}
