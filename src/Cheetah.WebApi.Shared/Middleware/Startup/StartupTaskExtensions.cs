using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.WebApi.Shared.Middleware.Startup
{
    public static class StartupTaskExtensions
    {
        private static readonly StartupTaskContext sharedContext = new();

        /// <summary>
        /// Add startup multiple tasks to the IServiceCollection
        /// </summary>
        /// <returns>The IServiceCollection</returns>
        public static IServiceCollection AddStartupTasks(this IServiceCollection services)
        {
            // Don't add StartupTaskContext if we've already added it
            if (services.Any(x => x.ServiceType == typeof(StartupTaskContext)))
            {
                return services;
            }

            return services.AddSingleton(sharedContext);
        }

        /// <summary>
        /// Add startup task to the IServiceCollection
        /// </summary>
        /// <returns>The IServiceCollection</returns>
        public static IServiceCollection AddStartupTask<T>(this IServiceCollection services)
            where T : class, IStartupTask
        {
            sharedContext.RegisterTask();
            return services
                .AddStartupTasks() // in case AddStartupTasks() hasn't been called
                .AddHostedService<T>();
        }
    }
}
