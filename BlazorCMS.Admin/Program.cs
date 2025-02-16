using BlazorCMS.Admin.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebApplication.CreateBuilder(args);



//// 🔹 Add Logging (Console & Debug)
//builder.Logging.ClearProviders();
//builder.Logging.AddConsole();
//builder.Logging.AddDebug();

//var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
//logger.LogInformation("🚀 BlazorCMS.Admin starting...");

// 🔹 Configure Blazor Server and Razor Pages
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddHttpClient();
builder.Services.AddLogging();
// 🔹 Add Logging Service for UI
builder.Services.AddSingleton<UILoggerService>();
// 🔹 Configure Database
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlite(builder.Configuration.GetConnectionString("SQLite")));

//builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
//    .AddEntityFrameworkStores<ApplicationDbContext>()
//    .AddDefaultTokenProviders();
builder.Services.AddScoped<CustomAuthStateProvider>(); // ✅ Register CustomAuthStateProvider
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<CustomAuthStateProvider>());

builder.Services.AddScoped<SignalRService>(); // ✅ Ensure SignalRService is registered

builder.Services.AddAuthorizationCore();
//builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<AdminAuthService>();
builder.Services.AddScoped<AdminBlogService>();

// 🔹 Add HTTP Client
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7250/api") });


var app = builder.Build();

// 🔹 Middleware Configuration
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

//logger.LogInformation("✅ BlazorCMS.Admin is running!");

app.Run();
