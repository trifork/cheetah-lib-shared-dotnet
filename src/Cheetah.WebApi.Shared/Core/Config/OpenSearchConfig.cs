namespace Cheetah.Shared.WebApi.Core.Config
{
    public class OpenSearchConfig
    {
        public const string Position = "OpenSearch";
        public string IndexNamePrefix { get; set; } = string.Empty;
        public string Url { get; set; } = "http://opensearch:9200";
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string TokenEndpoint { get; set; } = "http://cheetahoauthsimulator:80/oauth2/token";
        public string UserName { get; set; } = "admin";
        public string Password { get; set; } = "admin";

        public OpenSearchAuthMode AuthMode { get; set; } = OpenSearchAuthMode.BasicAuth;

        public void ValidateConfig()
        {
            switch (AuthMode)
            {
                case OpenSearchAuthMode.BasicAuth:
                    _ = ClientId ?? throw new ArgumentNullException(nameof(UserName));
                    _ = ClientSecret ?? throw new ArgumentNullException(nameof(Password));
                    break;
                case OpenSearchAuthMode.OAuth2:
                    _ = ClientId ?? throw new ArgumentNullException(nameof(ClientId));
                    _ = ClientSecret ?? throw new ArgumentNullException(nameof(ClientSecret));
                    _ = TokenEndpoint ?? throw new ArgumentNullException(nameof(TokenEndpoint));
                    break;

            }
        }

        public enum OpenSearchAuthMode
        {
            None,
            BasicAuth,
            OAuth2
        }
    }
}