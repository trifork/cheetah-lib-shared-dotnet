using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cheetah.Core.Util;
using Cheetah.OpenSearch.Config;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
                new List<KeyValuePair<string, string?>>
                {
                    new ("OPENSEARCH:AUTHMODE", "Basic"),
                    new ("OPENSEARCH:USERNAME", "admin"),
                    new ("OPENSEARCH:PASSWORD", "admin")
                }
            };

            yield return new object[]
            {
                "OAuth2",
                new List<KeyValuePair<string, string?>>
                {
                    new ("OPENSEARCH:AUTHMODE", "OAuth2"),
                    new ("OPENSEARCH:CLIENTID", "clientId"),
                    new ("OPENSEARCH:CLIENTSECRET", "1234")
                }
            };
        }

        [Theory]
        [MemberData(nameof(TestConfigurationActions))]
        public async Task Should_WriteAndReadToOpenSearch_When_UsingAuthentication(string authType, List<KeyValuePair<string, string?>> additionalConfiguration)
        {
            // This line and the parameter are really just here to make it easier to see which test is running (or failing)
            _testOutputHelper.WriteLine($"Testing OpenSearch connectivity using {authType}");
            
            /*
             * The structure here is a little complex, but the goal is to
             * allow us to specify test-specific parts of the configuration through the actions (e.g. AuthMode, UserName, etc.)
             * while allowing us to specify environment-specific parts through defaults (running locally)
             * or through environment variables (running in docker)
             */
            
            var config = GetDefaultConfigurationBuilder().AddInMemoryCollection(additionalConfiguration).Build();
            var sut = CreateClient(config);
            
            var newIndexName = Guid.NewGuid().ToString();
            var newIndicesResponse = await sut.Indices.CreateAsync(new CreateIndexRequest(newIndexName));
            Assert.True(newIndicesResponse.Acknowledged);

            var indices = await IndicesWithoutInternal(sut);

            Assert.Contains(newIndexName, indices);

            await sut.Indices.DeleteAsync(new DeleteIndexRequest(newIndexName));

            indices = await IndicesWithoutInternal(sut);
            Assert.DoesNotContain(newIndexName, indices);
        }

        static async Task<List<string>> IndicesWithoutInternal(IOpenSearchClient sut)
        {
            var indicesResponse = await sut.Indices.GetAsync(new GetIndexRequest(Indices.All));
            var indicesWithoutInternal = indicesResponse.Indices.Select(index => index.Key.ToString())
                .Where(x => !x.StartsWith('.')).ToList();
            return indicesWithoutInternal;
        }

        /// <summary>
        /// Gets the default configuration based on static dictionary of local values, with the option to override using environment variables
        /// </summary>
        /// <returns>A configuration with default values, optionally overriden by environment variables</returns>
        private static IConfigurationBuilder GetDefaultConfigurationBuilder()
        {
            var configurationDict = new Dictionary<string, string?>
            {
                { "OPENSEARCH:URL", "http://localhost:9200" },
                { "OPENSEARCH:TOKENENDPOINT", "http://localhost:1752/oauth2/token" }
            };
            
            return new ConfigurationBuilder()
                .AddInMemoryCollection(configurationDict)
                .AddEnvironmentVariables();
        }

        private static IOpenSearchClient CreateClient(IConfiguration config)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.AddSingleton<IHostEnvironment>(new HostingEnvironment()
            {
                EnvironmentName = "Development"
            });
            serviceCollection.AddCheetahOpenSearch(config);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider.GetRequiredService<IOpenSearchClient>();
        }
    }
}
