using System;
using System.Threading;
using Cheetah.Core.Authentication;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Cheetah.Kafka.Extensions
{
    public static class CheetahKafkaExtensions
    {

        public static ConsumerBuilder<TKey, TValue> AddCheetahOAuthentication<TKey, TValue>(this ConsumerBuilder<TKey, TValue> builder, ITokenService tokenService, ILogger logger)
        {
            return builder.SetOAuthBearerTokenRefreshHandler((client, _) => TokenRefreshHandler(client, tokenService, logger));
        }

        public static ProducerBuilder<TKey, TValue> AddCheetahOAuthentication<TKey, TValue>(this ProducerBuilder<TKey, TValue> builder, ITokenService tokenService, ILogger logger)
        {
            return builder.SetOAuthBearerTokenRefreshHandler((client, _) => TokenRefreshHandler(client, tokenService, logger));
        }
        
        public static AdminClientBuilder AddCheetahOAuthentication(this AdminClientBuilder builder, ITokenService tokenService, ILogger logger)
        {
            return builder.SetOAuthBearerTokenRefreshHandler((client, _) => TokenRefreshHandler(client, tokenService, logger));
        }
        
        private static void TokenRefreshHandler(
            IClient client,
            ITokenService tokenService,
            ILogger logger
        )
        {
            var token = tokenService.RequestClientCredentialsTokenAsync(CancellationToken.None).GetAwaiter().GetResult();
            if (string.IsNullOrEmpty(token.AccessToken))
            {
                logger.LogError("Could not retrieve oauth2 accesstoken from provided token service");
                client.OAuthBearerSetTokenFailure(
                    "Could not retrieve access token from IDP. Look at environment values to ensure they are correct"
                );
                return;
            }

            long expiration = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn).ToUnixTimeMilliseconds();
            client.OAuthBearerSetToken(token.AccessToken, expiration, "unused");
        }
    }
}
