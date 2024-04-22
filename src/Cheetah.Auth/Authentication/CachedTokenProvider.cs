using System;
using System.Threading;
using System.Threading.Tasks;
using Cheetah.Auth.Configuration;
using IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Auth.Authentication
{
    /// <summary>
    /// CachedTokenProvider manages the retrieval and caching of OAuth2 tokens, optimizing performance by reducing unnecessary token requests.
    /// It includes a mechanism for refreshing tokens in a separate thread, ensuring a consistent supply of valid tokens.
    /// IMPORTANT: Before calling RequestAccessToken(), ensure to invoke StartAsync() unless you're utilizing Dependency Injection, where this process is managed by the builder.RunAsync() method.
    /// </summary>
    public abstract class CachedTokenProvider : ITokenService, IDisposable
    {
        readonly IOptions<OAuth2Config>? _config;
        readonly ILogger<CachedTokenProvider> _logger;
        private readonly ICachableTokenProvider _tokenProvider;
        private readonly TimeSpan _retryInterval;
        private readonly TimeSpan _earlyRefresh;
        private readonly TimeSpan _earlyExpiry;
        readonly CancellationTokenSource _cts = new();
        private TokenWithExpiry? _token;
        
        /// <summary>
        /// Create a new instance of <see cref="CachedTokenProvider"/>.
        /// </summary>
        /// <param name="tokenProvider">Token provider used to fetch a new token.</param>
        /// <param name="retryInterval">Interval between retry attempts.</param>
        /// <param name="earlyRefresh">Time before the token's actual expiry when it should be refreshed.</param>
        /// <param name="earlyExpiry">Time before the token's actual expiry when it should be considered expired.</param>
        /// <param name="logger">The logger to be used for logging.</param>
        public CachedTokenProvider(ICachableTokenProvider tokenProvider, TimeSpan retryInterval, TimeSpan earlyRefresh, TimeSpan earlyExpiry, ILogger<CachedTokenProvider> logger)
        {
            _tokenProvider = tokenProvider;
            _retryInterval = retryInterval;
            _earlyRefresh = earlyRefresh;
            _earlyExpiry = earlyExpiry;
            _logger = logger;
        }

        /// <summary>
        /// Create a new instance of <see cref="CachedTokenProvider"/> with default values.
        /// </summary>
        /// <param name="config">OAuth2 configuration</param>
        /// <param name="tokenProvider">The token provider used to fetch a new token.</param>
        /// <param name="logger">The logger to be used for logging.</param>
        public CachedTokenProvider(IOptions<OAuth2Config> config, ICachableTokenProvider tokenProvider, ILogger<CachedTokenProvider> logger)
        {
            config.Value.Validate();
            _config = config;
            _tokenProvider = tokenProvider;
            _retryInterval = _config.Value.RetryInterval;
            _earlyRefresh = _config.Value.EarlyRefresh;
            _earlyExpiry = _config.Value.EarlyExpiry;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves the token and starts the token refresh loop.
        /// IMPORTANT: Before calling RequestAccessToken(), ensure to invoke StartAsync() unless you're utilizing Dependency Injection, where this process is managed by the builder.RunAsync() method.
        /// </summary>
        /// <exception cref="OAuth2TokenException"></exception>
        public async Task StartAsync()
        {
            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    _token = await FetchTokenAsync();
                    await Task.Delay(TimeSpan.FromSeconds(GetExpiryInSeconds()).Subtract(_earlyRefresh)); 
                }
            }
            catch (OAuth2TokenException e)
            {
                Dispose();
                throw new OAuth2TokenException(e.Message);
            }
        }

        private async Task<TokenWithExpiry> FetchTokenAsync()
        {
            _logger.LogInformation($"Fetching new token for service: {DateTimeOffset.UtcNow}");
            for (int retries = 0;; retries++)
            {
                if (retries > 0)
                {
                    _logger.LogWarning($"Unable to fetch OAuth token. Retrying in {retries}.");
                    await Task.Delay(_retryInterval);
                }
                
                TokenResponse? token = await FetchTokenOrNullAsync(_cts.Token);

                if (token == null) continue;
                
                if (!token.IsError)
                {
                    return new TokenWithExpiry(token.AccessToken, DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn));
                }
                
                _logger.LogWarning("Failed to retrieve token with following error message: " + token.Error);
            }
        }
        
        private async Task<TokenResponse?> FetchTokenOrNullAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await _tokenProvider.GetTokenResponse(cancellationToken);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Requests the access token with expiry synchronously.
        /// </summary>
        /// <returns>Returns the access token and the expiry of the token</returns>
        /// <exception cref="OAuth2TokenException"></exception>
        public async Task<(string AccessToken, long Expiration)> RequestAccessTokenAsync(
            CancellationToken cancellationToken
        )
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_token == null)
                {
                    _logger.LogWarning($"No token available yet. Waiting for {_retryInterval} before checking again.");
                    await Task.Delay(_retryInterval, cancellationToken);
                    continue;
                }

                if (TimeSpan.FromSeconds(GetExpiryInSeconds()).Subtract(_earlyExpiry) > TimeSpan.Zero)
                {
                    if (_token?.AccessToken == null)
                    {
                        throw new OAuth2TokenException($"Failed to retrieve access token - Access token is null");
                    }
                    return (_token.AccessToken, DateTimeOffset.UtcNow.AddSeconds(GetExpiryInSeconds()).ToUnixTimeMilliseconds());
                }
                _logger.LogWarning($"No token available yet. Waiting for {_retryInterval} before checking again");
            }
            throw new OAuth2TokenException("Cancellation of Token Requested");
        }

        private double GetExpiryInSeconds()
        {
            if (_token != null)
                return (_token.Expires - DateTimeOffset.UtcNow).TotalSeconds;
            return 0;
        }
        
        /// <summary>
        /// Disposes of the token provider and cancels the token refresh loop.
        /// </summary>
        public void Dispose()
        { 
            _cts.Dispose();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class CachedKafkaTokenProvider : CachedTokenProvider
    {
        /// <summary>
        /// 
        /// </summary>
        public CachedKafkaTokenProvider(IOptions<KafkaOAuth2Config> config, [FromKeyedServices("kafka")]ICachableTokenProvider tokenProvider, ILogger<CachedKafkaTokenProvider> logger) : base(config, tokenProvider, logger)
        {
            
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class CachedOpenSearchTokenProvider : CachedTokenProvider
    {
        /// <summary>
        /// 
        /// </summary>
        public CachedOpenSearchTokenProvider(IOptions<OpenSearchOAuth2Config> config, [FromKeyedServices("opensearch")]ICachableTokenProvider tokenProvider, ILogger<CachedOpenSearchTokenProvider> logger) : base(config, tokenProvider, logger)
        {
            
        }
    }
}
