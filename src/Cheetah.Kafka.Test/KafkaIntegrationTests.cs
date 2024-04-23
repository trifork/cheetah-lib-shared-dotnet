using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cheetah.Auth.Authentication;
using Cheetah.Kafka.Extensions;
using Cheetah.Kafka.Test.Util;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Cheetah.Kafka.Test
{
    [Trait("Category", "Kafka"), Trait("TestType", "IntegrationTests")]
    public class KafkaIntegrationTests
    {
        readonly IServiceProvider _serviceProvider;

        public KafkaIntegrationTests()
        {
            var localConfig = new Dictionary<string, string?>
            {
                { "KAFKA:URL", "localhost:9092" },
                { "KAFKA:OAUTH2:CLIENTID", "default-access" },
                { "KAFKA:OAUTH2:CLIENTSECRET", "default-access-secret" },
                { "KAFKA:OAUTH2:TOKENENDPOINT", "http://localhost:1852/realms/local-development/protocol/openid-connect/token " },
                { "KAFKA:OAUTH2:SCOPE", "kafka schema-registry" },
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(localConfig)
                .AddEnvironmentVariables() // Allow override of config through environment variables if running in docker.
                .Build();

            var services = new ServiceCollection();
            services.AddLogging(s =>
            {
                s.SetMinimumLevel(LogLevel.Debug);
                s.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
                s.AddConsole();
            });
            
            services
                .AddCheetahKafka(configuration)
                .WithProducer<string, string>()
                .WithConsumer<string, string>(options =>
                {
                    options.ConfigureClient(cfg =>
                    {
                        cfg.GroupId = $"{nameof(KafkaIntegrationTests)}_{Guid.NewGuid()}";
                        cfg.AutoOffsetReset = AutoOffsetReset.Earliest;
                    });
                })
                .WithAdminClient();

            _serviceProvider = services.BuildServiceProvider();
            
            var bgService = _serviceProvider.GetRequiredService<IHostedService>();
            bgService.StartAsync(CancellationToken.None);
        }

        [Fact]
        public async Task Should_ReturnEqualAccessToken_When_TheExpiryIsNotOverdue()
        {
            // Arrange
            var cachedTokenProvider = _serviceProvider.GetRequiredKeyedService<ITokenService>("kafka");
            
            // Act
            var response1 = await cachedTokenProvider.RequestAccessTokenAsync(CancellationToken.None);
            
            await Task.Delay(TimeSpan.FromSeconds(30));
            
            var response2 = await cachedTokenProvider.RequestAccessTokenAsync(CancellationToken.None);
            
            // Assert
            Assert.Equal(response1.Item1, response2.Item1);
        }

        [Fact]
        public async Task OAuthBearerToken_PublishConsume()
        {
            // Arrange
            string topic = $"dotnet_{nameof(OAuthBearerToken_PublishConsume)}_{Guid.NewGuid()}";

            // Emulate a service injecting an IAdminClient, IProducer and IConsumer
            await using var topicDeleter = new KafkaTopicDeleter(
                _serviceProvider.GetRequiredService<IAdminClient>(),
                topic
            ); // Will delete the created topic when the test concludes, regardless of outcome
            var producer = _serviceProvider.GetRequiredService<IProducer<string, string>>();
            var consumer = _serviceProvider.GetRequiredService<IConsumer<string, string>>();
            
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
