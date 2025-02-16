using BlazorCMS.API.Services;
using BlazorCMS.Data;
using BlazorCMS.Data.Models;
using BlazorCMS.Data.Repositories;
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
    // ✅ Validate JWT Configuration
    var jwtConfig = configuration.GetSection("Jwt") ?? throw new InvalidOperationException("❌ JWT Configuration is missing!");
    var jwtSecret = jwtConfig["Secret"] ?? throw new InvalidOperationException("❌ JWT Secret is missing!");
    var jwtIssuer = jwtConfig["Issuer"] ?? throw new InvalidOperationException("❌ JWT Issuer is missing!");
    var jwtAudience = jwtConfig["Audience"] ?? throw new InvalidOperationException("❌ JWT Audience is missing!");

    var sqliteConnection = configuration.GetConnectionString("SQLite")
        ?? throw new InvalidOperationException("❌ SQLite connection string is missing!");

    logger.LogInformation("✅ Configuration settings validated.");

    // ✅ Configure Database
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(sqliteConnection));

    

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
    builder.Services.AddScoped<JwtTokenService>();
    builder.Services.AddScoped<LoggingService>();
    builder.Services.AddScoped<AuthService>();
    builder.Services.AddScoped<IBlogRepository, BlogRepository>();
    builder.Services.AddScoped<IRepository<Page>, PageRepository>(); // If PageRepository is missing, implement it
    builder.Services.AddScoped<BlogService>();
    builder.Services.AddScoped<PageService>();
    builder.Services.AddScoped<DatabaseInitializer>();
    builder.Services.AddAuthorization();
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // ✅ Register Swagger Services
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "BlazorCMS API", Version = "v1" });

        // ✅ Add JWT Bearer Authentication
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter 'Bearer [token]' to authenticate."
        });

        // ✅ Ensure JWT Token is attached to requests
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

        // ✅ Ensure `AddAuthHeaderOperationFilter` Exists
        c.OperationFilter<AddAuthHeaderOperationFilter>();
    });


    var app = builder.Build();

    // ✅ Database Initialization
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
        await dbInitializer.InitializeAsync();
    }

    // ✅ Enable Swagger UI Only in Development
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "BlazorCMS API v1");
            c.RoutePrefix = "swagger"; // ✅ Ensures Swagger is available at /swagger
        });
    }

    app.MapGet("/", () => Results.Redirect("/swagger"));
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    logger.LogInformation("🚀 BlazorCMS.API is running!");
    app.Run();
}
catch (Exception ex)
{
    logger.LogCritical(ex, "❌ Fatal error: Application startup failed!");
    throw;
}
