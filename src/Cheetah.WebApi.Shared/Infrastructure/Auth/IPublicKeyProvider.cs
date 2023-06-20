using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Cheetah.WebApi.Shared.Infrastructure.Auth
{
    public interface IPublicKeyProvider
    {
        Task<JsonWebKey[]> GetKey(string clientId);
    }
}
