using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cheetah.Kafka.Config;
using Cheetah.Kafka.Extensions;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Cheetah.Kafka.Test
{
    [Trait("Category", "Kafka"), Trait("TestType", "IntegrationTests")]
    public class KafkaIntegrationTests
    {
        private readonly ServiceProvider _serviceProvider;
        readonly IAdminClient _adminClient;
        readonly ClientConfig _clientConfig;

        public KafkaIntegrationTests()
        {
            var localConfig = new Dictionary<string, string?> 
            {
                { "KAFKA:URL", "localhost:9092" },
                { "KAFKA:CLIENTID", "tester" },
                { "KAFKA:CLIENTSECRET", "1234" },
                { "KAFKA:TOKENENDPOINT", "http://localhost:1752/oauth2/token" },
                { "KAFKA:OAUTHSCOPE", "kafka" },
            };
            
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(localConfig)
                .AddEnvironmentVariables() // Allow override of config through environment variables if running in docker.
                .Build();

            var kafkaConfig = new KafkaConfig();
            configuration.GetSection(KafkaConfig.Position).Bind(kafkaConfig);
            
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

            _serviceProvider = services.BuildServiceProvider();
            _clientConfig = new ClientConfig {
                BootstrapServers = kafkaConfig.Url,
                SecurityProtocol = SecurityProtocol.SaslPlaintext,
                SaslMechanism = SaslMechanism.OAuthBearer
            };
            _adminClient = new AdminClientBuilder(_clientConfig)
                .AddCheetahOAuthentication(_serviceProvider)
                .Build();
        }

        [Fact]
        public async Task OAuthBearerToken_PublishConsume()
        {
            // Arrange
            string topic = $"dotnet_{nameof(OAuthBearerToken_PublishConsume)}_{Guid.NewGuid()}";
            await using var topicDeleter = new KafkaTopicDeleter(_adminClient, topic); // Will delete the created topic when the test concludes, regardless of outcome
            
            var consumerConfig = new ConsumerConfig(_clientConfig)
            {
                GroupId = $"{Guid.NewGuid()}",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            
            var producer = new ProducerBuilder<string, string>(_clientConfig)
                .AddCheetahOAuthentication(_serviceProvider)
                .Build();
            
            var consumer = new ConsumerBuilder<string, string>(consumerConfig)
                .AddCheetahOAuthentication(_serviceProvider)
                .Build();

            consumer.Subscribe(topic);

            var message = new Message<string, string>
            {
                Key = $"{Guid.NewGuid()}",
                Value = $"{DateTimeOffset.UtcNow:T}"
            };
            
            // Act
            await producer.ProduceAsync(topic, message);
            var received = consumer.Consume(TimeSpan.FromSeconds(5));
            
            // Assert
            Assert.NotNull(received);
            Assert.Equal(message.Key, received.Message.Key);
            Assert.Equal(message.Value, received.Message.Value);
            consumer.Commit(received);
        }
    }
}
