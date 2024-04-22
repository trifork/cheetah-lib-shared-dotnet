using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cheetah.Auth.Authentication;

/// <summary>
/// Background service responsible for starting the token service.
/// </summary>
public abstract class StartUpTokenService : BackgroundService
{
    readonly internal ITokenService _tokenService;
    
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

/// <summary>
/// 
/// </summary>
public class StartKafkaTokenService : StartUpTokenService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tokenService"></param>
    public StartKafkaTokenService([FromKeyedServices("kafka")]ITokenService tokenService) : base(tokenService)
    { }
}

/// <summary>
/// 
/// </summary>
public class StartOpenSearchTokenService : StartUpTokenService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tokenService"></param>
    public StartOpenSearchTokenService([FromKeyedServices("opensearch")]ITokenService tokenService) : base(tokenService)
    { }
}
