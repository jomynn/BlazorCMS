using Microsoft.Extensions.DependencyInjection;
using BlazorCMS.Infrastructure.Authentication;
using BlazorCMS.Infrastructure.Email;
using BlazorCMS.Infrastructure.Logging;
using BlazorCMS.Infrastructure.Storage;

namespace BlazorCMS.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<JwtTokenService>();
            services.AddSingleton<EmailService>();
            services.AddSingleton<LoggingService>();
            services.AddSingleton<FileStorageService>();

            return services;
        }
    }
}
