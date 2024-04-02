using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace Cheetah.Auth.Authentication
{
    public interface ICachableTokenProvider
    {
        Task<TokenResponse?> GetTokenResponse(CancellationToken cancellationToken);
    }
}
