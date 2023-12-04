using System.ComponentModel.DataAnnotations;
using Cheetah.Core;
using Cheetah.Core.Configuration;
using Confluent.Kafka;

namespace Cheetah.Kafka.Config
{
    /// <summary>
    /// KafkaConfig for IOptions
    /// </summary>
    public class KafkaConfig : OAuth2Config
    {
        /// <summary>
        /// Prefix for options e.g. Kafka__
        /// </summary>
        public const string Position = "Kafka";

        /// <summary>
        /// Bootstrap Url
        /// </summary>
        /// <value></value>
        [Required]
        public string Url { get; set; } = null!;

        public SecurityProtocol SecurityProtocol { get; set; } = SecurityProtocol.SaslPlaintext;

        public ProducerConfig ToProducerConfig()
        {
            return new ProducerConfig
            {
                BootstrapServers = Url,
                SaslMechanism = SaslMechanism.OAuthBearer,
                SecurityProtocol = SecurityProtocol,
            };
        }
        
        public ConsumerConfig ToConsumerConfig()
        {
            return new ConsumerConfig
            {
                BootstrapServers = Url,
                SaslMechanism = SaslMechanism.OAuthBearer,
                SecurityProtocol = SecurityProtocol,
            };
        }
    }
}
