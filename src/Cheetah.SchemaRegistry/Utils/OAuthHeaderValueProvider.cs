using System.Net.Http.Headers;
using System.Threading;
using Cheetah.Auth.Authentication;
using Confluent.SchemaRegistry;

namespace Cheetah.SchemaRegistry.Utils
{
    /// <summary>
    /// IAuthenticationHeaderValueProvider to inject a bearer token from an <see cref="ITokenService"/>
    /// </summary>
    public sealed class OAuthHeaderValueProvider : IAuthenticationHeaderValueProvider
    {
        readonly ITokenService _tokenService;

        /// <summary>
        /// Creates an instance of <see cref="OAuthHeaderValueProvider"/> from an <see cref="ITokenService"/> .
        /// </summary>
        /// <param name="tokenService"></param>
        public OAuthHeaderValueProvider(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        /// <summary>
        /// Get AuthenticationHeader with a bearer token
        /// </summary>
        /// <returns>An instance of <see cref="AuthenticationHeaderValue"/> with a bearer token</returns>
        public AuthenticationHeaderValue GetAuthenticationHeader()
        {
            string? token = _tokenService.RequestAccessTokenAsync(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult().AccessToken;
            return new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
