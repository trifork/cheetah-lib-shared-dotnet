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
        /// Request an access token asynchronously.
        /// </summary>
        /// <returns>A tuple containing the access token and its absolute expiration in epoch millis </returns>
        // Developer note: It is tempting to make this return some well-named POCO instead of a tuple, but in the end we want to rely only on a standard language type
        // so that library consumers can use their own implementation without needing to reference Cheetah.Auth
        Task<(string AccessToken, long Expiration)> RequestAccessTokenAsync(
            CancellationToken cancellationToken
        );

        /// <summary>
        /// Start the token service.
        /// IMPORTANT: Before calling RequestAccessToken(), ensure to invoke StartAsync() unless you're utilizing Dependency Injection, where this process is managed by the builder.RunAsync() method.
        /// </summary>
        /// <returns></returns>
        Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Stop the token service.
        /// </summary>
        void Dispose();
    }
}
