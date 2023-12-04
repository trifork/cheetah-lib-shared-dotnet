namespace Cheetah.Core.Configuration
{
    /// <summary>
    /// Configuration for OAuth2 authentication
    /// </summary>
    public class OAuth2Config
    {
        /// <summary>
        /// The endpoint to retrieve the token from
        /// </summary>
        public string TokenEndpoint { get; set; } = null!;
        
        /// <summary>
        /// The client id to use when authenticating
        /// </summary>
        public string ClientId { get; set; } = null!;
        
        /// <summary>
        /// The client secret to use when authenticating
        /// </summary>
        public string ClientSecret { get; set; } = null!;
        
        /// <summary>
        /// Optional scope to request when authenticating
        /// </summary>
        public string? AuthScope { get; set; }
    }
}
