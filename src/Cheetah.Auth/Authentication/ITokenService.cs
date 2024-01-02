using System.Threading;
using System.Threading.Tasks;

namespace Cheetah.Auth.Authentication
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
        
        // Developer note: It is tempting to make this return some well-named POCO instead of a tuple, but in the end we want to rely only on a standard language type
        // so that library consumers can use their own implementation without needing to reference Cheetah.Auth
        Task<(string AccessToken, long Expiration)> RequestAccessTokenAsync(CancellationToken cancellationToken);
    }
}
