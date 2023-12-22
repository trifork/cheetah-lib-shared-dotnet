using System;
using System.ComponentModel.DataAnnotations;
using Cheetah.Auth.Configuration;
using Confluent.Kafka;

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
        /// The security protocol used to communicate with brokers.
        /// </summary>
        public SecurityProtocol SecurityProtocol { get; set; } = SecurityProtocol.SaslPlaintext;
        
        /// <summary>
        /// The OAuth2 configuration
        /// </summary>
        public OAuth2Config OAuth2 { get; set; } = null!;

        /// <summary>
        /// Converts the configuration to a <see cref="ClientConfig"/>/>.
        /// </summary>
        /// <returns>The converted <see cref="ClientConfig"/></returns>
        public ClientConfig GetClientConfig()
        {
            return new ClientConfig
            {
                BootstrapServers = Url,
                SaslMechanism = SaslMechanism.OAuthBearer,
                SecurityProtocol = SecurityProtocol,
            };
        }
    }
}
