using System;

namespace Cheetah.Auth.Authentication;

public class TokenWithExpiry
{
    public string? AccessToken { get; set; }
    public DateTimeOffset Expires { get; set; }

    public TokenWithExpiry(string? accessToken, DateTimeOffset expires)
    {
        AccessToken = accessToken;
        Expires = expires;
    }
    
}
