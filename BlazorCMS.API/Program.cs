using BlazorCMS.API.Services;
using BlazorCMS.Data;
using BlazorCMS.Data.Models;
using BlazorCMS.Infrastructure;
using BlazorCMS.Infrastructure.Authentication;
using BlazorCMS.Infrastructure.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var environment = builder.Environment;

// ✅ Setup Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();

try
{
    // ✅ Validate Configuration
    var jwtSecret = configuration["Jwt:Secret"] ?? throw new InvalidOperationException("❌ JWT Secret is missing!");
    var jwtIssuer = configuration["Jwt:Issuer"] ?? "https://localhost";
    var jwtAudience = configuration["Jwt:Audience"] ?? "https://localhost";

    var sqliteConnection = configuration.GetConnectionString("SQLite")
        ?? throw new InvalidOperationException("❌ SQLite connection string is missing!");

    logger.LogInformation("✅ Configuration settings validated.");

    // ✅ Configure Database
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(sqliteConnection));

    builder.Services.AddScoped<DatabaseInitializer>();

    // ✅ Configure Identity
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    builder.Services.AddScoped<UserManager<ApplicationUser>>();
    builder.Services.AddScoped<SignInManager<ApplicationUser>>();
    builder.Services.AddScoped<RoleManager<IdentityRole>>();

    // ✅ Configure JWT Authentication
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
            };
        });

    // ✅ Register Services & Repositories
    builder.Services.AddScoped<JwtTokenService>(); // 🔹 Fix missing service
    // ✅ Register Application Services
    builder.Services.AddSingleton<LoggingService>(); // 🔹 Fix Missing Service Error
    builder.Services.AddScoped<AuthService>();
    builder.Services.AddScoped<BlogService>();
    builder.Services.AddScoped<PageService>();
    builder.Services.AddScoped<BlazorCMS.Data.Repositories.BlogRepository>();
    builder.Services.AddScoped<BlazorCMS.Data.Repositories.PageRepository>();

    builder.Services.AddAuthorization();
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // ✅ Configure Swagger (Dev Only)
    if (environment.IsDevelopment())
    {
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "BlazorCMS API", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter 'Bearer [token]' to authenticate."
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
        });
    }

    var app = builder.Build();

    // ✅ Database Initialization
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var dbContext = services.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.Migrate();

            var dbInitializer = services.GetRequiredService<DatabaseInitializer>();
            await dbInitializer.InitializeAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error during database initialization.");
        }
    }

    // ✅ Root Endpoint Redirects to Swagger
    app.MapGet("/", () => Results.Redirect("/swagger"));

    // ✅ Middleware Configuration
    if (environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "BlazorCMS API v1");
            c.RoutePrefix = "swagger";
        });
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    logger.LogInformation("🚀 BlazorCMS.API is running at: {Urls}", configuration["ASPNETCORE_URLS"]);

    app.Run();
}
catch (Exception ex)
{
    logger.LogCritical(ex, "❌ Fatal error: Application startup failed!");
    throw;
}
