using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Bus_Station_Ticket_Management.Controllers
{
    public class AccountController : Controller
    {
        // Initiate the Google login challenge.
        [HttpGet("signin-google")]
        public IActionResult LoginWithGoogle()
        {
            var redirectUrl = Url.Action("GoogleResponse", "Account");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        // Handle the callback from Google.
        [HttpGet("google-response")]
        public async Task<IActionResult> GoogleResponse()
        {
            // Authenticate using the cookie scheme (configured in Program.cs)
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded || result.Principal == null)
            {
                // Log error details as needed.
                return RedirectToAction("Login", "Account");
            }

            // Process the claims (you can add user creation/sign-in logic here)
            var claims = result.Principal.Claims.Select(c => new { c.Type, c.Value });
            return Json(claims);
        }

        // Log out the user.
        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/");
        }
    }
}
