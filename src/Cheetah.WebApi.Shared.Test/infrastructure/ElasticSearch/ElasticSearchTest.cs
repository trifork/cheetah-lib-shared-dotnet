using Cheetah.Shared.WebApi.Infrastructure.Services.ElasticSearch;
using Cheetah.WebApi.Shared.Infrastructure.Services.IndexAccess;
using Cheetah.WebApi.Shared.Middleware.Metric;
using Cheetah.template.WebApi.Core.Config;
using Elasticsearch.Net;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace Cheetah.WebApi.Shared.Test.Infrastructure.ElasticSearch
{
    public class ElasticSearchTest
    {
        [Fact]
        public async void ConnectingToInvalidPortFails()
        {
            var elasticConfig = new ElasticConfig();
            // Elastic is not running on port 80 so requests will fail
            elasticConfig.Url = "http://localhost:80";
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
            await Assert.ThrowsAsync<ElasticsearchClientException>(async () => await client.GetIndicies(new List<IndexDescriptor>()));
        }
    }

    public class ElasticSearchIntegrationTest
    {
        [Fact]
        public async void ConnectingToContainerElasticIntegration()
        {
            var elasticConfig = new ElasticConfig();
            elasticConfig.Url = "http://localhost:9200";
            elasticConfig.UserName = "elastic";
            elasticConfig.Password = "custom_elastic_password_for_testing asdasd";
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
            var actual = await client.GetIndicies(new List<IndexDescriptor>());
            Assert.Equal(actual, new List<string>());
        }
    }
}
