using System.Net.Http.Headers;
using System.Threading;
using Cheetah.Auth.Authentication;
using Confluent.SchemaRegistry;

namespace Cheetah.SchemaRegistry.Utils
{
    internal sealed class OAuthHeaderValueProvider : IAuthenticationHeaderValueProvider
    {
        readonly ITokenService _tokenService;

        internal OAuthHeaderValueProvider(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        // TODO: Move to Cheetah.Kafka
        public AuthenticationHeaderValue GetAuthenticationHeader()
        {
            string? token = _tokenService.RequestAccessTokenAsync(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult().AccessToken;
            return new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
