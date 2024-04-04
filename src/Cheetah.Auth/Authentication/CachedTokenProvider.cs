using System;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.Logging;

namespace Cheetah.Auth.Authentication
{
    public class CachedTokenProvider : ITokenService, IAsyncDisposable
    {
        readonly ILogger<CachedTokenProvider> _logger;
        private static readonly TimeSpan DEFAULT_RETRY_INTERVAL = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan DEFAULT_EARLY_REFRESH = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan DEFAULT_EARLY_EXPIRY = TimeSpan.FromSeconds(5);
        private readonly ICachableTokenProvider _tokenProvider;
        private readonly TimeSpan _retryInterval;
        private readonly TimeSpan _earlyRefresh;
        private readonly TimeSpan _earlyExpiry;
        private PeriodicTimer? _timer;
        readonly CancellationTokenSource _cts = new();
        private Task? _timerTask;
        private TokenResponse? _token;

        public CachedTokenProvider(ICachableTokenProvider tokenProvider, TimeSpan retryInterval, TimeSpan earlyRefresh, TimeSpan earlyExpiry, ILogger<CachedTokenProvider> logger)
        {
            _tokenProvider = tokenProvider;
            _retryInterval = retryInterval;
            _earlyRefresh = earlyRefresh;
            _earlyExpiry = earlyExpiry;
            _logger = logger;
            
            this.StartFetchToken();
        }

        public CachedTokenProvider(ICachableTokenProvider tokenProvider, ILogger<CachedTokenProvider> logger)
        {
            _tokenProvider = tokenProvider;
            _retryInterval = DEFAULT_RETRY_INTERVAL;
            _earlyRefresh = DEFAULT_EARLY_REFRESH;
            _earlyExpiry = DEFAULT_EARLY_EXPIRY;
            _logger = logger;

            this.StartFetchToken();
        }
        
        private void StartFetchToken()
        {
            try
            {
                _token = FetchToken().Result;
                
                if (_token == null)
                    throw new OAuth2TokenException("Failed to retrieve token returned null");
                
                if (_token.IsError)
                    throw new OAuth2TokenException(_token.ErrorDescription);
                
                _timer = new PeriodicTimer(TimeSpan.FromSeconds(_token.ExpiresIn).Subtract(_earlyRefresh));
                _timerTask = FetchTokenAsync();    
            }
            catch (OAuth2TokenException e)
            {
                throw new OAuth2TokenException(e.Message);
            }
            
        }
        
        private async Task FetchTokenAsync()
        {
            try
            {
                while (_timer != null && !_cts.IsCancellationRequested && await _timer.WaitForNextTickAsync(_cts.Token))
                {
                    _token = await FetchToken();
                }
            }
            catch (OAuth2TokenException e)
            {
                await DisposeAsync();
                throw new OAuth2TokenException(e.Message);
            }
        }

        private async Task<TokenResponse> FetchToken()
        {
            _logger.LogDebug("Fetching new token.");
            for (int retries = 0;; retries++)
            {
                if (retries > 0)
                {
                    _logger.LogWarning($"Unable to fetch OAuth token. Retrying in {retries}");
                    TrySleep(_retryInterval);
                }
        
                TokenResponse? token = await FetchTokenOrNullAsync(_cts.Token);
                
                if (token == null) continue;
                
                if (token.IsError)
                    _logger.LogWarning("Failed to retrieve token with following error message: " + token.ErrorDescription);
                
                return token;
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
        
        private static void TrySleep(TimeSpan duration)
        {
            try
            {
                Thread.Sleep(duration);
            }
            catch (ThreadInterruptedException e)
            {
                Thread.CurrentThread.Interrupt();
                throw new OAuth2TokenException(e.Message);
            }
        }

        public async ValueTask DisposeAsync()
        {
            _timer?.Dispose();
            _cts.Cancel();
            if (_timerTask != null) await _timerTask;
        }

        public (string, long) RequestAccessToken()
        {
            while (true)
            {
                if (_token == null)
                {
                    TrySleep(_retryInterval);
                    _logger.LogWarning($"No token available yet. Waiting for {_retryInterval} before checking again");
                    continue;
                }

                if (TimeSpan.FromSeconds(_token.ExpiresIn).Subtract(_earlyExpiry) > TimeSpan.Zero)
                {
                    if (_token == null || _token.IsError || _token.AccessToken == null)
                    {
                        throw new OAuth2TokenException($"Failed to retrieve access token Error: {_token?.Error}");
                    }

                    return (_token.AccessToken, DateTimeOffset.UtcNow.AddSeconds(_token.ExpiresIn).ToUnixTimeMilliseconds());
                }

                _logger.LogWarning($"No token available yet. Waiting for {_retryInterval} before checking again");
            }
        }
    }
}
