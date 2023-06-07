using IdentityModel.Client;

namespace Cheetah.Core.Infrastructure.Auth
{
    public interface ITokenService
    {
        Task<TokenResponse?> RequestAccessTokenCachedAsync(CancellationToken cancellationToken);
        Task<TokenResponse> RequestClientCredentialsTokenAsync(CancellationToken cancellationToken);
    }
}