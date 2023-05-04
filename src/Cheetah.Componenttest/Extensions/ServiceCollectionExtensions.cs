using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.ComponentTest.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOptionsValidateOnStart<T>(
            this IServiceCollection services,
            string configurationPath)
            where T : class
        {
            services.AddOptions<T>().BindConfiguration(configurationPath).ValidateDataAnnotations().ValidateOnStart();
            return services;
        }
    }
}
