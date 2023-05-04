using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Cheetah.WebApi.Shared.Core.Configure
{
    public class ConfigureAsymmetricTokenValidationParameters
        : IPostConfigureOptions<JwtBearerOptions>
    {
        private readonly IPublicKeyProvider _publicKeyProvider;

        public ConfigureAsymmetricTokenValidationParameters(IPublicKeyProvider publicKeyProvider)
        {
            _publicKeyProvider = publicKeyProvider;
        }

        public void PostConfigure(string name, JwtBearerOptions options)
        {
            options.TokenValidationParameters.IssuerSigningKeyResolver = CreateAsymmetricResolver();
        }

        private IssuerSigningKeyResolver CreateAsymmetricResolver()
        {
            // assign variable to avoid capturing this class instance in the closure
            var publicKeyCache = _publicKeyProvider;

            return (token, securityToken, kid, validationParameters) =>
                publicKeyCache.GetKey(securityToken.Issuer).GetAwaiter().GetResult();
        }
    }
}
