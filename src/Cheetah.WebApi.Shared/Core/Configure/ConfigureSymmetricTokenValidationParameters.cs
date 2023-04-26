using System.Collections.Generic;
using System.Text;
using Cheetah.WebApi.Shared.Core.Config;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Cheetah.WebApi.Shared.Core.Configure
{
  public class ConfigureSymmetricTokenValidationParameters : IPostConfigureOptions<JwtBearerOptions>
  {
    private readonly IOptions<OAuthConfig> _oauthConfig;

    public ConfigureSymmetricTokenValidationParameters(IOptions<OAuthConfig> oauthConfig)
    {
      _oauthConfig = oauthConfig;
    }
    public void PostConfigure(string name, JwtBearerOptions options)
    {
      options.TokenValidationParameters.IssuerSigningKeyResolver = CreateSymmetricKeyResolver();
    }

    private IssuerSigningKeyResolver CreateSymmetricKeyResolver()
    {
      var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_oauthConfig.Value.SymmetricPrivateKey));
      var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature);
      return (token, securityToken, kid, validationParameters) => new List<SecurityKey>() { signingCredentials.Key };
    }
  }
}