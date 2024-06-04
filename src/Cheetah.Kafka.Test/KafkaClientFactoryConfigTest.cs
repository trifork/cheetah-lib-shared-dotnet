using System;
using System.Collections.Generic;
using Cheetah.Kafka.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Cheetah.Kafka.Test
{
    [Trait("Category", "Kafka"), Trait("TestType", "IntegrationTests")]
    public class KafkaClientFactoryConfigTest
    {
        [Theory]
        [InlineData("KAFKA:OAUTH2:CLIENTID")]
        [InlineData("KAFKA:OAUTH2:CLIENTSECRET")]
        [InlineData("KAFKA:OAUTH2:TOKENENDPOINT")]
        public void Should_ThrowArgumentNullException_When_RequiredConfigurationIsMissing(string missingKey)
        {
            var configurationDictionary = new Dictionary<string, string?>
            {
                { "KAFKA:URL", "localhost:9092" },
                { "KAFKA:OAUTH2:CLIENTID", "default-access" },
                { "KAFKA:OAUTH2:CLIENTSECRET", "default-access-secret" },
                { "KAFKA:OAUTH2:TOKENENDPOINT", "http://localhost:1852/realms/local-development/protocol/openid-connect/token " },
            };

            configurationDictionary.Remove(missingKey);

            var invalidConfiguration = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationDictionary)
                .Build();

            Assert.Throws<ArgumentNullException>(() => KafkaTestClientFactory.Create(invalidConfiguration));
        }

        [Theory]
        [InlineData("https://")]
        [InlineData("http://")]
        [InlineData("ftp://")]
        [InlineData("ssh://")]
        [InlineData("://")]
        public void Should_ThrowArgumentException_When_KafkaUrlIsInvalid(string kafkaUrl)
        {
            var configurationDictionary = new Dictionary<string, string?>
            {
                { "KAFKA:URL", kafkaUrl + "localhost:9092" },
                { "KAFKA:OAUTH2:CLIENTID", "default-access" },
                { "KAFKA:OAUTH2:CLIENTSECRET", "default-access-secret" },
                { "KAFKA:OAUTH2:TOKENENDPOINT", "http://localhost:1852/realms/local-development/protocol/openid-connect/token " },
            };
            var invalidConfiguration = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationDictionary)
                .Build();

            Assert.Throws<ArgumentException>(() => KafkaTestClientFactory.Create(invalidConfiguration));
        }
    }
}
