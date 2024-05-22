using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cheetah.Kafka.Testing;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Cheetah.Kafka.Test
{
    [Trait("TestType", "IntegrationTests")]
    [Collection("IntegrationTests")]
    public class KafkaTestClientFactoryTests
    {
        readonly KafkaTestClientFactory _testClientFactory;

        public KafkaTestClientFactoryTests()
        {
            var localConfig = new Dictionary<string, string?>
            {
                { "KAFKA:URL", "localhost:9092" },
                { "KAFKA:OAUTH2:CLIENTID", "default-access" },
                { "KAFKA:OAUTH2:CLIENTSECRET", "default-access-secret" },
                { "KAFKA:OAUTH2:TOKENENDPOINT", "http://localhost:1852/realms/local-development/protocol/openid-connect/token " },
                { "KAFKA:OAUTH2:SCOPE", "kafka" },
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(localConfig)
                .AddEnvironmentVariables()
                .Build();

            _testClientFactory = KafkaTestClientFactory.Create(configuration);
        }

        [Fact]
        public async Task Should_WriteAndRead_When_UsingJson()
        {
            var writer = _testClientFactory.CreateTestWriter<string, string>(
                "MyJsonTopic",
                message => message
            );
            var reader = _testClientFactory.CreateTestReader<string, string>(
                "MyJsonTopic",
                "MyConsumerGroup"
            );

            await writer.WriteAsync("Message4");
            var readMessages = reader.ReadMessages(1, TimeSpan.FromSeconds(5));
            readMessages.Should().HaveCount(1);
            reader.VerifyNoMoreMessages(TimeSpan.FromSeconds(1)).Should().BeTrue();
        }

        [Fact]
        public async Task Should_ThrowArgumentException_When_AttemptingToWrite0Messages()
        {
            var writer = _testClientFactory.CreateTestWriter<string, string>(
                "MyThrowinTopic",
                message => message
            );

            await writer
                .Invoking(w => w.WriteAsync())
                .Should()
                .ThrowAsync<ArgumentException>("it should not be possible to write 0 messages");
        }

        [Theory]
        [InlineData("")]
        [InlineData("my!cool:topic#")]
        [InlineData("my$expensive$topic")]
        // 249 characters is the maximum allowed length, this is 250 'a's
        [InlineData(
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
        )]
        public void Should_ThrowArgumentException_When_ProvidedInvalidTopicName(string topicName)
        {
            _testClientFactory
                .Invoking(factory => factory.CreateTestWriter<string>(topicName))
                .Should()
                .Throw<ArgumentException>(
                    "the factory should not be able to create clients with invalid topic names"
                );

            _testClientFactory
                .Invoking(factory => factory.CreateTestReader<string>(topicName))
                .Should()
                .Throw<ArgumentException>(
                    "the factory should not be able to create clients with invalid topic names"
                );
        }


    }
}
