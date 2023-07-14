using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Cheetah.Core.Config;
using Cheetah.Core.Infrastructure.Auth;
using Cheetah.WebApi.Shared.Core.Config;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.WebApi.Shared.Infrastructure.Auth
{
    public class TimedOpenSearchTokenRefresherService : BackgroundService
    {
        private readonly ILogger<TimedOpenSearchTokenRefresherService> _logger;
        private readonly TokenService? tokenService;
        private readonly IOptions<OpenSearchConfig> openSearchConfig;

        public TimedOpenSearchTokenRefresherService(
            ILogger<TimedOpenSearchTokenRefresherService> logger,
            IMemoryCache cache,
            IHttpClientFactory httpClientFactory,
            IOptions<OpenSearchConfig> openSearchConfig
        )
        {
            this.openSearchConfig = openSearchConfig;
            _logger = logger;
            if (openSearchConfig.Value.AuthMode != OpenSearchConfig.OpenSearchAuthMode.OAuth2)
            {
                logger.LogWarning(
                    nameof(TimedOpenSearchTokenRefresherService)
                        + " was registered but OAuth2 has not been enabled"
                );
                return;
            }
            tokenService = new CheetahOpenSearchTokenService(
                logger,
                httpClientFactory,
                cache,
                openSearchConfig.Value.ClientId,
                openSearchConfig.Value.ClientSecret,
                openSearchConfig.Value.TokenEndpoint,
                openSearchConfig.Value.Scope
            );
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            // Short-circuit if tokenService is null
            if (tokenService == null)
            {
                _logger.LogWarning(
                    $"{nameof(TimedOpenSearchTokenRefresherService)} will not be running. OAuth2 has not been enabled"
                );
                return;
            }
            
            _logger.LogInformation("TimedOpenSearchTokenRefresherService running.");

            try
            {
                using PeriodicTimer timer = new(TimeSpan.FromSeconds(1));
                do // Run once, then every time the timer elapses.
                {
                    try
                    {
                        await tokenService.RequestAccessTokenCachedAsync(cancellationToken);
                    }
                    catch (OAuth2TokenException e)
                    {
                        _logger.LogWarning($"Periodic refresh of OpenSearch Auth token failed with error: '{e.Error}'");
                    }
                } while (await timer.WaitForNextTickAsync(cancellationToken));
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("TimedOpenSearchTokenRefresherService is stopping.");
            }
        }
    }
}
