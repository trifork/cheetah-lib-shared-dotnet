using System;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace Cheetah.Core.Authentication
{
    public interface ITokenService
    {
        Task<(string AccessToken, long Expiration, string? PrincipalName)?> RequestAccessTokenAsync(CancellationToken cancellationToken);
    }
}
