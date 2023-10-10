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
        /// OAuth2 specific. What scope to request from TokenEndpoint
        /// </summary>
        /// <value></value>
        public string OAuthScope { get; set; } = string.Empty;

        /// <summary>
        /// Client id used to obtain JWT from token endpoint
        /// </summary>
        /// <value></value>
        public string ClientId { get; set; } = null!;

        /// <summary>
        /// Client secret used to obtain JWT from token endpoint
        /// </summary>
        /// <value></value>
        public string ClientSecret { get; set; } = null!;

        /// <summary>
        /// Token endpoint used to obtain token for authentication and authorization
        /// </summary>
        /// <value></value>
        public string TokenEndpoint { get; set; } = null!;
    }
}
