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

using System.Net.Http;

using Microsoft.Extensions.Caching.Memory;

using Cheetah.WebApi.Shared_test.TestUtils;

using OpenSearch.Client;

using OpenSearch.Net;

using static Cheetah.Shared.WebApi.Core.Config.OpenSearchConfig;

using Serilog;

using Xunit.Abstractions;

using Microsoft.Extensions.Logging.Configuration;

using Microsoft.Extensions.Hosting.Internal;

namespace Cheetah.WebApi.Shared.Test.Infrastructure.ElasticSearch
{
    public class ElasticSearchTest
    {
        [Fact]
        public async void ConnectingToInvalidPortFails()
        {
            var openSearchConfig = new OpenSearchConfig
            {
                Url = $"http://localhost:80",
                AuthMode = OpenSearchAuthMode.BasicAuth,
                UserName = "admin",
                Password = "admin"
            };
            var options = Options.Create(openSearchConfig);
            var mockEnv = new Mock<IHostEnvironment>();
            mockEnv.Setup(s => s.EnvironmentName).Returns(Environments.Development);
            var mockLogger = new Mock<ILogger<CheetahOpenSearchClient>>();
            var mockMetricReporter = new Mock<IMetricReporter>();
            var cache = new Mock<IMemoryCache>();
            var httpClientfactory = new Mock<IHttpClientFactory>();

            // Initialization succeeds, but the connection is not verified until a request is made
            CheetahOpenSearchClient client = new CheetahOpenSearchClient(
                cache.Object,
                httpClientfactory.Object,
                options,
                mockEnv.Object,
                mockLogger.Object,
                mockMetricReporter.Object);
            await Assert.ThrowsAsync<OpenSearchClientException>(async () => await client.GetIndices(new List<IndexDescriptor>()));
        }
    }

    public class OpenSearchIntegrationTest
    {
        // elasticClient is an unprotected client for elastic. It helps with setting-up or tearing down tests
        private readonly string port;
        public OpenSearchIntegrationTest(ITestOutputHelper output)
        {
            // The default elastic port used in the CI/local container
            port = "9200";
        }



        [Theory]
        [InlineData(OpenSearchAuthMode.BasicAuth, "admin", "admin", "", "", "")]
        //[InlineData(OpenSearchAuthMode.OAuth2, "", "", "opensearch", "1234", "http://cheetahoauthsimulator:80/oauth2/token")]
        public async void GetIndicesIntegration(OpenSearchAuthMode authMode, string username, string password, string clientId, string clientSecret, string tokenEndpoint)
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
