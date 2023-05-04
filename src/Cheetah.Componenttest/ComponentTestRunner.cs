using Cheetah.ComponentTest.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Cheetah.ComponentTest
{
    public class ComponentTestRunner
    {
        private readonly List<Action<IServiceCollection>> _serviceCollectionActions = new();

        /// <summary>
        /// Add a component test to the collection of test to run
        /// </summary>
        /// <typeparam name="T">Of type ComponentTest</typeparam>
        /// <returns></returns>
        public ComponentTestRunner AddTest<T>()
                where T : ComponentTest
        {
            _serviceCollectionActions.Add(services => services.AddSingleton<IComponentTest, T>());
            return this;
        }

        /// <summary>
        /// Adds all classes implementing the IComponentTest interface
        /// </summary>
        /// <returns></returns>
        public ComponentTestRunner AddAllTests()
        {
            AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(
                    x =>
                        typeof(ComponentTest).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract
                )
                .ToList()
                .ForEach(
                    instance =>
                        _serviceCollectionActions.Add(
                            services => services.AddSingleton(typeof(IComponentTest), instance)
                        )
                );
            return this;
        }

        public ComponentTestRunner WithConfiguration<TConfiguration>(string configurationPath)
            where TConfiguration : class
        {
            _serviceCollectionActions.Add(
                services => services.AddOptionsValidateOnStart<TConfiguration>(configurationPath)
            );
            return this;
        }

        public async Task RunAsync(string[] args)
        {
            Log.Logger = new LoggerConfiguration().MinimumLevel
                .Verbose()
                .Enrich.FromLogContext()
                .WriteTo.Console(LogEventLevel.Information)
                .CreateLogger();

            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(
                    (context, services) =>
                    {
                        foreach (var action in _serviceCollectionActions)
                        {
                            action.Invoke(services);
                            services.AddHostedService<ComponentTestWorker>();
                        }
                    }
                )
                .ConfigureLogging(builder =>
                {
                    builder.AddFilter("Microsoft", LogLevel.Warning);
                })
                .Build();

            await host.RunAsync();
        }
    }
}
