using Cheetah.Shared.WebApi.Core.Config;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.WebApi.Shared.Infrastructure.Auth
{
    public class TimedOpenSearchTokenRefresherService : BackgroundService
    {
        private readonly ILogger<TimedOpenSearchTokenRefresherService> _logger;
        private readonly TokenService tokenService;

        public TimedOpenSearchTokenRefresherService(ILogger<TimedOpenSearchTokenRefresherService> logger, IMemoryCache cache, IHttpClientFactory httpClientFactory, IOptions<OpenSearchConfig> openSearchConfig)
        {
            _logger = logger;
            this.tokenService = new CheetahOpenSearchTokenService(logger, httpClientFactory, cache, openSearchConfig.Value.ClientId, openSearchConfig.Value.ClientSecret, openSearchConfig.Value.TokenEndpoint);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("TimedOpenSearchTokenRefresherService running.");

            // When the timer should have no due-time, then do the work once now.
            await tokenService.RequestAccessTokenCachedAsync(cancellationToken);

            using PeriodicTimer timer = new(TimeSpan.FromSeconds(1));

            try
            {
                while (await timer.WaitForNextTickAsync(cancellationToken))
                {
                    await tokenService.RequestAccessTokenCachedAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("TimedOpenSearchTokenRefresherService is stopping.");
            }
        }
    }
}