using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Cheetah.Auth.Authentication;

public class InitService : IHostedService
{
    ITokenService _tokenService;
    
    public InitService(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.Yield();
        try
        {
            await _tokenService.FetchTokenAsync();
        }
        catch (OAuth2TokenException)
        {
            await _tokenService.DisposeAsync();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _tokenService.DisposeAsync();
    }
}
