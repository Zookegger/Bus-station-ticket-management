// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Bus_Station_Ticket_Management.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Bus_Station_Ticket_Management.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginModel> _logger;


        public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        public bool ShowResendConfirmation { get; set; } = false;

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            try {
                if (!string.IsNullOrEmpty(ErrorMessage))
                {
                    ModelState.AddModelError(string.Empty, ErrorMessage);
                }

                returnUrl ??= Url.Content("~/");

                // Clear the existing external cookie to ensure a clean login process
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

                ReturnUrl = returnUrl;

                Input = new InputModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while logging in.");
                ModelState.AddModelError(string.Empty, "An error occurred while logging in. Please try again later.");
            }
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            try {
                returnUrl ??= Url.Content("~/");

                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

                if (ModelState.IsValid)
                {
                    // Get the user by email first
                    var user = await _userManager.FindByEmailAsync(Input.Email);

                    if (user == null)
                    {
                        ModelState.AddModelError(string.Empty, "Invalid Email or Password, Please try again!");
                        return Page();
                    }

                    // If user exists but hasn't confirmed email
                    if (user != null && !await _userManager.IsEmailConfirmedAsync(user))
                    {
                        ShowResendConfirmation = true;
                        ModelState.AddModelError(string.Empty, "You must confirm your email to log in.");
                        return Page();
                    }

                    // Attempt to sign in (only if user is confirmed)
                    var result = await _signInManager.PasswordSignInAsync(
                        userName: user.UserName,
                        password: Input.Password,
                        isPersistent: Input.RememberMe, lockoutOnFailure: false);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User logged in.");

                        // Get the user's roles
                        var roles = await _userManager.GetRolesAsync(user);
                        bool isAdmin = roles.Contains("Admin");

                        // Optional: sign out default cookie first (if using multiple schemes)
                        await _signInManager.SignOutAsync();

                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = Input.RememberMe,
                            ExpiresUtc = isAdmin
                                ? DateTimeOffset.UtcNow.AddHours(1)
                                : DateTimeOffset.UtcNow.AddYears(1)
                        };

                        await _signInManager.SignInAsync(user, authProperties);

                        // Redirect based on role
                        if (roles.Contains("Admin"))
                        {
                            return RedirectToAction("Index", "Home", new { area = "Admin" });
                        }
                        else if (roles.Contains("Employee"))
                        {
                            return RedirectToAction("Index", "Home", new { area = "Employee" });
                        }
                        else if (roles.Contains("Customer"))
                        {
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            return LocalRedirect(returnUrl);
                        }
                    }

                    if (result.RequiresTwoFactor)
                    {
                        return RedirectToPage("./LoginWith2fa", new
                        {
                            ReturnUrl = returnUrl,
                            RememberMe = Input.RememberMe
                        });
                    }

                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning("User account locked out.");
                        return RedirectToPage("./Lockout");
                    }

                    // Fallback error
                    ModelState.AddModelError(string.Empty, "Invalid Email or Password, Please try again!");
                    return Page();
                }

                // Validation failed — redisplay form
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while logging in.");
                ModelState.AddModelError(string.Empty, "An error occurred while logging in. Please try again later.");
                return Page();
            }
        }
    }
}
