using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using BlazorCMS.Data;
using BlazorCMS.Data.Models;
using Microsoft.Extensions.Configuration;

namespace BlazorCMS.Infrastructure
{
    public class DatabaseInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DatabaseInitializer> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IServiceProvider _serviceProvider;

        public DatabaseInitializer(
            ApplicationDbContext context,
            ILogger<DatabaseInitializer> logger,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IServiceProvider serviceProvider)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Ensures the database is migrated and seeds roles/users.
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                _logger.LogInformation("🔹 Applying pending database migrations...");
                await _context.Database.MigrateAsync();
                _logger.LogInformation("✅ Database migrations applied successfully.");

                // Seed roles & admin user
                await SeedRolesAsync();
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
            var roles = new[] { "Admin", "Editor", "User" };

            var existingRoles = _roleManager.Roles.Select(r => r.Name).ToList();
            var newRoles = roles.Except(existingRoles).ToList();

            if (newRoles.Any())
            {
                foreach (var role in newRoles)
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
            string defaultPassword = GetAdminPassword();

            var existingAdmin = await _userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Administrator",
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(adminUser, defaultPassword);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                    _logger.LogInformation("✅ Default Admin user created.");
                }
                else
                {
                    _logger.LogError("❌ Failed to create Admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        /// <summary>
        /// Retrieves admin password from appsettings.json or uses a default.
        /// </summary>
        private string GetAdminPassword()
        {
            var config = _serviceProvider.GetRequiredService<IConfiguration>();
            string password = config["Admin:DefaultPassword"];

            return !string.IsNullOrWhiteSpace(password) ? password : "SecureAdmin@123";
        }
    }
}
