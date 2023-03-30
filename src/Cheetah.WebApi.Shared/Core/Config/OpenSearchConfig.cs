namespace Cheetah.Shared.WebApi.Core.Config
{
    public class OpenSearchConfig
    {
        public const string Position = "OpenSearch";
        public string IndexNamePrefix { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string TokenEndpoint { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public OpenSearchAuthMode AuthMode { get; set; }

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