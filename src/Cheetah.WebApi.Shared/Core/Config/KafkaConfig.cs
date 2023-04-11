namespace Cheetah.Shared.WebApi.Core.Config
{
    public class KafkaConfig
    {
        public const string Position = "Kafka";
        public string KafkaUrl { get; set; } = "kafka:19092";
        public string OAuthScopes { get; set; }
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string TokenEndpoint { get; set; } = "http://cheetahoauthsimulator:80/oauth2/token";
    }
}