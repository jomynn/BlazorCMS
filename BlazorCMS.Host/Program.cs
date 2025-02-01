using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Configure Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();

// 🔹 Add Blazor Services
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor(); // Blazor Server for Admin Panel

var app = builder.Build();

// 🔹 Middleware Configuration
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Serve static files (CSS, JS)
app.UseRouting();

// 🔹 Serve Blazor WebAssembly Client manually
app.UseWhen(context => !context.Request.Path.StartsWithSegments("/api"), appBuilder =>
{
    appBuilder.UseStaticFiles();
    appBuilder.UseDefaultFiles();
});

// 🔹 Configure API & Blazor Endpoints
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapRazorPages();
    endpoints.MapBlazorHub(); // Blazor Server SignalR
    endpoints.MapFallbackToFile("index.html"); // Blazor WebAssembly Client
});

// 🔹 Log Application Start
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("🚀 BlazorCMS.Host is running on {Url}", app.Urls);

// 🔹 Run Application
app.Run();
