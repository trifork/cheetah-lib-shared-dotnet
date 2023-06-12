using Cheetah.ComponentTest.Kafka;
using Microsoft.Extensions.Configuration;

namespace Cheetah.ComponentTest.XUnit
{
    [Trait("TestType", "IntegrationTests")]
    public class WriterTest
    {
        readonly IConfiguration configuration;

        public WriterTest()
        {
            /*Dictionary<string, string> conf = new Dictionary<string, string>()
            {
                {"KAFKA:AUTHENDPOINT", "http://localhost:1752/oauth2/token"},
                {"KAFKA:CLIENTID", "ClientId" },
                {"KAFKA:SECRET", "1234" },
                {"KAFKA:URL", "localhost:9092"}
            };*/
            configuration = new ConfigurationBuilder()
                //.AddInMemoryCollection(conf)
                .AddEnvironmentVariables()
                .Build();
        }

        [Fact]
        public async Task WriteToQueueAsync()
        {
            var writer = KafkaWriterBuilder.Create<string, string>()
                .WithKafkaConfigurationPrefix(string.Empty, configuration)
                .WithTopic("MyTopic")
                .WithKeyFunction(message => message)
                .Build();

            var reader = await KafkaReaderBuilder.Create<string, string>()
                .WithKafkaConfigurationPrefix(string.Empty, configuration)
                .WithTopic("MyTopic")
                .WithGroupId("Mygroup")
                .BuildAsync();

            writer.Write("Message4");
            var readMessages = reader.ReadMessages(1, TimeSpan.FromSeconds(20));
            Assert.Single(readMessages);
            Assert.True(reader.VerifyNoMoreMessages(TimeSpan.FromSeconds(3)));
        }
    }
}
