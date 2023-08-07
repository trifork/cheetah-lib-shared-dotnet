using System.ComponentModel.DataAnnotations;
using Cheetah.Core.Infrastructure.Services.OpenSearchClient;

namespace Cheetah.Core.Config
{
    /// <summary>
    /// OpenSearchConfig for IOptions
    /// </summary>
    public class OpenSearchConfig
    {
        /// <summary>
        /// Prefix for options e.g. OpenSearch__
        /// </summary>
        public const string Position = "OpenSearch";

        /// <summary>
        /// Can be used to set prefix when querying indices
        /// </summary>
        /// <value></value>
        public string IndexNamePrefix { get; set; } = string.Empty;

        /// <summary>
        /// Url for OpenSearch
        /// </summary>
        /// <value></value>
        [Required]
        public string Url { get; set; } = null!;

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
        /// OAuth2 specific. What scope to request from TokenEndpoint
        /// </summary>
        /// <value></value>
        public string? OAuthScope { get; set; }

        /// <summary>
        /// Token endpoint used to obtain token for authentication and authorization
        /// </summary>
        /// <value></value>
        public string TokenEndpoint { get; set; } = null!;

        /// <summary>
        /// UserName for Basic Auth
        /// </summary>
        /// <value></value>
        public string UserName { get; set; } = null!;

        /// <summary>
        /// Password for Basic Auth
        /// </summary>
        /// <value></value>
        public string Password { get; set; } = null!;

        /// <summary>
        /// Authentication mode used by <see cref="CheetahOpenSearchClient"/>
        /// </summary>
        /// <value></value>

        public OpenSearchAuthMode AuthMode { get; set; } = OpenSearchAuthMode.BasicAuth;

        /// <summary>
        /// Validates and throws an error if values are not set for a given <see cref="AuthMode"/>.
        /// </summary>
        public void ValidateConfig()
        {
            switch (AuthMode)
            {
                case OpenSearchAuthMode.BasicAuth:
                    _ = UserName ?? throw new ArgumentNullException(nameof(UserName));
                    _ = Password ?? throw new ArgumentNullException(nameof(Password));
                    break;
                case OpenSearchAuthMode.OAuth2:
                    _ = ClientId ?? throw new ArgumentNullException(nameof(ClientId));
                    _ = ClientSecret ?? throw new ArgumentNullException(nameof(ClientSecret));
                    _ = TokenEndpoint ?? throw new ArgumentNullException(nameof(TokenEndpoint));
                    break;
                case OpenSearchAuthMode.None:
                    break;
                default:
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
