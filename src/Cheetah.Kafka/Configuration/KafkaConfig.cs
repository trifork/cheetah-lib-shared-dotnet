using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Cheetah.Auth.Configuration;
using Confluent.Kafka;
using Confluent.SchemaRegistry;

namespace Cheetah.Kafka.Configuration
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
        /// Bootstrap Url.
        /// </summary>
        /// <value></value>
        [Required]
        public string Url { get; set; } = null!;

        /// <summary>
        /// Optional schema registry url.
        /// </summary>
        public string? SchemaRegistryUrl { get; set; }

        /// <summary>
        /// The principal used for authentication. Defaults to <c>unused</c> and is <i>usually</i> not required.
        /// </summary>
        public string Principal { get; set; } = "unused";

        /// <summary>
        /// The security protocol used to communicate with brokers.
        /// </summary>
        public SecurityProtocol SecurityProtocol { get; set; } = SecurityProtocol.SaslPlaintext;

        /// <summary>
        /// The location of the CA certificate file used to verify the broker's certificate.
        /// </summary>
        public string SslCaLocation { get; set; } = "";

        /// <summary>
        /// The OAuth2 configuration
        /// </summary>
        public KafkaOAuth2Config OAuth2 { get; set; } = new();

        /// <summary>
        /// Validates and throws an error if the configuration is invalid.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the configuration is invalid</exception>
        public void Validate()
        {
            if (!Uri.IsWellFormedUriString(Url, UriKind.Absolute))
            {
                throw new ArgumentException($"The provided Kafka Url is invalid: {Url})");
            }
            ValidateKafkaUrlHasNoScheme();

            if (SecurityProtocol == SecurityProtocol.SaslSsl && string.IsNullOrEmpty(SslCaLocation))
            {
                throw new ArgumentException("The SslCaLocation must be set when using SecurityProtocol.SaslSsl");
            }

            OAuth2.Validate();
        }

        private void ValidateKafkaUrlHasNoScheme()
        {
            // Kafka producers and consumers will silently fail if a scheme is prepended (e.g. http://kafka:19092)
            // This ensures that we fail early and loudly if this is the case. Uses a regex that should match any prefix followed by '://'
            // We could also just strip the scheme prefix if it's there, but that would hide the fact that the input is wrong.
            var hasSchemePrefix = Regex.Match(Url, "(.*://).*");
            if (hasSchemePrefix.Success)
            {
                throw new ArgumentException(
                    $"Found Kafka address: '{Url}'. The Kafka URL cannot contain a scheme prefix - Remove the '{hasSchemePrefix.Groups[1].Value}'-prefix"
                );
            }
        }

        /// <summary>
        /// Converts the configuration to a <see cref="ClientConfig"/>/>.
        /// </summary>
        /// <returns>The converted <see cref="ClientConfig"/></returns>
        public ClientConfig GetClientConfig()
        {
            var clientConfig = new ClientConfig
            {
                BootstrapServers = Url,
                SaslMechanism = SaslMechanism.OAuthBearer,
                SecurityProtocol = SecurityProtocol,
                SslCaLocation = SslCaLocation,
            };

            return clientConfig;
        }

        /// <summary>
        /// Converts the configuration to a <see cref="SchemaRegistryConfig"/>
        /// </summary>
        /// <returns>The converted <see cref="SchemaRegistryConfig"/></returns>
        public SchemaRegistryConfig GetSchemaRegistryConfig()
        {
            if (!Uri.IsWellFormedUriString(SchemaRegistryUrl, UriKind.Absolute))
            {
                throw new ArgumentException("The provided Schema Registry Url is invalid");
            }

            return new SchemaRegistryConfig { Url = SchemaRegistryUrl };
        }
    }
}
