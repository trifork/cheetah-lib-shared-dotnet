using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Cheetah.WebApi.Shared.Core
{
    public interface IPublicKeyProvider
    {
        Task<JsonWebKey[]> GetKey(string clientId);
    }
}
