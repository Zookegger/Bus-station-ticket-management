// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Bus_Station_Ticket_Management.Models;
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

namespace Bus_Station_Ticket_Management.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailBackgroundQueue _emailQueue;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailBackgroundQueue emailQueue)
        {
            _userManager = userManager;
            _emailQueue = emailQueue;
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

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid) {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user))) {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                var message = new MimeMessage();
                message.To.Add(new MailboxAddress("", Input.Email));
                message.Subject = "Reset Password";
                message.Body = new TextPart("html") { 
                    Text = $@"
                    <html>
                    <head>
                        <style>
                            body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }}
                            .container {{ border: 1px solid #ddd; border-radius: 5px; padding: 20px; }}
                            .header {{ background-color: #0d6efd; color: white; padding: 10px; text-align: center; border-radius: 5px 5px 0 0; }}
                            .content {{ padding: 20px; }}
                            .button {{ display: inline-block; background-color: #0d6efd; color: white; text-decoration: none; padding: 10px 20px; border-radius: 5px; margin-top: 15px; }}
                            .footer {{ margin-top: 20px; font-size: 12px; color: #777; text-align: center; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h2>Password Reset Request</h2>
                            </div>
                            <div class='content'>
                                <p>Hello,</p>
                                <p>We received a request to reset your password. If you didn't make this request, you can safely ignore this email.</p>
                                <p>To reset your password, please click the button below:</p>
                                <p style='text-align: center;'>
                                    <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' class='button'>Reset Password</a>
                                </p>
                                <p>If the button doesn't work, copy and paste the following link into your browser:</p>
                                <p style='word-break: break-all;'><a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>{HtmlEncoder.Default.Encode(callbackUrl)}</a></p>
                                <p>This link will expire in 24 hours.</p>
                                <p>Thank you,<br>Bus Station Ticket Management Team</p>
                            </div>
                            <div class='footer'>
                                <p>This is an automated message, please do not reply to this email.</p>
                            </div>
                        </div>
                    </body>
                    </html>"
                };

                await _emailQueue.QueueEmail(message);

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
