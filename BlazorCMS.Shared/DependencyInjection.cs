using Microsoft.Extensions.DependencyInjection;
namespace BlazorCMS.Shared
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSharedServices(this IServiceCollection services)
        {
            return services;
        }
    }
}
