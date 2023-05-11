using System.ComponentModel.DataAnnotations;

namespace Cheetah.ComponentTest
{
    public class ComponentTestConfig
    {
        public const string Position = "ComponentTest";

        public string ConsumerGroup { get; set; } = "ComponentTest";

        [Required]
        public string ConsumerTopic { get; set; }

        [Required]
        public string ProducerTopic { get; set; }
    }
}
