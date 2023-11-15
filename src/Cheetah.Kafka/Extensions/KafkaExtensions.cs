using System;
using System.Net.Http;
using System.Threading;
using Cheetah.Core.Authentication;
using Cheetah.Kafka.Config;
using Confluent.Kafka;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Kafka.Extensions
{
    public static class CheetahKafkaExtensions
    {
        private static void TokenRefreshHandler(
            ILogger logger,
            ITokenService tokenService,
            IClient client,
            string cfg
        )
        {
            var cachedAccessToken = tokenService
                .RequestClientCredentialsTokenAsync(CancellationToken.None)
                .GetAwaiter()
                .GetResult();
            if (cachedAccessToken == null || string.IsNullOrEmpty(cachedAccessToken.AccessToken))
            {
                logger.LogError($"Could not retrieve oauth2 accesstoken from {cfg}", cfg);
                client.OAuthBearerSetTokenFailure(
                    "Could not retrieve access token from IDP. Look at environment values to ensure they are correct"
                );
                return;
            }
            logger.LogDebug($"Forwarded new oauth2 accesstoken to kafka from  {cfg}", cfg);
            client.OAuthBearerSetToken(
                cachedAccessToken.AccessToken,
                DateTimeOffset.UtcNow
                    .AddSeconds(cachedAccessToken.ExpiresIn)
                    .ToUnixTimeMilliseconds(),
                "unused"
            );
        }

        private static CheetahKafkaTokenService BuildTokenService(
            ILogger logger,
            IServiceProvider provider
        ) // We are not using DI, as we do not know which settings to look at
        {
            var oauthConfig = provider.GetRequiredService<IOptions<KafkaConfig>>();
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            var cache = provider.GetRequiredService<IMemoryCache>();

            var tokenService = new CheetahKafkaTokenService(
                logger,
                httpClientFactory,
                cache,
                oauthConfig
            );
            return tokenService;
        }

        /// <summary>
        /// Setup OAuth authentication for Kafka consumer
        /// </summary>
        public static ConsumerBuilder<TKey, TValue> AddCheetahOAuthentication<TKey, TValue>(
            this ConsumerBuilder<TKey, TValue> builder,
            IServiceProvider provider
        )
        {
            var logger = provider.GetRequiredService<ILogger<CheetahKafkaTokenService>>();

            var tokenService = BuildTokenService(logger, provider);
            builder.SetOAuthBearerTokenRefreshHandler(
                (client, cfg) => TokenRefreshHandler(logger, tokenService, client, cfg)
            );
            return builder;
        }

        /// <summary>
        /// Setup OAuth authentication for Kafka producer
        /// </summary>
        public static ProducerBuilder<TKey, TValue> AddCheetahOAuthentication<TKey, TValue>(
            this ProducerBuilder<TKey, TValue> builder,
            IServiceProvider provider
        )
        {
            var logger = provider.GetRequiredService<ILogger<CheetahKafkaTokenService>>();
            var tokenService = BuildTokenService(logger, provider);
            builder.SetOAuthBearerTokenRefreshHandler(
                (client, cfg) => TokenRefreshHandler(logger, tokenService, client, cfg)
            );
            return builder;
        }

        /// <summary>
        /// Setup OAuth authentication for Kafka producer
        /// </summary>
        public static AdminClientBuilder AddCheetahOAuthentication(
            this AdminClientBuilder builder,
            IServiceProvider provider
        )
        {
            var logger = provider.GetRequiredService<ILogger<CheetahKafkaTokenService>>();
            var tokenService = BuildTokenService(logger, provider);
            builder.SetOAuthBearerTokenRefreshHandler(
                (client, cfg) => TokenRefreshHandler(logger, tokenService, client, cfg)
            );
            return builder;
        }
        
        public static ConsumerBuilder<TKey, TValue> AddCheetahOAuthentication<TKey, TValue>(this ConsumerBuilder<TKey, TValue> builder, ITokenService tokenService, ILogger logger)
        {
            return builder.SetOAuthBearerTokenRefreshHandler((client, cfg) => TokenRefreshHandler(logger, tokenService, client, cfg));
        }

        public static ProducerBuilder<TKey, TValue> AddCheetahOAuthentication<TKey, TValue>(this ProducerBuilder<TKey, TValue> builder, ITokenService tokenService, ILogger logger)
        {
            return builder.SetOAuthBearerTokenRefreshHandler((client, cfg) => TokenRefreshHandler(logger, tokenService, client, cfg));
        }
    }
}
