using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cheetah.Auth.Authentication
{
    /// <summary>
    /// Background service responsible for starting the token service.
    /// </summary>
    public class StartUpTokenService : BackgroundService
    {
        readonly ITokenService _tokenService;

        /// <summary>
        /// Creates a new instance of <see cref="StartUpTokenService"/>.
        /// </summary>
        /// <param name="tokenService"></param>
        public StartUpTokenService(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        /// <summary>
        /// Starts the token service asynchronously.
        /// </summary>
        /// <param name="stoppingToken">The token that can be used to request cancellation of the background operation</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();
            try
            {
                await _tokenService.StartAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // cancelled - this is expected during shutdown
            }
        }
    }
}
