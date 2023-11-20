using System;
using System.ComponentModel.DataAnnotations;
using Cheetah.Core;

namespace Cheetah.OpenSearch.Config
{
    /// <summary>
    /// OpenSearchConfig for IOptions
    /// </summary>
    public class OpenSearchConfig : OAuth2Config
    {
        /// <summary>
        /// Prefix for options e.g. OpenSearch__
        /// </summary>
        public const string Position = "OpenSearch";

        /// <summary>
        /// Url for OpenSearch
        /// </summary>
        [Required]
        public string Url { get; set; } = null!;

        /// <summary>
        /// UserName for Basic Auth
        /// </summary>
        public string UserName { get; set; } = null!;

        /// <summary>
        /// Password for Basic Auth
        /// </summary>
        public string Password { get; set; } = null!;

        /// <summary>
        /// Authentication mode used by the OpenSearchClient
        /// </summary>

        public OpenSearchAuthMode AuthMode { get; set; } = OpenSearchAuthMode.Basic;

        /// <summary>
        /// Disables TLS validation for OpenSearch
        /// </summary>
        public bool DisableTlsValidation { get; set; }

        /// <summary>
        /// Path to CA certificate used to validate OpenSearch certificate
        /// </summary>
        public string? CaCertificatePath { get; set; }

        /// <summary>
        /// Validates and throws an error if values are not set for a given <see cref="AuthMode"/>.
        /// </summary>
        public void ValidateConfig()
        {
            switch (AuthMode)
            {
                case OpenSearchAuthMode.Basic:
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
                    throw new ArgumentOutOfRangeException();
            }
        }

        public enum OpenSearchAuthMode
        {
            None,
            Basic,
            OAuth2
        }
    }
}
