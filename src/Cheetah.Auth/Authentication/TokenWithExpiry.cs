using System;

namespace Cheetah.Auth.Authentication;

/// <summary>
/// Represents a token with an expiry time.
/// </summary>
public class TokenWithExpiry
{
    /// <summary>
    /// The access token.
    /// </summary>
    public string? AccessToken { get; set; }
    
    /// <summary>
    /// Expiry time of the token.
    /// </summary>
    public DateTimeOffset Expires { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenWithExpiry"/> class.
    /// </summary>
    /// <param name="accessToken">The access token.</param>
    /// <param name="expires">The expiry time of the token.</param>
    public TokenWithExpiry(string? accessToken, DateTimeOffset expires)
    {
        AccessToken = accessToken;
        Expires = expires;
    }
}
