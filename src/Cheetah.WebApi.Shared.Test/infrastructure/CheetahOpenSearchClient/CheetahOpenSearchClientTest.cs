using Cheetah.WebApi.Shared.Infrastructure.Services.IndexAccess;
using Cheetah.WebApi.Shared.Middleware.Metric;
using Cheetah.Shared.WebApi.Core.Config;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Collections.Generic;
using Xunit;
using System;
using Cheetah.Shared.WebApi.Infrastructure.Services.CheetahOpenSearchClient;
using Microsoft.Extensions.Caching.Memory;
using Cheetah.WebApi.Shared_test.TestUtils;
using OpenSearch.Client;
using Xunit.Abstractions;
using Microsoft.Extensions.Hosting.Internal;

namespace Cheetah.WebApi.Shared.Test.Infrastructure.OpenSearch
{
    public class OpenSearchIntegrationTest
    {
        private readonly string port;
        public OpenSearchIntegrationTest(ITestOutputHelper output)
        {
            // The default opensearch port used in the CI/local container
            port = "9200";
        }

        [Theory]
        [InlineData(OpenSearchConfig.OpenSearchAuthMode.BasicAuth, "admin", "admin", "", "", "")]
        [InlineData(OpenSearchConfig.OpenSearchAuthMode.OAuth2, "", "", "opensearch", "1234", "http://cheetahoauthsimulator:80/oauth2/token")]
        public async void GetIndicesIntegration(OpenSearchConfig.OpenSearchAuthMode authMode, string username, string password, string clientId, string clientSecret, string tokenEndpoint)
        {
            var openSearchConfig = new OpenSearchConfig
            {
                AuthMode = authMode,
                Url = $"http://opensearch:{port}",
                // Basic auth
                UserName = username,
                Password = password,
                // Oauth2
                ClientId = clientId,
                ClientSecret = clientSecret,
                TokenEndpoint = tokenEndpoint
            };
            var options = Options.Create(openSearchConfig);
            var env = new HostingEnvironment { EnvironmentName = Environments.Development };

            var mockMetricReporter = new Mock<IMetricReporter>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions
            {
            });
            var httpClientfactory = new DefaultHttpClientFactory();
            var loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
                    builder.AddConsole();
                });

            var logger = loggerFactory.CreateLogger<CheetahOpenSearchClient>();
            CheetahOpenSearchClient client = new CheetahOpenSearchClient(
                memoryCache,
                httpClientfactory,
                options,
                env,
                logger,
                mockMetricReporter.Object);

            var newIndexName = Guid.NewGuid().ToString();
            var newIndicesResponse = client.InternalClient.Indices.Create(new CreateIndexRequest(newIndexName));
            Assert.True(newIndicesResponse.Acknowledged);

            var indices = await client.GetIndices(new List<IndexDescriptor>());
            Assert.Contains(newIndexName, indices);

            client.InternalClient.Indices.Delete(new DeleteIndexRequest(newIndexName));

            indices = await client.GetIndices(new List<IndexDescriptor>());
            Assert.DoesNotContain(newIndexName, indices);
        }
    }
}
