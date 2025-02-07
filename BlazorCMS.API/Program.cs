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

// 🔹 Setup Logging
var loggerFactory = LoggerFactory.Create(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});
var logger = loggerFactory.CreateLogger("Program");

try
{
    var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret is missing!");
    var sqliteConnection = builder.Configuration.GetConnectionString("SQLite") ?? throw new InvalidOperationException("SQLite connection string is missing!");

    logger.LogInformation("✅ Configuration settings validated.");

    // 🔹 Configure Database
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(sqliteConnection));

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
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
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

    // 🔹 Swagger Configuration
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

    // 🔹 Dependency Injection
    builder.Services.AddInfrastructure();

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
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BlazorCMS API v1");
        c.RoutePrefix = "swagger"; // API Docs at /swagger
    });

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // 🔹 Log API Start
    logger.LogInformation("🚀 BlazorCMS.API is running on {Url}", builder.Configuration["ASPNETCORE_URLS"]);

    app.Run();
}
catch (Exception ex)
{
    logger.LogCritical(ex, "❌ Fatal error: Application startup failed!");
    throw;
}
