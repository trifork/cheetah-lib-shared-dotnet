using System.Threading;
using System.Threading.Tasks;

namespace Cheetah.Core.Authentication
{
    /// <summary>
    /// Service for retrieving OAuth2 access tokens
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Asynchronously request an access token
        /// </summary>
        /// <param name="cancellationToken">Cancellation used to cancel the operation</param>
        /// <returns>A tuple containing the access token, absolute expiration in epoch millis and optionally a principal name to use for the token</returns>
        Task<(string AccessToken, long Expiration, string? PrincipalName)?> RequestAccessTokenAsync(CancellationToken cancellationToken);
    }
}
