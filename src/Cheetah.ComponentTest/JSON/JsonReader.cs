using Microsoft.Extensions.Configuration;

namespace Cheetah.ComponentTest.JSON
{

    public class JsonConfiguration
    {
        public string ProducerTopic { get; set; }
        public string ConsumerTopic { get; set; }

    }

    public class JsonReader
    {

        protected IConfiguration? Configuration { get; }

        public JsonReader(IConfiguration configuration)
        {
            Configuration = configuration;
        }

    }
}