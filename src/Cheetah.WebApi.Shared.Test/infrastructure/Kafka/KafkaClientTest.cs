using System.Threading;
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
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Text;

namespace Cheetah.WebApi.Shared.Test.Infrastructure.OpenSearch
{
    [Trait("Category", "Kafka"), Trait("TestType", "IntegrationTests")]
    public class KafkaIntegrationTests
    {
        public KafkaIntegrationTests(ITestOutputHelper output)
        {

        }

        // Inspired by: https://github.com/confluentinc/confluent-kafka-dotnet/blob/master/test/Confluent.Kafka.IntegrationTests/Tests/SimpleProduceConsume.cs
        [Theory]
        [InlineData("kafka", "1234", "http://cheetahoauthsimulator:80/oauth2/token")]
        public async Task SimpleProduceConsume(string clientId, string clientSecret, string tokenEndpoint)
        {
            // Arrange
            var kafkaConfig = new KafkaConfig
            {
                KafkaUrl = "kafka:19093",
                ClientId = clientId,
                ClientSecret = clientSecret,
                TokenEndpoint = tokenEndpoint
            };

            var services = new ServiceCollection();
            services.AddTransient<IOptions<KafkaConfig>>(p => Options.Create(kafkaConfig));
            services.AddMemoryCache();
            services.AddHttpClient();
            services.AddLogging(s =>
            {
                s.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
                s.AddFilter("System.Net.Http.HttpClient", Microsoft.Extensions.Logging.LogLevel.Warning);
                s.AddConsole();
            });



            var clientConfig = new ClientConfig
            {
                BootstrapServers = kafkaConfig.KafkaUrl,
                SecurityProtocol = SecurityProtocol.SaslPlaintext,
                SaslMechanism = SaslMechanism.OAuthBearer,

            };

            var provider = services.BuildServiceProvider();

            var producer = new ProducerBuilder<Null, string>(new ProducerConfig(clientConfig)
            {
            })
               .AddCheetahOAuthentication(provider)
               .Build();


            var consumer = new ConsumerBuilder<Ignore, string>(new ConsumerConfig(clientConfig)
            {
                GroupId = Guid.NewGuid().ToString(),
                SessionTimeoutMs = 6000
            })
                .AddCheetahOAuthentication(provider)
                .Build();
            var testString = "a log message";
            var topic = "yo";

            // act
            Action<DeliveryReport<Null, string>> handler = (report) =>
            {
                Console.WriteLine("yo");
                Assert.Equal(expected: PersistenceStatus.Persisted, report.Status);
                Assert.Equal(0, producer.Flush(TimeSpan.FromSeconds(10)));

            };

            var result = producer.ProduceAsync(topic, new Message<Null, string> { Value = testString }).Result;
            Assert.Equal(0, producer.Flush(TimeSpan.FromSeconds(10)));

            consumer.Subscribe("examples");
            // var consumeResult = consumer.Consume(TimeSpan.FromSeconds(10));
            // Assert.NotNull(consumeResult?.Message);
            //Assert.Equal(testString, consumeResult?.Message.Value);

        }
    }
}
