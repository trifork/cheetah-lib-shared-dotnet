using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Cheetah.Auth.Authentication
{
    /// <summary>
    /// Background service responsible for starting the token service.
    /// </summary>
    public abstract class StartUpTokenService : BackgroundService
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
                await _tokenService.StartAsync();
            }
            catch (OAuth2TokenException)
            {
                _tokenService.Dispose();
            }
        }
    }

    // TODO: These should be replaced with some kind of keyedhosted service (Which does not exist in dotnet).
    // TODO: Currently, registering two instances of AddHostedService<StartUpTokenService>() results in configuration overwrite.
    // TODO: Hence, StartUpKafkaTokenService and StartUpOpenSearchTokenService were introduced to mitigate this issue. 
    /// <summary>
    /// Background service responsible for starting the kafka token service.
    /// </summary>
    public class StartUpKafkaTokenService : StartUpTokenService
    {
        /// <summary>
        /// Creates a new instance StartUpKafkaTokenService
        /// </summary>
        /// <param name="tokenService"></param>
        public StartUpKafkaTokenService(ITokenService tokenService) : base(tokenService)
        {
        }
    }

    /// <summary>
    /// Background service responsible for starting the OpenSearch token service.
    /// </summary>
    public class StartUpOpenSearchTokenService : StartUpTokenService
    {
        /// <summary>
        /// Creates a new instance StartUpOpenSearchTokenService
        /// </summary>
        /// <param name="tokenService"></param>
        public StartUpOpenSearchTokenService(ITokenService tokenService) : base(tokenService)
        {
        }
    }
    /// <summary>
    /// Background service responsible for starting the Schema-registry token service.
    /// </summary>
    public class StartUpSchemaRegistryTokenService : StartUpTokenService
    {
        /// <summary>
        /// Creates a new instance StartUpSchemaRegistryTokenService
        /// </summary>
        /// <param name="tokenService"></param>
        public StartUpSchemaRegistryTokenService(ITokenService tokenService) : base(tokenService)
        {
        }
    }
}



