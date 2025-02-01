using Microsoft.Extensions.DependencyInjection;

namespace BlazorCMS.UIComponents
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddUIComponents(this IServiceCollection services)
        {
            return services;
        }
    }
}
