// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Bus_Station_Ticket_Management.Models;
using Bus_Station_Ticket_Management.Services.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using MimeKit;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

namespace Bus_Station_Ticket_Management.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResendEmailConfirmationModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailBackgroundQueue _emailQueue;

        public ResendEmailConfirmationModel(UserManager<ApplicationUser> userManager, IEmailBackgroundQueue emailQueue)
        {
            _userManager = userManager;
            _emailQueue = emailQueue;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
                return Page();
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId = userId, code = code },
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
            message.To.Add(new MailboxAddress("", Input.Email));
            message.Subject = "Confirm your email";
            message.Body = new TextPart("html") { Text = htmlBody };
            await _emailQueue.QueueEmail(message);

            SuccessMessage = "Verification email sent. Please check your email.";
            return Page();
        }
    }
}
