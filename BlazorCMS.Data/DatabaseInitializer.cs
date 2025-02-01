using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using BlazorCMS.Data.Models;

namespace BlazorCMS.Data
{
    public static class DatabaseInitializer
    {
        /// <summary>
        /// Ensures the database is created and migrations are applied.
        /// </summary>
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("DatabaseInitializer");

            try
            {
                var dbContext = services.GetRequiredService<ApplicationDbContext>();

                // 🔹 Apply Migrations Before Creating Roles
                logger.LogInformation("🔹 Applying pending migrations...");
                await dbContext.Database.MigrateAsync();
                logger.LogInformation("✅ Database and migrations applied successfully.");

                // 🔹 Seed Roles & Admin AFTER Migrations
                await SeedRolesAndAdminAsync(services, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ An error occurred while initializing the database.");
            }
        }

        /// <summary>
        /// Seeds default roles and an admin user if not exists.
        /// </summary>
        private static async Task SeedRolesAndAdminAsync(IServiceProvider services, ILogger logger)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            // 🔹 Ensure 'AspNetRoles' Table Exists Before Calling RoleManager
            var dbContext = services.GetRequiredService<ApplicationDbContext>();
            if (!await dbContext.Database.CanConnectAsync())
            {
                logger.LogError("❌ Database connection failed. Skipping role seeding.");
                return;
            }

            // 🔹 Ensure "Admin" Role Exists
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
                logger.LogInformation("✅ Created 'Admin' role.");
            }

            // 🔹 Ensure "User" Role Exists
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
                logger.LogInformation("✅ Created 'User' role.");
            }

            // 🔹 Check if Admin User Exists
            var adminEmail = "admin@blazorcms.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    FullName = "Admin User"
                };

                var result = await userManager.CreateAsync(newAdmin, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                    logger.LogInformation("✅ Created admin user with email: {Email}", adminEmail);
                }
                else
                {
                    logger.LogError("❌ Failed to create admin user. Errors: {Errors}", result.Errors);
                }
            }
        }
    }
}
