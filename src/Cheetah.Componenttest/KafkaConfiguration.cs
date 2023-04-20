using System.ComponentModel.DataAnnotations;

namespace Common;

public class KafkaConfiguration
{
    public const string Position = "Kafka";

    public string ConsumerGroup { get; } = "ComponentTest";

    [Required]
    public string BootstrapServer { get; set; } = "kafka:19092";

    [Required]
    public string ConsumerTopic { get; set; } = "MappedInfoCodes";

    [Required]
    public string ProducerTopic { get; set; } = "RawMeterStates";
}