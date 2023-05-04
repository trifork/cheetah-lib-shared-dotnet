namespace Cheetah.WebApi.Shared.Core.Config
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
        public string KafkaUrl { get; set; } = "kafka:19092";

        /// <summary>
        /// OAuth2 specific. What scopes to request from TokenEndpoint
        /// </summary>
        /// <value></value>
        public string OAuthScopes { get; set; }

        /// <summary>
        /// Client id used to obtain JWT from token endpoint
        /// </summary>
        /// <value></value>
        public string ClientId { get; set; } = string.Empty;
        /// <summary>
        /// Client secret used to obtain JWT from token endpoint
        /// </summary>
        /// <value></value>
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// Token endpoint used to obtain token for authentication and authorization
        /// </summary>
        /// <value></value>
        public string TokenEndpoint { get; set; } = "http://cheetahoauthsimulator:80/oauth2/token";
    }
}
