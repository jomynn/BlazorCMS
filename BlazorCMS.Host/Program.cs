using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorCMS.Admin;
using BlazorCMS.Client;
using AdminServices = BlazorCMS.Admin.Services;
using ClientServices = BlazorCMS.Client.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure services for both Admin (Blazor Server) and Client (Blazor WebAssembly)
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Register Blazor Server Admin
builder.Services.AddServerSideBlazor();

// Register Blazor WebAssembly Client
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:5001/api/") });

// Use explicit namespaces to resolve ambiguity
builder.Services.AddScoped<ClientServices.ClientAuthService>();
builder.Services.AddScoped<ClientServices.ClientBlogService>();
builder.Services.AddScoped<ClientServices.ClientPageService>();

builder.Services.AddScoped<AdminServices.AdminAuthService>();
builder.Services.AddScoped<AdminServices.AdminBlogService>();
builder.Services.AddScoped<AdminServices.AdminPageService>();

var app = builder.Build();

// Configure middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Map Blazor Server Admin
app.MapBlazorHub();
app.MapFallbackToPage("/admin", "/Admin");

// Map Blazor WebAssembly Client
app.MapFallbackToFile("index.html");

app.Run();
