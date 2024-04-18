using System;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace Cheetah.Auth.Authentication
{
    /// <summary>
    /// Interface for a token provider that returns a token response.
    /// </summary>
    public interface ICachableTokenProvider
    {
        /// <summary>
        /// Get a token response.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="serviceName"></param>
        /// <returns>TokenResponse</returns>
        Task<TokenResponse?> GetTokenResponse(Guid serviceName, CancellationToken cancellationToken);
    }
}
