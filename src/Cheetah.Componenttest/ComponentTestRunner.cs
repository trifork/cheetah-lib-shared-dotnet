using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cheetah.ComponentTest.Extensions;
using Cheetah.Core.Config;
using Cheetah.Core.Infrastructure.Auth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Cheetah.ComponentTest
{
    public class ComponentTestRunner
    {
        private readonly List<Action<IServiceCollection>> _serviceCollectionActions = new();
        private LoggingLevelSwitch levelSwitch = new LoggingLevelSwitch(LogEventLevel.Verbose);

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

        /// <summary>
        /// Sets the logging level switch to use for the component test
        /// </summary>
        /// <example>new LoggingLevelSwitch(LogEventLevel.Verbose);</example>
        /// <param name="levelSwitch"></param>
        /// <returns></returns>
        public ComponentTestRunner WithLoggingLevelSwitch(LoggingLevelSwitch levelSwitch)
        {
            this.levelSwitch = levelSwitch;
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
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(levelSwitch)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            var host = Host.CreateDefaultBuilder(args)
          .ConfigureServices((context, services) =>
          {
              foreach (var action in _serviceCollectionActions)
              {
                  action.Invoke(services);
              }
              services.AddHttpClient();
              services.AddSingleton<Microsoft.Extensions.Logging.ILogger>(provider =>
                provider.GetRequiredService<ILogger<CheetahKafkaTokenService>>());
              services.AddHostedService<ComponentTestWorker>();
              services.AddMemoryCache();
              services.Configure<KafkaConfig>
                (context.Configuration.GetSection(KafkaConfig.Position));
              services.Configure<ComponentTestConfig>
                (context.Configuration.GetSection(ComponentTestConfig.Position));
              services.AddSingleton<CheetahKafkaTokenService>();
          })
          .ConfigureLogging(builder =>
          {
              builder.AddFilter("Microsoft", LogLevel.Warning);
          })
          .Build();

            await host.RunAsync();
        }
    }
}
