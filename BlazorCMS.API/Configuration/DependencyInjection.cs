using Microsoft.Extensions.DependencyInjection;
using BlazorCMS.API.Services;

namespace BlazorCMS.API.Configuration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services)
        {
            services.AddScoped<AuthService>();
            services.AddScoped<BlogService>();
            services.AddScoped<PageService>();
            return services;
        }
    }
}
