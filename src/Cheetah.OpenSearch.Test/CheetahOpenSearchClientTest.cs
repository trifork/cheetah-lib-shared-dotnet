using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cheetah.Core.Util;
using Cheetah.OpenSearch.Client;
using Cheetah.OpenSearch.Config;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenSearch.Client;
using Xunit;
using Xunit.Abstractions;

namespace Cheetah.OpenSearch.Test
{
    [Trait("Category", "OpenSearch"), Trait("TestType", "IntegrationTests")]
    public class OpenSearchIntegrationTest
    {
        readonly ITestOutputHelper _testOutputHelper;

        public OpenSearchIntegrationTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        /// <summary>
        /// Supplies test cases to the OpenSearch integration test
        /// </summary>
        /// <returns>A list of test names and actions to apply to the default configuration</returns>
        public static IEnumerable<object[]> TestConfigurationActions()
        {
            yield return new object[]
            {
                "Basic Auth",
                new Action<OpenSearchConfig>(config =>
                {
                    config.AuthMode = OpenSearchConfig.OpenSearchAuthMode.BasicAuth;
                    config.UserName = "admin";
                    config.Password = "admin";
                })
            };

            yield return new object[]
            {
                "OAuth2",
                new Action<OpenSearchConfig>(config =>
                {
                    config.AuthMode = OpenSearchConfig.OpenSearchAuthMode.OAuth2;
                    config.ClientId = "clientId";
                    config.ClientSecret = "1234";
                })
            };
        }

        [Theory]
        [MemberData(nameof(TestConfigurationActions))]
        public async Task Should_WriteAndReadToOpenSearch_When_UsingAuthentication(string authType, Action<OpenSearchConfig> openSearchConfigAction)
        {
            // This line and the parameter are really just here to make it easier to see which test is running (or failing)
            _testOutputHelper.WriteLine($"Testing OpenSearch connectivity using {authType}");
            
            /*
             * The structure here is a little complex, but the goal is to
             * allow us to specify test-specific parts of the configuration through the actions (e.g. AuthMode, UserName, etc.)
             * while allowing us to specify environment-specific parts through defaults (running locally)
             * or through environment variables (running in docker)
             */
            
            var config = GetDefaultConfig();
            openSearchConfigAction.Invoke(config);
            var sut = CreateClient(config);
            
            var newIndexName = Guid.NewGuid().ToString();
            var newIndicesResponse = await sut.InternalClient.Indices.CreateAsync(new CreateIndexRequest(newIndexName));
            Assert.True(newIndicesResponse.Acknowledged);

            var indices = await sut.GetIndices();
            Assert.Contains(newIndexName, indices);

            await sut.InternalClient.Indices.DeleteAsync(new DeleteIndexRequest(newIndexName));

            indices = await sut.GetIndices();
            Assert.DoesNotContain(newIndexName, indices);
        }
        
        /// <summary>
        /// Gets the default configuration based on static dictionary of local values, with the option to override using environment variables
        /// </summary>
        /// <returns>A configuration with default values, optionally overriden by environment variables</returns>
        private static OpenSearchConfig GetDefaultConfig()
        {
            var configurationDict = new Dictionary<string, string?>
            {
                { "OPENSEARCH:URL", "http://localhost:9200" },
                { "OPENSEARCH:TOKENENDPOINT", "http://localhost:1752/oauth2/token" }
            };
            
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationDict)
                .AddEnvironmentVariables().Build();

            var defaultConfig = new OpenSearchConfig();
            configuration.GetSection(OpenSearchConfig.Position).Bind(defaultConfig);
            return defaultConfig;
        }

        private static CheetahOpenSearchClient CreateClient(OpenSearchConfig config)
        {
            var options = Options.Create(config);
            var env = new HostingEnvironment { EnvironmentName = Environments.Development };
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var httpClientFactory = new DefaultHttpClientFactory();
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
                builder.AddConsole();
            });

            var logger = loggerFactory.CreateLogger<CheetahOpenSearchClient>();
            return new CheetahOpenSearchClient(memoryCache, httpClientFactory, options, env, logger);
        }
    }
}
