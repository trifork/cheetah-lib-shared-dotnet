using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cheetah.Auth.Authentication;
using Cheetah.Auth.Configuration;
using Cheetah.Auth.Extensions;
using Cheetah.Kafka.Configuration;
using Cheetah.Kafka.Extensions;
using Cheetah.Kafka.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
                { "KAFKA:SECURITYPROTOCOL", "SaslPlaintext" },
                { "KAFKA:SASLMECHANISM", "OAuthBearer" },
            };

            configurationDictionary.Remove(missingKey);

            var invalidConfiguration = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationDictionary)
                .Build();

            Assert.Throws<ArgumentNullException>(() => KafkaTestClientFactory.Create(invalidConfiguration));
        }

        [Fact]
        public async Task Should_Use_Custom_Provided_ITokenService()
        {
            IServiceProvider _serviceProvider;
            var localConfig = new Dictionary<string, string?>
            {
                { "KAFKA:URL", "localhost:9092" },
                { "KAFKA:OAUTH2:CLIENTID", "default-access" },
                { "KAFKA:OAUTH2:CLIENTSECRET", "default-access-secret" },
                { "KAFKA:OAUTH2:TOKENENDPOINT", "http://localhost:1852/realms/local-development/protocol/openid-connect/token " },
                { "KAFKA:OAUTH2:SCOPE", "kafka" },
                { "KAFKA:SECURITYPROTOCOL", "SaslPlaintext" },
                { "KAFKA:SASLMECHANISM", "OAuthBearer" },
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(localConfig)
                .Build();

            var configOAuth = new OAuth2Config();
            configuration.GetSection(KafkaConfig.Position).GetSection(nameof(KafkaConfig.OAuth2)).Bind(configOAuth);
            configOAuth.ClientId = "default-access";
            configOAuth.Validate();


            var services = new ServiceCollection();
            services.TryAddCheetahKeyedTokenService(Constants.TokenServiceKey, configOAuth);
            services.AddCheetahKafka(configuration).WithAdminClient();
            _serviceProvider = services.BuildServiceProvider();


            var bgServices = _serviceProvider.GetServices<IHostedService>();
            foreach (var bgService in bgServices)
            {
                await bgService.StartAsync(CancellationToken.None);
            }

            var tokenService = _serviceProvider.GetRequiredKeyedService<ITokenService>(Constants.TokenServiceKey);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(5));
            (string AccessToken, _) = await tokenService.RequestAccessTokenAsync(cancellationTokenSource.Token);

            Assert.NotNull(AccessToken);
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
