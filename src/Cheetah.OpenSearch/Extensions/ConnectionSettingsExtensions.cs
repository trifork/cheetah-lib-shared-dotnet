using System.Security.Cryptography.X509Certificates;
using Cheetah.OpenSearch.Config;
using OpenSearch.Client;
using OpenSearch.Net;

namespace Cheetah.OpenSearch.Extensions
{
    internal static class ConnectionSettingsExtensions
    {
        internal static ConnectionSettings ConfigureBasicAuthIfEnabled(this ConnectionSettings settings, OpenSearchConfig config)
        {
            if (config.AuthMode == OpenSearchConfig.OpenSearchAuthMode.Basic)
            {
                settings = settings.BasicAuthentication(
                    config.UserName,
                    config.Password
                );
            }

            return settings;
        }

        internal static ConnectionSettings ConfigureTlsValidation(this ConnectionSettings settings, OpenSearchConfig config)
        {
            if (config.DisableTlsValidation)
            {
                settings = settings.ServerCertificateValidationCallback(CertificateValidations.AllowAll);
            }
            else if (!string.IsNullOrWhiteSpace(config.CaCertificatePath))
            {
                settings = settings.ServerCertificateValidationCallback(
                    CertificateValidations.AuthorityIsRoot(
                        new X509Certificate2(config.CaCertificatePath)
                    )
                );
            }

            return settings;
        }
    }
}
