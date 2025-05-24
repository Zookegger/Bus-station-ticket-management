// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Bus_Station_Ticket_Management.Models;
using Bus_Station_Ticket_Management.Services;
using Bus_Station_Ticket_Management.Services.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using MimeKit;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

namespace Bus_Station_Ticket_Management.Areas.Identity.Pages.Account.Manage
{
    public class EmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailBackgroundQueue _emailQueue;
        private readonly ILogger<EmailModel> _logger;

        public EmailModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailBackgroundQueue emailQueue,
            ILogger<EmailModel> logger    
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailQueue = emailQueue;
            _logger = logger;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public bool IsEmailConfirmed { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

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
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "New email")]
            public string NewEmail { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var email = await _userManager.GetEmailAsync(user);
            Email = email;

            Input = new InputModel {
                NewEmail = email,
            };

            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostChangeEmailAsync()
        {
            try {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) {
                    return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
                }

                if (!ModelState.IsValid) {
                    await LoadAsync(user);
                    return Page();
                }

                var email = await _userManager.GetEmailAsync(user);
                if (Input.NewEmail != email) {
                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmailChange",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, email = Input.NewEmail, code = code },
                        protocol: Request.Scheme);

                    var encodedUrl = HtmlEncoder.Default.Encode(callbackUrl);
                    var htmlBody = $@"
                            <div style=""margin: 0 auto; padding: 12px;"">
                                <header style=""margin-top: 1rem;"">
                                    <span style=""font-size: 28px; font-weight: 700;"">Confirm your account</span>
                                </header>
                                <main style=""margin-top: 1.5rem;"">
                                    <span style=""font-size: 20px; font-weight: 400;"">
                                        Please click the button below to confirm your email address and finish setting up your account.
                                    </span>
                                </main>
                                <footer style=""margin-top: 1rem;"">
                                    <a href=""{encodedUrl}"" 
                                    style=""display: inline-block; font-weight: 400; text-align: center; vertical-align: middle; 
                                            cursor: pointer; user-select: none; padding: 0.375rem 0.75rem; font-size: 1rem; 
                                            line-height: 1.5; border-radius: 0.25rem; color: #fff; background-color: #007bff; 
                                            text-decoration: none;"">
                                    Confirm
                                    </a>
                                </footer>
                            </div>";
                        
                    var message = new MimeMessage();
                    message.To.Add(new MailboxAddress("", Input.NewEmail));
                    message.Subject = "Confirm your email";
                    message.Body = new TextPart("html") { Text = htmlBody };

                    await _emailQueue.QueueEmail(message);

                    StatusMessage = "Confirmation link to change email sent. Please check your email.";
                    return RedirectToPage();
                }

                StatusMessage = "Your email is unchanged.";
                return RedirectToPage();
            } catch (Exception ex) {
                _logger.LogError(ex, "An error occurred while changing the email.");
                StatusMessage = "An unexpected error occurred.";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid) {
                await LoadAsync(user);
                return Page();
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = code },
                protocol: Request.Scheme);

            var encodedUrl = HtmlEncoder.Default.Encode(callbackUrl);
            var htmlBody = $@"
                    <div style=""margin: 0 auto; padding: 12px;"">
                        <header style=""margin-top: 1rem;"">
                            <span style=""font-size: 28px; font-weight: 700;"">Confirm your account</span>
                        </header>
                        <main style=""margin-top: 1.5rem;"">
                            <span style=""font-size: 20px; font-weight: 400;"">
                                Please click the button below to confirm your email address and finish setting up your account.
                            </span>
                        </main>
                        <footer style=""margin-top: 1rem;"">
                            <a href=""{encodedUrl}"" 
                            style=""display: inline-block; font-weight: 400; text-align: center; vertical-align: middle; 
                                    cursor: pointer; user-select: none; padding: 0.375rem 0.75rem; font-size: 1rem; 
                                    line-height: 1.5; border-radius: 0.25rem; color: #fff; background-color: #007bff; 
                                    text-decoration: none;"">
                            Confirm
                            </a>
                        </footer>
                    </div>";
                
            var message = new MimeMessage();
            message.To.Add(new MailboxAddress("", Input.NewEmail));
            message.Subject = "Confirm your email";
            message.Body = new TextPart("html") { Text = htmlBody };

            await _emailQueue.QueueEmail(message);
            
            StatusMessage = "Verification email sent. Please check your email.";
            return RedirectToPage();
        }
    }
}
