using System;
using System.ComponentModel.DataAnnotations;

namespace Cheetah.Auth.Configuration
{
    /// <summary>
    /// Configuration for OAuth2 authentication
    /// </summary>
    public class OAuth2Config
    {
        /// <summary>
        /// The endpoint to retrieve the token from
        /// </summary>
        [Required]
        public string TokenEndpoint { get; set; } = null!;

        /// <summary>
        /// The client id to use when authenticating
        /// </summary>

        [Required]
        public string ClientId { get; set; } = null!;

        /// <summary>
        /// The client secret to use when authenticating
        /// </summary>

        [Required]
        public string ClientSecret { get; set; } = null!;

        /// <summary>
        /// Optional scope to request when authenticating
        /// </summary>
        public string? Scope { get; set; }
        
        /// <summary>
        /// The interval between retry attempts to get a token
        /// </summary>
        public TimeSpan RetryInterval { get; set; } = TimeSpan.FromSeconds(1);
        
        /// <summary>
        /// The time before the token's actual expiry when it should be refreshed.
        /// </summary>
        public TimeSpan EarlyRefresh { get; set; } = TimeSpan.FromSeconds(30);
        
        /// <summary>
        /// The time before the token's actual expiry when it should be considered expired.
        /// </summary>
        public TimeSpan EarlyExpiry { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Validates that configuration has minimum values
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public void Validate()
        {
            if (string.IsNullOrEmpty(TokenEndpoint))
            {
                throw new ArgumentNullException(nameof(TokenEndpoint));
            }

            if (string.IsNullOrEmpty(ClientId))
            {
                throw new ArgumentNullException(nameof(ClientId));
            }

            if (string.IsNullOrEmpty(ClientSecret))
            {
                throw new ArgumentNullException(nameof(ClientSecret));
            }
        }
    }
}
