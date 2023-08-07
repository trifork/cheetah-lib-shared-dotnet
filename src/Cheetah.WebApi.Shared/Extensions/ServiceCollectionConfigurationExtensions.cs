using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.WebApi.Shared.Extensions
{
    public static class ServiceCollectionConfigurationExtensions
    {
        /// <summary>
        /// Configures a configuration object using AddOptions<typeparamref name="T"/> and sets up DataAnnotations validation on startup.
        /// This will cause an exception to be thrown on startup if any required configuration is missing.
        /// </summary>
        /// <param name="services">The service collection to add configuration to</param>
        /// <param name="configurationSection">The section of configuration to read from</param>
        /// <typeparam name="T">The type of configuration to configure</typeparam>
        /// <returns>The service collection this method was called on to allow method-chaining</returns>
        public static IServiceCollection ConfigureAndValidate<T>(this IServiceCollection services, string configurationSection) where T : class
        {
            services.AddOptions<T>()
                .BindConfiguration(configurationSection)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            return services;
        }
    }
}
