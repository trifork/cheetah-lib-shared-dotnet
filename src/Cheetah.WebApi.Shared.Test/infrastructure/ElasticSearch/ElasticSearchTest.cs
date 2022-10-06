using Cheetah.Shared.WebApi.Infrastructure.Services.ElasticSearch;
using Xunit;
using Microsoft.Extensions.Options;
using Cheetah.template.WebApi.Core.Config;
using Moq;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Cheetah.WebApi.Shared.Middleware.Metric;
using System.Collections.Generic;
using Cheetah.WebApi.Shared.Infrastructure.Services.IndexAccess;
using Elasticsearch.Net;

namespace Cheetah.WebApi.Shared_test.infrastructure.IndexFragments
{
    public class ElasticSearchTest
    {
        [Fact]
        public async void ShouldRejectCustomerIdentifiersWithIllegalChars()
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
}
