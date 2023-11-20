using System.ComponentModel.DataAnnotations;
using Cheetah.Core;

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
    }
}
