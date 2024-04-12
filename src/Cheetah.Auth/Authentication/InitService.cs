using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Cheetah.Auth.Authentication;

public class InitService : BackgroundService
{
    ITokenService _tokenService;
    
    public InitService(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
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
}
