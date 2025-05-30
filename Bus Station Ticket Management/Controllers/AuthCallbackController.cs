using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bus_Station_Ticket_Management.Controllers
{
    public class AuthCallbackController : Controller
    {
        private readonly ILogger<AuthCallbackController> _logger;

        public AuthCallbackController(ILogger<AuthCallbackController> logger)
        {
            _logger = logger;
        }

        [HttpGet("signin-facebook")]
        public async Task<IActionResult> FacebookCallback()
        {
            try
            {
                // Get the authentication result
                var result = await HttpContext.AuthenticateAsync(FacebookDefaults.AuthenticationScheme);

                if (!result.Succeeded)
                {
                    _logger.LogWarning("Facebook authentication failed");
                    return RedirectToAction("Login", "Account", new { error = "Facebook authentication failed" });
                }

                // Get the user's Facebook information
                var facebookUser = result.Principal;
                var email = facebookUser.FindFirst(ClaimTypes.Email)?.Value;
                var name = facebookUser.FindFirst(ClaimTypes.Name)?.Value;
                var facebookId = facebookUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // TODO: Handle user registration/login logic here
                // You might want to:
                // 1. Check if user exists in your database
                // 2. Create new user if doesn't exist
                // 3. Update existing user's Facebook information
                // 4. Sign in the user to your application

                // For now, we'll just redirect to home
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Facebook authentication callback");
                return RedirectToAction("Login", "Account", new { error = "An error occurred during authentication" });
            }
        }
    }
} 