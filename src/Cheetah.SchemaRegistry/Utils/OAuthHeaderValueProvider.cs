using System.Net.Http.Headers;
using System.Threading;
using Cheetah.Auth.Authentication;
using Confluent.SchemaRegistry;

namespace Cheetah.SchemaRegistry.Utils
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class OAuthHeaderValueProvider : IAuthenticationHeaderValueProvider
    {
        readonly ITokenService _tokenService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokenService"></param>
        public OAuthHeaderValueProvider(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public AuthenticationHeaderValue GetAuthenticationHeader()
        {
            string? token = _tokenService.RequestAccessTokenAsync(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult().AccessToken;
            return new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
