using Cheetah.ComponentTest.Kafka;
using Microsoft.Extensions.Configuration;

namespace Cheetah.ComponentTest.XUnit
{
    public class WriterTest
    {
        readonly IConfiguration configuration;

        public WriterTest()
        {
            configuration = new ConfigurationBuilder()
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

            var reader = KafkaReaderBuilder.Create<string, string>()
                .WithKafkaConfigurationPrefix(string.Empty, configuration)
                .WithTopic("MyTopic")
                .WithGroupId("Mygroup")
                .Build();

            await writer.WriteAsync("Message4");
            var readMessages = reader.ReadMessages(1, TimeSpan.FromSeconds(1));
            Assert.Single(readMessages);
            Assert.True(reader.VerifyNoMoreMessages(TimeSpan.FromSeconds(1)));
        }
    }
}
