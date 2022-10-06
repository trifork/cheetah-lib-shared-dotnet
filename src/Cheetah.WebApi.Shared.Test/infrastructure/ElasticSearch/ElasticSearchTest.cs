using Cheetah.Shared.WebApi.Infrastructure.Services.ElasticSearch;
using Cheetah.WebApi.Shared.Infrastructure.Services.IndexAccess;
using Cheetah.WebApi.Shared.Middleware.Metric;
using Cheetah.Shared.WebApi.Core.Config;
using Elasticsearch.Net;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Collections.Generic;
using Xunit;
using System;
using Nest;

namespace Cheetah.WebApi.Shared.Test.Infrastructure.ElasticSearch
{
    public class ElasticSearchTest
    {
        [Fact]
        public async void ConnectingToInvalidPortFails()
        {
            var elasticConfig = new ElasticConfig();
            // Elastic should never run on port 80 so requests will fail
            elasticConfig.Url = "http://localhost:80";
            var options = Options.Create(elasticConfig);
            var mockEnv = new Mock<IHostEnvironment>();
            mockEnv.Setup(s => s.EnvironmentName).Returns(Environments.Development);
            var mockLogger = new Mock<ILogger<CheetahElasticClient>>();
            var mockMetricReporter = new Mock<IMetricReporter>();
            // Initialization succeeds, but the connection is not verified until a request is made
            CheetahElasticClient client = new CheetahElasticClient(
                options,
                mockEnv.Object,
                mockLogger.Object,
                mockMetricReporter.Object);
            await Assert.ThrowsAsync<ElasticsearchClientException>(async () => await client.GetIndices(new List<IndexDescriptor>()));
        }
    }

    public class ElasticSearchIntegrationTest
    {
        // elasticClient is an unprotected client for elastic. It helps with setting-up or tearing down tests
        private ElasticClient elasticClient;
        private string port;
        public ElasticSearchIntegrationTest()
        {
            // The default elastic port used in the CI/local container
            port = "9200";
            elasticClient = new ElasticClient(new Uri($"http://localhost:{port}"));
        }
        [Fact]
        public async void GetIndicesIntegration()
        {
            var elasticConfig = new ElasticConfig();
            elasticConfig.Url = $"http://localhost:{port}";
            var options = Options.Create(elasticConfig);
            var mockEnv = new Mock<IHostEnvironment>();
            mockEnv.Setup(s => s.EnvironmentName).Returns(Environments.Development);
            var mockLogger = new Mock<ILogger<CheetahElasticClient>>();
            var mockMetricReporter = new Mock<IMetricReporter>();
            CheetahElasticClient client = new CheetahElasticClient(
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

        public CreateIndexResponse CreateIndex(IndexName index)
        {
            return elasticClient.Indices.Create(new CreateIndexRequest(index));
        }

        public DeleteIndexResponse DeleteIndex(IndexName index)
        {
            return elasticClient.Indices.Delete(new DeleteIndexRequest(index));
        }

        public string GenerateRandomIndexName()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
