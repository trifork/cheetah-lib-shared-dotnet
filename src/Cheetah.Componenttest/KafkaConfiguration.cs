using System.ComponentModel.DataAnnotations;

namespace Cheetah.ComponentTest
{
  public class KafkaConfiguration
  {
    public const string Position = "Kafka";

    public string ConsumerGroup { get; } = "ComponentTest";

    [Required]
    public string BootstrapServer { get; set; }

    [Required]
    public string ConsumerTopic { get; set; }

    [Required]
    public string ProducerTopic { get; set; }
  }
}