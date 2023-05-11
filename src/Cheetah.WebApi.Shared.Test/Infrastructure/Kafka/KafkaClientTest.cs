using System;
using Cheetah.Core.Core.Config;
using Cheetah.Core.Infrastucture.Services.Kafka;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Cheetah.WebApi.Shared.Test.Infrastructure.Kafka
{
    [Trait("Category", "Kafka"), Trait("TestType", "IntegrationTests")]
    public class KafkaIntegrationTests
    {
        private readonly ServiceCollection _sut;

        public KafkaIntegrationTests()
        {
            var kafkaConfig = new KafkaConfig
            {
                KafkaUrl = "kafka:19093",
                ClientId = "tester",
                ClientSecret = "1234",
                TokenEndpoint = "http://cheetahoauthsimulator:80/oauth2/token"
            };

            var services = new ServiceCollection();
            services.AddTransient(_ => Options.Create(kafkaConfig));
            services.AddMemoryCache();
            services.AddHttpClient();
            services.AddLogging(s =>
            {
                s.SetMinimumLevel(LogLevel.Debug);
                s.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
                s.AddConsole();
            });

            _sut = services;
        }

        //https://github.com/confluentinc/confluent-kafka-dotnet/blob/master/test/Confluent.Kafka.IntegrationTests/Tests/OauthBearerToken_PublishConsume.cs#L9
        [Fact]
        public void OAuthBearerToken_PublishConsume()
        {
            var provider = _sut.BuildServiceProvider();
            var kafkaConfig = provider.GetRequiredService<IOptions<KafkaConfig>>();
            var topic = $"dotnet_{nameof(OAuthBearerToken_PublishConsume)}_{Guid.NewGuid()}";

            var message = new Message<string, string>
            {
                Key = $"{Guid.NewGuid()}",
                Value = $"{DateTimeOffset.UtcNow:T}"
            };
            var producerConfig = new ProducerConfig(
                new ClientConfig
                {
                    BootstrapServers = kafkaConfig.Value.KafkaUrl,
                    SecurityProtocol = SecurityProtocol.SaslPlaintext,
                    SaslMechanism = SaslMechanism.OAuthBearer
                }
            );
            var consumerConfig = new ConsumerConfig(
                new ClientConfig
                {
                    BootstrapServers = kafkaConfig.Value.KafkaUrl,
                    SecurityProtocol = SecurityProtocol.SaslPlaintext,
                    SaslMechanism = SaslMechanism.OAuthBearer
                }
            )
            {
                GroupId = $"{Guid.NewGuid()}",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            var producer = new ProducerBuilder<string, string>(producerConfig)
                .SetErrorHandler((_, _) => { })
                .AddCheetahOAuthentication(provider)
                .Build();

            var consumer = new ConsumerBuilder<string, string>(consumerConfig)
                .AddCheetahOAuthentication(provider)
                .SetErrorHandler((_, _) => { })
                .Build();

            consumer.Subscribe(topic);
            producer.Produce(topic, message);
            producer.Flush(TimeSpan.FromSeconds(30));
            var received = consumer.Consume(TimeSpan.FromSeconds(30));

            Assert.NotNull(received);
            consumer.Commit(received);

            Assert.Equal(message.Key, received.Message.Key);
            Assert.Equal(message.Value, received.Message.Value);
        }
    }
}
