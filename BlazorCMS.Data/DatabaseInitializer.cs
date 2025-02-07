using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using BlazorCMS.Data;
using BlazorCMS.Data.Models;

namespace BlazorCMS.Infrastructure
{
    public class DatabaseInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DatabaseInitializer> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DatabaseInitializer(
            ApplicationDbContext context,
            ILogger<DatabaseInitializer> logger,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// Ensures the database is created, applies migrations, and seeds roles/users.
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                _logger.LogInformation("🔹 Checking if the database exists...");
                await _context.Database.EnsureCreatedAsync();

                _logger.LogInformation("🔹 Applying pending migrations...");
                await _context.Database.MigrateAsync();

                _logger.LogInformation("✅ Database is ready.");

                // Seed default roles
                await SeedRolesAsync();

                // Seed admin user
                await SeedAdminUserAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error initializing the database.");
                throw;
            }
        }

        /// <summary>
        /// Ensures predefined roles exist in the system.
        /// </summary>
        private async Task SeedRolesAsync()
        {
            string[] roles = { "Admin", "Editor", "User" };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                    _logger.LogInformation($"✅ Created role: {role}");
                }
            }
        }

        /// <summary>
        /// Seeds a default admin user if no admin exists.
        /// </summary>
        private async Task SeedAdminUserAsync()
        {
            string adminEmail = "admin@blazorcms.com";
            string adminPassword = "Admin@123"; // Change this in production

            if (await _userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Administrator",
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                    _logger.LogInformation("✅ Default Admin user created.");
                }
                else
                {
                    _logger.LogError("❌ Failed to create Admin user: {Errors}", string.Join(", ", result.Errors));
                }
            }
        }
    }
}
