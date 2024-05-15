using System;
using System.ComponentModel.DataAnnotations;
using Cheetah.Auth.Configuration;
using Confluent.Kafka;
using Confluent.SchemaRegistry;

namespace Cheetah.SchemaRegistry.Configuration
{
    /// <summary>
    /// SchemaConfig for IOptions
    /// </summary>
    public class SchemaConfig
    {
        /// <summary>
        /// Prefix for options e.g. SchemaRegistry__
        /// </summary>
        public const string Position = "SchemaRegistry";

        /// <summary>
        /// Bootstrap Url.
        /// </summary>
        /// <value></value>
        [Required]
        public string? Url { get; set; } = null!;

        /// <summary>
        /// The security protocol used to communicate with brokers.
        /// </summary>
        public SecurityProtocol SecurityProtocol { get; set; } = SecurityProtocol.SaslPlaintext;

        /// <summary>
        /// The OAuth2 configuration
        /// </summary>
        public OAuth2Config OAuth2 { get; set; } = null!;

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
            OAuth2.Validate();
        }

        /// <summary>
        /// Converts the configuration to a <see cref="SchemaRegistryConfig"/>
        /// </summary>
        /// <returns>The converted <see cref="SchemaRegistryConfig"/></returns>
        public SchemaRegistryConfig GetSchemaRegistryConfig()
        {
            if (!Uri.IsWellFormedUriString(Url, UriKind.Absolute))
            {
                throw new ArgumentException("The provided Schema Registry Url is invalid");
            }

            return new SchemaRegistryConfig
            {
                Url = Url
            };
        }

    }
}
