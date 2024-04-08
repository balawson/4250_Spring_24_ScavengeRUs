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

    }
}