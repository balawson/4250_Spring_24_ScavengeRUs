using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ScavengeRUs.Models.Entities;
using ScavengeRUs.Data;

namespace ScavengeRUs.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ForgotPasswordModel> _logger;
        private readonly ApplicationDbContext _db;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, ILogger<ForgotPasswordModel> logger, ApplicationDbContext db)
        {
            _userManager = userManager;
            _logger = logger;
            _db = db;
        }

        [BindProperty]
        public ForgotPasswordInputModel Input { get; set; }

        public class ForgotPasswordInputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // Check if the user is an admin
                var isAdmin = await IsAdminAsync(user.UserName);
                if (!isAdmin)
                {
                    // Only admins can reset passwords
                    ModelState.AddModelError(string.Empty, "Only administrators can reset passwords.");
                    return Page();
                }

                // Generate a random password
                string newPassword = GenerateRandomPassword();

                // Set the new password for the user
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

                if (result.Succeeded)
                {
                    // Send the new password to the user's email
                    await SendPassEmail(user.Email, "Password Reset", "Your password has been reset.", newPassword);

                    return RedirectToPage("./ForgotPasswordConfirmation");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }
            }

            return Page();
        }


        // Method to check if the user is an admin
        private async Task<bool> IsAdminAsync(string userName)
        {
            var user = await _db.Users.FindAsync(userName.ToLower());
            if (user != null)
            {
                // Your logic to determine if the user is an admin
                // For example, check if the user has a specific role
                return await _userManager.IsInRoleAsync(user, "Admin");
            }
            return false;
        }


        private string GenerateRandomPassword()
        {
            // Generate a random password with at least 8 characters
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var newPassword = new char[8];
            for (int i = 0; i < newPassword.Length; i++)
            {
                newPassword[i] = chars[random.Next(chars.Length)];
            }
            return new string(newPassword);
        }



        public static async Task SendPassEmail(string recipientEmail, string subject, string body, string newPassword)
        {
            // Configure the SMTP client
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("etsubuchunt98@gmail.com", "your_password"),
                EnableSsl = true,
            };

            // Create the email message
            var message = new MailMessage("etsubuchunt98@gmail.com", recipientEmail, subject, body)
            {
                IsBodyHtml = true
            };

            // Add the new password to the email body
            message.Body += "<br><br>Your new password: " + newPassword;

            try
            {
                // Send the email
                await smtpClient.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                Console.WriteLine("Error sending email: " + ex.Message);
            }
        }
    }
}