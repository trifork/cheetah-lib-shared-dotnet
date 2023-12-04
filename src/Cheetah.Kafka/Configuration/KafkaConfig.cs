using System.ComponentModel.DataAnnotations;
using Cheetah.Core.Configuration;
using Confluent.Kafka;

namespace Cheetah.Kafka.Configuration
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

        /// <summary>
        /// The security protocol used to communicate with brokers.
        /// </summary>
        public SecurityProtocol SecurityProtocol { get; set; } = SecurityProtocol.SaslPlaintext;

        /// <summary>
        /// Converts the configuration to a <see cref="ProducerConfig"/> for use with <see cref="ProducerBuilder{TKey,TValue}"/>.
        /// </summary>
        /// <returns>The converted <see cref="ProducerConfig"/></returns>
        public ProducerConfig ToProducerConfig()
        {
            return new ProducerConfig
            {
                BootstrapServers = Url,
                SaslMechanism = SaslMechanism.OAuthBearer,
                SecurityProtocol = SecurityProtocol,
            };
        }
        
        /// <summary>
        /// Converts the configuration to a <see cref="ConsumerConfig"/> for use with <see cref="ConsumerBuilder{TKey,TValue}"/>.
        /// </summary>
        /// <returns>The converted <see cref="ConsumerConfig"/></returns>
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
