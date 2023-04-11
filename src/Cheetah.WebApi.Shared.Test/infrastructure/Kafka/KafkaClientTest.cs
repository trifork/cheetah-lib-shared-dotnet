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
        private ServiceCollection Sut;

        public KafkaIntegrationTests(ITestOutputHelper output)
        {

            var kafkaConfig = new KafkaConfig
            {
                KafkaUrl = "kafka:19093",
                ClientId = "tester",
                ClientSecret = "1234",
                TokenEndpoint = "http://cheetahoauthsimulator:80/oauth2/token"
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

            Sut = services;
        }

        //https://github.com/confluentinc/confluent-kafka-dotnet/blob/master/test/Confluent.Kafka.IntegrationTests/Tests/OauthBearerToken_PublishConsume.cs#L9
        [Fact]
        public void OAuthBearerToken_PublishConsume()
        {
            var provider = Sut.BuildServiceProvider();
            var kafkaConfig = provider.GetRequiredService<IOptions<KafkaConfig>>();

            var message = new Message<string, string>
            {
                Key = $"{Guid.NewGuid()}",
                Value = $"{DateTimeOffset.UtcNow:T}"
            };

            var config = new ClientConfig
            {
                BootstrapServers = kafkaConfig.Value.KafkaUrl,
                SecurityProtocol = SecurityProtocol.SaslPlaintext,
                SaslMechanism = SaslMechanism.OAuthBearer
            };
            var producerConfig = new ProducerConfig(config);
            var consumerConfig = new ConsumerConfig(config)
            {
                GroupId = $"{Guid.NewGuid()}",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            var producer = new ProducerBuilder<string, string>(producerConfig)
                .AddCheetahOAuthentication(provider)
                .Build();
            var consumer = new ConsumerBuilder<string, string>(consumerConfig)
                .AddCheetahOAuthentication(provider)
                .Build();

            consumer.Subscribe("exampletopic");
            producer.Produce("exampletopic", message);
            producer.Flush(TimeSpan.FromSeconds(30));
            var received = consumer.Consume(TimeSpan.FromSeconds(30));

            Assert.NotNull(received);
            consumer.Commit(received);

            Assert.Equal(message.Key, received.Message.Key);
            Assert.Equal(message.Value, received.Message.Value);

        }

    }
}
