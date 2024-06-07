using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cheetah.Kafka.Testing;
using Confluent.Kafka;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Cheetah.Kafka.Test
{
    [Trait("TestType", "IntegrationTests")]
    [Collection("IntegrationTests")]
    public class OauthKafkaTestClientFactoryTests
    {
        readonly KafkaTestClientFactory _testClientFactory;

        public OauthKafkaTestClientFactoryTests()
        {
            var localConfig = new Dictionary<string, string?>
            {
                { "KAFKA:URL", "localhost:9093" },
                { "KAFKA:SECURITYPROTOCOL", "Plaintext" }
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
            IEnumerable<Message<string, string>> readMessages = reader.ReadMessages(1, TimeSpan.FromSeconds(5));
            readMessages.Should().HaveCount(1);
            Assert.Equal("Message4", readMessages.First().Value);
            reader.VerifyNoMoreMessages(TimeSpan.FromSeconds(1)).Should().BeTrue();
        }
    }
}
