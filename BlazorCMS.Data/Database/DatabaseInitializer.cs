using BlazorCMS.Data.Models;
using BlazorCMS.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("🔹 Applying pending database migrations...");
            await _context.Database.MigrateAsync();
            _logger.LogInformation("✅ Database migrations applied successfully.");

            RunSqlScript();
            await SeedRolesAsync();
            await SeedAdminUserAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error initializing the database.");
            throw;
        }
    }

    private async Task SeedRolesAsync()
    {
        var roles = new[] { "Admin", "Editor", "User" };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
                _logger.LogInformation($"✅ Created role: {role}");
            }
        }
    }

    private async Task SeedAdminUserAsync()
    {
        string adminEmail = "admin@blazorcms.com";
        string defaultPassword = GetAdminPassword(); // ✅ Fix Applied

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
        else
        {
            // Ensure password is updated if it's insecure
            if (!await _userManager.CheckPasswordAsync(existingAdmin, defaultPassword))
            {
                await _userManager.RemovePasswordAsync(existingAdmin);
                await _userManager.AddPasswordAsync(existingAdmin, defaultPassword);
                _logger.LogInformation("🔄 Admin password was updated for security.");
            }
        }
    }


    /// <summary>
    /// Retrieves admin password from appsettings.json or uses a default secure password.
    /// </summary>
    private string GetAdminPassword()
    {
        var config = _serviceProvider.GetRequiredService<IConfiguration>();
        string password = config["Admin:DefaultPassword"];

        return !string.IsNullOrWhiteSpace(password) ? password : "SecureAdmin@123";
    }


    private void RunSqlScript()
    {
        string sqlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts", "InitTables.sql");

        if (!File.Exists(sqlFilePath))
        {
            _logger.LogWarning("⚠️ No SQL script found.");
            return;
        }

        _logger.LogInformation($"📄 Running SQL script: {sqlFilePath}");

        string sql = File.ReadAllText(sqlFilePath);
        using var connection = _context.Database.GetDbConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
        connection.Close();

        _logger.LogInformation("✅ SQL script executed successfully.");
    }
}
