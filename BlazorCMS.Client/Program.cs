using BlazorCMS.Client;
using BlazorCMS.Client.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure API Base URL
string apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:5001/api/";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

// Register API Services
builder.Services.AddScoped<ClientAuthService>();
builder.Services.AddScoped<ClientBlogService>();
builder.Services.AddScoped<ClientPageService>();

// Register MudBlazor UI Framework
builder.Services.AddMudServices();

await builder.Build().RunAsync();
