using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

public class ConfigureAsymmetricTokenValidationParameters : IPostConfigureOptions<JwtBearerOptions>
{
    private readonly IPublicKeyProvider _publicKeyProvider;

    public ConfigureAsymmetricTokenValidationParameters(IPublicKeyProvider publicKeyProvider)
    {
        this._publicKeyProvider = publicKeyProvider;
    }

    public void PostConfigure(string name, JwtBearerOptions options)
    {
        options.TokenValidationParameters.IssuerSigningKeyResolver = this.CreateAsymmetricResolver();
    }

    private IssuerSigningKeyResolver CreateAsymmetricResolver()
    {
        // assign variable to avoid capturing this class instance in the closure
        var publicKeyCache = this._publicKeyProvider;

        return (token, securityToken, kid, validationParameters) => publicKeyCache.GetKey(securityToken.Issuer).GetAwaiter().GetResult();
    }
}