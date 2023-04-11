using Cheetah.Shared.WebApi.Core.Config;
using Cheetah.WebApi.Shared.Infrastructure.Auth;
using Confluent.Kafka;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public static class CheetahKafkaExtensions
{
    private static void TokenRefreshHandler(CheetahKafkaTokenService tokenService, IClient client, string cfg)
    {
        var cachedAccessToken = tokenService.RequestAccessTokenCachedAsync(CancellationToken.None).GetAwaiter().GetResult();
        if (cachedAccessToken == null || string.IsNullOrEmpty(cachedAccessToken.AccessToken))
        {
            client.OAuthBearerSetTokenFailure("Could not retrieve access token from IDP. Look at environment values to ensure they are correct");
            return;
        }
        client.OAuthBearerSetToken(cachedAccessToken.AccessToken, (long)TimeSpan.FromSeconds(cachedAccessToken.ExpiresIn).TotalMilliseconds, null);
    }
    private static CheetahKafkaTokenService BuildTokenService(IServiceProvider provider) // We are not using DI, as we do not know which settings to look at
    {
        var oauthConfig = provider.GetRequiredService<IOptions<KafkaConfig>>();
        var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
        var cache = provider.GetRequiredService<IMemoryCache>();
        var logger = provider.GetRequiredService<ILogger<CheetahKafkaTokenService>>();

        var tokenService = new CheetahKafkaTokenService(logger, httpClientFactory, cache, oauthConfig.Value.ClientId, oauthConfig.Value.ClientSecret, oauthConfig.Value.TokenEndpoint);
        return tokenService;
    }
    public static ConsumerBuilder<TKey, TValue> AddCheetahOAuthentication<TKey, TValue>(this ConsumerBuilder<TKey, TValue> builder, IServiceProvider provider)
    {
        var tokenService = BuildTokenService(provider);
        builder.SetOAuthBearerTokenRefreshHandler((client, cfg) => TokenRefreshHandler(tokenService, client, cfg));

        return builder;
    }

    public static ProducerBuilder<TKey, TValue> AddCheetahOAuthentication<TKey, TValue>(this ProducerBuilder<TKey, TValue> builder, IServiceProvider provider)
    {
        var tokenService = BuildTokenService(provider);
        builder.SetOAuthBearerTokenRefreshHandler((client, cfg) => TokenRefreshHandler(tokenService, client, cfg));

        return builder;
    }
}