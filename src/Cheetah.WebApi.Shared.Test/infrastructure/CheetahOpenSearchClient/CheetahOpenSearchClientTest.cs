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

namespace Cheetah.WebApi.Shared.Test.Infrastructure.ElasticSearch
{
    public class ElasticSearchTest
    {
        [Fact]
        public async void ConnectingToInvalidPortFails()
        {
            var openSearchConfig = new OpenSearchConfig();
            openSearchConfig.Url = $"http://localhost:80";
            openSearchConfig.UserName = "admin";
            openSearchConfig.Password = "admin";
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
        private OpenSearchClient openSearchClient;
        private string port;
        public OpenSearchIntegrationTest()
        {
            // The default elastic port used in the CI/local container
            port = "9229";
            openSearchClient = new OpenSearchClient(new Uri($"http://localhost:{port}"));
        }



        [Theory]
        [InlineData(OpenSearchAuthMode.BasicAuth, "admin", "admin", "", "", "")]
        [InlineData(OpenSearchAuthMode.OAuth2, "", "", "opensearch", "1234", "http://localhost:1752/oauth2/token")]
        public async void GetIndicesIntegration(OpenSearchAuthMode authMode, string username, string password, string clientId, string clientSecret, string tokenEndpoint)
        {
            var openSearchConfig = new OpenSearchConfig
            {
                AuthMode = OpenSearchConfig.OpenSearchAuthMode.BasicAuth,
                Url = $"http://localhost:{port}",
                UserName = "admin",
                Password = "admin"
            };
            var options = Options.Create(openSearchConfig);
            var mockEnv = new Mock<IHostEnvironment>();
            mockEnv.Setup(s => s.EnvironmentName).Returns(Environments.Development);
            var mockLogger = new Mock<ILogger<CheetahOpenSearchClient>>();
            var mockMetricReporter = new Mock<IMetricReporter>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = 1
            });
            var httpClientfactory = new DefaultHttpClientFactory();

            CheetahOpenSearchClient client = new CheetahOpenSearchClient(
                memoryCache,
        httpClientfactory,
                options,
                mockEnv.Object,
                mockLogger.Object,
                mockMetricReporter.Object);

            var newIndex = GenerateRandomIndexName();
            CreateIndex(newIndex);

            var indices = await client.GetIndices(new List<IndexDescriptor>());
            Assert.Contains(newIndex, indices);

            DeleteIndex(newIndex);

            indices = await client.GetIndices(new List<IndexDescriptor>());
            Assert.DoesNotContain(newIndex, indices);
        }

        private CreateIndexResponse CreateIndex(IndexName index)
        {
            return openSearchClient.Indices.Create(new CreateIndexRequest(index));
        }

        private DeleteIndexResponse DeleteIndex(IndexName index)
        {
            return openSearchClient.Indices.Delete(new DeleteIndexRequest(index));
        }

        private string GenerateRandomIndexName()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
