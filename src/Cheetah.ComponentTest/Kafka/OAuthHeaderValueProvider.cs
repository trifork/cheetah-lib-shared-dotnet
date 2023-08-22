using System.Net.Http.Headers;
using System.Threading;
using Cheetah.Core.Infrastructure.Auth;
using Confluent.SchemaRegistry;

namespace Cheetah.ComponentTest.Kafka;

public class OAuthHeaderValueProvider : IAuthenticationHeaderValueProvider
{
    readonly ITokenService _tokenService;

    public OAuthHeaderValueProvider(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public AuthenticationHeaderValue GetAuthenticationHeader()
    {
        string? token = _tokenService.RequestAccessTokenCachedAsync(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult()?.AccessToken;
        return new AuthenticationHeaderValue("Bearer",token);
    }
}
