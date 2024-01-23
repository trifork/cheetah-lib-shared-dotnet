using System;
using System.ComponentModel.DataAnnotations;
using Cheetah.Auth.Configuration;

namespace Cheetah.OpenSearch.Configuration
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
        /// The config to use when authenticating with OAuth2
        /// </summary>
        public OAuth2Config OAuth2 { get; set; } = null!;

        /// <summary>
        /// Validates and throws an error if values are not set for a given <see cref="AuthMode"/>.
        /// </summary>
        public void Validate()
        {
            _ = string.IsNullOrWhiteSpace(Url) ? throw new ArgumentNullException(nameof(Url)) : 0;
            switch (AuthMode)
            {
                case OpenSearchAuthMode.Basic:
                    _ = string.IsNullOrWhiteSpace(UserName) ? throw new ArgumentNullException(nameof(UserName)) : 0;
                    _ = string.IsNullOrWhiteSpace(Password) ? throw new ArgumentNullException(nameof(Password)) : 0;
                    break;
                case OpenSearchAuthMode.OAuth2:
                    _ = OAuth2 ?? throw new ArgumentNullException(nameof(OAuth2));
                    OAuth2.Validate();
                    break;
                case OpenSearchAuthMode.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Authentication method to use when connecting to OpenSearch
        /// </summary>
        public enum OpenSearchAuthMode
        {
            /// <summary>
            /// No authentication
            /// </summary>
            None,
            /// <summary>
            /// Basic username/password authentication
            /// </summary>
            Basic,
            /// <summary>
            /// OAuth2 token authentication
            /// </summary>
            OAuth2
        }
    }
}
