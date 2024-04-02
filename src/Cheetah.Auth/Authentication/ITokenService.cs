using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;

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
        /// <returns>A tuple containing the access token and its absolute expiration in epoch millis </returns>

        // Developer note: It is tempting to make this return some well-named POCO instead of a tuple, but in the end we want to rely only on a standard language type
        // so that library consumers can use their own implementation without needing to reference Cheetah.Auth
        (string AccessToken, long Expiration) RequestAccessToken();
    }
}
