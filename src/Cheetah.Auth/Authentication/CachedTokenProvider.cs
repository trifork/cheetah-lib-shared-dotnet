using System;
using System.Threading;
using System.Threading.Tasks;
using Cheetah.Auth.Configuration;
using IdentityModel.Client;
using Microsoft.Extensions.Logging;

namespace Cheetah.Auth.Authentication
{
    /// <summary>
    /// CachedTokenProvider manages the retrieval and caching of OAuth2 tokens, optimizing performance by reducing unnecessary token requests.
    /// Refreshing of tokens is handled in a separate thread, ensuring a consistent supply of valid tokens.
    /// IMPORTANT: Before calling RequestAccessToken(), ensure to invoke StartAsync() unless you're utilizing Dependency Injection, where this process is managed by the builder.RunAsync() method.
    /// </summary>
    public class CachedTokenProvider : ITokenService, IDisposable
    {
        readonly OAuth2Config? _config;
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
        public CachedTokenProvider(OAuth2Config config, ICachableTokenProvider tokenProvider, ILogger<CachedTokenProvider> logger)
        {
            config.Validate();
            _config = config;
            _tokenProvider = tokenProvider;
            _retryInterval = _config.RetryInterval;
            _earlyRefresh = _config.EarlyRefresh;
            _earlyExpiry = _config.EarlyExpiry;
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
            for (int retries = 0; ; retries++)
            {
                if (retries > 0)
                {
                    _logger.LogWarning($"Unable to fetch OAuth token. Retrying in {_retryInterval}.");
                    await Task.Delay(_retryInterval);
                }

                TokenResponse? token = await FetchTokenOrNullAsync(_cts.Token);

                if (token == null) continue;

                if (!token.IsError)
                {
                    return new TokenWithExpiry(token.AccessToken, DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn));
                }

                _logger.LogWarning($"Failed to retrieve token with following error message: \"{token.Error}: {token.ErrorDescription}\"");
            }
        }

        private async Task<TokenResponse?> FetchTokenOrNullAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await _tokenProvider.GetTokenResponse(cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogWarning("Token provider return null, with the following exception: " + exception.Message);
                return null;
            }
        }

        /// <summary>
        /// Requests the access token asynchronously.
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

                if (_token?.AccessToken == null)
                {
                    throw new OAuth2TokenException($"Failed to retrieve access token - Access token is null");
                }

                var aboutToExpire = TimeSpan.FromSeconds(GetExpiryInSeconds()).Subtract(_earlyExpiry) <= TimeSpan.Zero;
                if (aboutToExpire)
                {
                    _logger.LogWarning($"Token is about to expire. Requesting new token in {_retryInterval}.");
                    await Task.Delay(_retryInterval, cancellationToken);
                    continue;
                }

                return (_token.AccessToken, DateTimeOffset.UtcNow.AddSeconds(GetExpiryInSeconds()).ToUnixTimeMilliseconds());
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
}
