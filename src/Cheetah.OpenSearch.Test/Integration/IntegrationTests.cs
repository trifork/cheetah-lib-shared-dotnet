using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cheetah.OpenSearch.Extensions;
using Cheetah.OpenSearch.Test.Models;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenSearch.Client;
using Xunit;
using Xunit.Abstractions;

namespace Cheetah.OpenSearch.Test.Integration
{
    [Trait("Category", "OpenSearch"), Trait("TestType", "IntegrationTests")]
    public class IntegrationTests
    {
        readonly ITestOutputHelper _testOutputHelper;

        public IntegrationTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        /// <summary>
        /// Supplies test cases to the OpenSearch integration test
        /// </summary>
        /// <returns>A list of test names and actions to apply to the default configuration</returns>
        public static TheoryData<
            string,
            List<KeyValuePair<string, string?>>
        > TestConfigurationActions()
        {
            var testCases = new TheoryData<string, List<KeyValuePair<string, string?>>>
            {
                {
                    "Basic Auth",
                    new List<KeyValuePair<string, string?>>
                {
                    new("OPENSEARCH:AUTHMODE", "Basic"),
                    new("OPENSEARCH:USERNAME", "admin"),
                    new("OPENSEARCH:PASSWORD", "admin")
                }
                },
                {
                    "OAuth2",
                    new List<KeyValuePair<string, string?>>
                {
                    new("OPENSEARCH:AUTHMODE", "OAuth2"),
                    new("OPENSEARCH:OAUTH2:TOKENENDPOINT", "http://localhost:1852/realms/local-development/protocol/openid-connect/token"),
                    new("OPENSEARCH:OAUTH2:CLIENTID", "default-access"),
                    new("OPENSEARCH:OAUTH2:CLIENTSECRET", "default-access-secret"),
                    new("OPENSEARCH:OAUTH2:SCOPE", "opensearch")
                }
                }
            };

            return testCases;
        }

        [Theory]
        [MemberData(nameof(TestConfigurationActions))]
        public async Task Should_WriteAndReadToOpenSearch_When_UsingAuthentication(
            string authType,
            List<KeyValuePair<string, string?>> additionalConfiguration
        )
        {
            // This line and the parameter are really just here to make it easier to see which test is running (or failing)
            _testOutputHelper.WriteLine($"Testing OpenSearch connectivity using {authType}");

            // Create client using DI
            var configurationRoot = GetDefaultConfigurationBuilder()
                .AddInMemoryCollection(additionalConfiguration)
                .Build();
            var serviceProvider = CreateServiceProvider(configurationRoot);
            var client = serviceProvider.GetRequiredService<IOpenSearchClient>();


            try
            {
                // Start the background service
                var bgServices = serviceProvider.GetServices<IHostedService>();
                foreach (var bgService in bgServices)
                {
                    await bgService.StartAsync(CancellationToken.None);
                }
            }
            catch (Exception)
            {
                // ignored
            }

            var indexName = Guid.NewGuid().ToString();

            var documents = new List<OpenSearchTestModel>
            {
                new("Document 1", 2),
                new("Document 2", 3),
                new("Document 3", 4),
                new("Document 4", 5),
            };

            // Make sure the index is empty - Okay if this fails, since the index might not be there.
            await client.DeleteIndexAsync(indexName, allowFailure: true);

            // Insert some data and verify its count
            await client.InsertAsync(indexName, documents);
            await client.RefreshIndexAsync(indexName);

            // Verify the correct count
            (await client.CountIndexedDocumentsAsync(indexName))
                .Should()
                .Be(documents.Count);

            // Verify that all our documents were inserted
            var actualDocuments = await client.GetFromIndexAsync<OpenSearchTestModel>(indexName);
            actualDocuments
                .Should()
                .BeEquivalentTo(documents, options => options.WithoutStrictOrdering());

            // Verify that we can delete it all again and that nothing is left
            await client.DeleteIndexAsync(indexName);
        }

        /// <summary>
        /// Gets the default configuration based on static dictionary of local values, with the option to override using environment variables
        /// </summary>
        /// <returns>A configuration with default values, optionally overriden by environment variables</returns>
        private static IConfigurationBuilder GetDefaultConfigurationBuilder()
        {
            var configurationDict = new Dictionary<string, string?>
            {
                { "OPENSEARCH:URL", "http://localhost:9200" }
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(configurationDict)
                .AddEnvironmentVariables();
        }

        private static ServiceProvider CreateServiceProvider(IConfiguration config)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.AddCheetahOpenSearch(config);
            return serviceCollection.BuildServiceProvider();
        }
    }
}
