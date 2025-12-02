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
    public partial class CachedTokenProvider : ITokenService
    {
        readonly OAuth2Config? _config;
        readonly ILogger<CachedTokenProvider> _logger;

        // LoggerMessage source generators for high-performance logging
        [LoggerMessage(Level = LogLevel.Information, Message = "Fetching new token for service: {CurrentTime}")]
        private static partial void LogFetchingToken(ILogger logger, DateTimeOffset currentTime);

        [LoggerMessage(Level = LogLevel.Warning, Message = "Unable to fetch OAuth token. Retrying in {RetryInterval}.")]
        private static partial void LogRetryingTokenFetch(ILogger logger, TimeSpan retryInterval);

        [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to retrieve token with following error message: \"{Error}: {ErrorDescription}\"")]
        private static partial void LogTokenRetrievalError(ILogger logger, string? error, string? errorDescription);

        [LoggerMessage(Level = LogLevel.Warning, Message = "No token available yet. Waiting for {RetryInterval} before checking again.")]
        private static partial void LogNoTokenAvailable(ILogger logger, TimeSpan retryInterval);

        [LoggerMessage(Level = LogLevel.Warning, Message = "Token is about to expire. Requesting new token in {RetryInterval}.")]
        private static partial void LogTokenAboutToExpire(ILogger logger, TimeSpan retryInterval);
        private readonly ICachableTokenProvider _tokenProvider;
        private readonly TimeSpan _retryInterval;
        private readonly TimeSpan _earlyRefresh;
        private readonly TimeSpan _earlyExpiry;
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
        /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token.</exception>
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            while (true)
            {
                _token = await FetchTokenAsync(cancellationToken);
                await Task.Delay(TimeSpan.FromSeconds(GetExpiryInSeconds()).Subtract(_earlyRefresh), cancellationToken);
            }
        }


        /// <summary>
        /// Retrieves the token and starts the token refresh loop.
        /// IMPORTANT: Before calling RequestAccessToken(), ensure to invoke StartAsync() unless you're utilizing Dependency Injection, where this process is managed by the builder.RunAsync() method.
        /// </summary>
        /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token.</exception>
        public Task StartAsync()
        {
            return StartAsync(CancellationToken.None);
        }

        private async Task<TokenWithExpiry> FetchTokenAsync(CancellationToken cancellationToken)
        {
            LogFetchingToken(_logger, DateTimeOffset.UtcNow);
            for (int retries = 0; ; retries++)
            {
                if (retries > 0)
                {
                    LogRetryingTokenFetch(_logger, _retryInterval);
                    await Task.Delay(_retryInterval, cancellationToken);
                }

                TokenResponse? token = await FetchTokenOrNullAsync(cancellationToken);

                if (token == null) continue;

                if (!token.IsError)
                {
                    return new TokenWithExpiry(token.AccessToken, DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn));
                }

                LogTokenRetrievalError(_logger, token.Error, token.ErrorDescription);
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
        /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token.</exception>
        public async Task<(string AccessToken, long Expiration)> RequestAccessTokenAsync(
            CancellationToken cancellationToken
        )
        {
            while (true)
            {
                if (_token == null)
                {
                    LogNoTokenAvailable(_logger, _retryInterval);
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
                    LogTokenAboutToExpire(_logger, _retryInterval);
                    await Task.Delay(_retryInterval, cancellationToken);
                    continue;
                }

                return (_token.AccessToken, DateTimeOffset.UtcNow.AddSeconds(GetExpiryInSeconds()).ToUnixTimeMilliseconds());
            }
        }

        private double GetExpiryInSeconds()
        {
            if (_token != null)
                return (_token.Expires - DateTimeOffset.UtcNow).TotalSeconds;
            return 0;
        }
    }
}
