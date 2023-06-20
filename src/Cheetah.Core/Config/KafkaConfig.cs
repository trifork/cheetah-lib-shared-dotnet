using System.ComponentModel.DataAnnotations;

namespace Cheetah.Core.Config
{
    /// <summary>
    /// KafkaConfig for IOptions
    /// </summary>
    public class KafkaConfig
    {
        /// <summary>
        /// Prefix for options e.g. Kafka__
        /// </summary>
        public const string Position = "Kafka";

        /// <summary>
        /// Bootstrap Url
        /// </summary>
        /// <value></value>
        [Required]
        public string Url { get; set; } = null!;

        /// <summary>
        /// OAuth2 specific. What scopes to request from TokenEndpoint
        /// </summary>
        /// <value></value>
        /// 
        public string OAuthScopes { get; set; } = string.Empty;

        /// <summary>
        /// Client id used to obtain JWT from token endpoint
        /// </summary>
        /// <value></value>
        [Required]
        public string ClientId { get; set; } = null!;
        
        /// <summary>
        /// Client secret used to obtain JWT from token endpoint
        /// </summary>
        /// <value></value>
        [Required]
        public string ClientSecret { get; set; } = null!;

        /// <summary>
        /// Token endpoint used to obtain token for authentication and authorization
        /// </summary>
        /// <value></value>
        [Required]
        public string TokenEndpoint { get; set; } = null!;
    }
}
