using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

public interface IPublicKeyProvider
{
    Task<JsonWebKey[]> GetKey(string clientId);
}