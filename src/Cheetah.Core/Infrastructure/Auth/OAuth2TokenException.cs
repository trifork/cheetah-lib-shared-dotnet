namespace Cheetah.Core.Infrastructure.Auth;

public class OAuth2TokenException : Exception
{
    public OAuth2TokenException(string message) : base(message)
    {
    }
}
