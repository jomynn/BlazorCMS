using BlazorCMS.API.Services;
using BlazorCMS.Data;
using BlazorCMS.Data.Models;
using BlazorCMS.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var environment = builder.Environment;

// 🔹 Setup Logging (Using Built-in Logging)
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();

try
{
    // 🔹 Validate Configuration
    var jwtConfig = configuration.GetSection("Jwt");
    if (string.IsNullOrWhiteSpace(jwtConfig["Secret"]))
        throw new InvalidOperationException("❌ JWT Secret is missing!");

    var sqliteConnection = configuration.GetConnectionString("SQLite")
        ?? throw new InvalidOperationException("❌ SQLite connection string is missing!");

    logger.LogInformation("✅ Configuration settings validated.");

    // 🔹 Configure Database
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(sqliteConnection));

    // 🔹 Register DatabaseInitializer (Fixes DI Issue)
    builder.Services.AddScoped<DatabaseInitializer>();

    // 🔹 Configure Identity
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    // 🔹 Register Identity Services
    builder.Services.AddScoped<UserManager<ApplicationUser>>();
    builder.Services.AddScoped<SignInManager<ApplicationUser>>();
    builder.Services.AddScoped<RoleManager<IdentityRole>>();

    // 🔹 Configure JWT Authentication
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
                ValidIssuer = jwtConfig["Issuer"],
                ValidAudience = jwtConfig["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["Secret"]))
            };
        });

    // 🔹 Register Application Services
    builder.Services.AddScoped<AuthService>();
    builder.Services.AddScoped<BlogService>();
    builder.Services.AddScoped<PageService>();

    // 🔹 Register Repositories
    builder.Services.AddScoped<BlazorCMS.Data.Repositories.BlogRepository>();
    builder.Services.AddScoped<BlazorCMS.Data.Repositories.PageRepository>();

    // 🔹 Authorization & Controllers
    builder.Services.AddAuthorization();
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // 🔹 Swagger Configuration (Enable only in Development)
    if (environment.IsDevelopment())
    {
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "BlazorCMS API", Version = "v1" });

            // 🔹 Add JWT Authentication to Swagger
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

    // 🔹 Call DatabaseInitializer
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var dbInitializer = services.GetRequiredService<DatabaseInitializer>();
            await dbInitializer.InitializeAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error during database initialization.");
        }
    }

    // 🔹 Add a Root Endpoint (Redirect to Swagger)
    app.MapGet("/", () => Results.Redirect("/swagger"));

    // 🔹 Middleware Configuration
    if (environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "BlazorCMS API v1");
            c.RoutePrefix = "swagger"; // API Docs at /swagger
        });
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // 🔹 Log API Start
    logger.LogInformation("🚀 BlazorCMS.API is running at: {Url}", configuration["ASPNETCORE_URLS"]);

    app.Run();
}
catch (Exception ex)
{
    logger.LogCritical(ex, "❌ Fatal error: Application startup failed!");
    throw;
}
