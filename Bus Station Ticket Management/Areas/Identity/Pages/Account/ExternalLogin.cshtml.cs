// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable
using System.Net.Http.Headers;
using Bus_Station_Ticket_Management.Models;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace Bus_Station_Ticket_Management.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly RoleManager<ApplicationUser> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;
        private readonly GooglePeopleApiHelper _peopleApiHelper;

        public ExternalLoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender
            // GooglePeopleApiHelper peopleApiHelper
            )
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
            _emailSender = emailSender;
            // _peopleApiHelper = peopleApiHelper;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ProviderDisplayName { get; set; }

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
        [TempData]
        public string SuccessMessage { get; set; }

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
        }

        public IActionResult OnGet() => RedirectToPage("./Login");

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Check if the user already has an external login
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (result.Succeeded)
            {
                // User logged in successfully
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }

            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            else
            {
                // If the user does not have an account, ask to create an account
                ReturnUrl = returnUrl;
                ProviderDisplayName = info.ProviderDisplayName;

                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                    var existingUser = await _userManager.FindByEmailAsync(email);

                    if (existingUser != null)
                    {
                        var alreadyLinked = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                        if (alreadyLinked == null)
                        {
                            var result_add = await _userManager.AddLoginAsync(existingUser, info);
                            if (!result_add.Succeeded)
                            {
                                ErrorMessage = "Failed to link external login to existing user account.";
                                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                            }
                            _logger.LogInformation("Linked {LoginProvider} to existing user {Email}", info.LoginProvider, email);
                        }
                        else
                        {
                            _logger.LogInformation("{LoginProvider} is already linked to user {Email}", info.LoginProvider, email);
                        }

                        await _signInManager.SignInAsync(existingUser, isPersistent: false);
                        SuccessMessage = "Welcome back!";
                        return LocalRedirect(returnUrl);
                    }
                    else
                    {
                        Input = new InputModel
                        {
                            Email = email
                        };
                    }
                }
                return Page();
            }
        }


        private ApplicationUser CreateUserFromClaims(ExternalLoginInfo info, GoogleAdditionalUserInfo additionalUserInfo)
        {
            var user = CreateUser();
            user.UserName = info.Principal.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown username";
            user.FullName = info.Principal.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown Name";
            // user.Gender = additionalUserInfo.Gender ?? "Other";
            // user.PhoneNumber = additionalUserInfo.PhoneNumber ?? null;
            // user.Address = additionalUserInfo.Address ?? null;
            // user.DateOfBirth = additionalUserInfo.DateOfBirth ?? null;
            user.Gender = "Other";
            user.PhoneNumber = null;
            user.Address = null;
            user.DateOfBirth = null;

            return user;
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Extract the access token to call the People API.
            var accessToken = info.AuthenticationTokens.FirstOrDefault(t => t.Name == "access_token")?.Value;

            GoogleAdditionalUserInfo additionalUserInfo = null;

            if (!string.IsNullOrEmpty(accessToken))
            {
                additionalUserInfo = await _peopleApiHelper.GetGoogleAdditionalUserInfoAsync(accessToken);
            }

            // Check if a user already exists with this external login.
            var find_user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (find_user != null)
            {
                await _signInManager.SignInAsync(find_user, isPersistent: false, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }

            // If the access token is available, call the People API for additional info.
            // if (!string.IsNullOrEmpty(accessToken))
            // {
            //     var additionalInfo = await _peopleApiHelper.GetGoogleAdditionalUserInfoAsync(accessToken);
            // }

            // Create user using basic claims and additional information.
            var user = CreateUserFromClaims(info, additionalUserInfo);

            if (ModelState.IsValid)
            {
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Customer");

                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                        // var userId = await _userManager.GetUserIdAsync(user);
                        // var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        // code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        // var callbackUrl = Url.Page(
                        //     "/Account/ConfirmEmail",
                        //     pageHandler: null,
                        //     values: new { area = "Identity", userId = userId, code = code },
                        //     protocol: Request.Scheme);

                        // await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        //     $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                        // // If account confirmation is required, we need to show the link if we don't have a real email sender
                        // if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        // {
                        //     return RedirectToPage("./RegisterConfirmation", new { Email = Input.Email });
                        // }

                        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the external login page in /Areas/Identity/Pages/Account/ExternalLogin.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
