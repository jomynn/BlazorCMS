using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorCMS.Admin.Services;  // Ensures we use Admin Services

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:5001/api/") });

// Explicitly define services
builder.Services.AddScoped<BlazorCMS.Admin.Services.AdminAuthService>();
builder.Services.AddScoped<BlazorCMS.Admin.Services.AdminBlogService>();
builder.Services.AddScoped<BlazorCMS.Admin.Services.AdminPageService>();

await builder.Build().RunAsync();
