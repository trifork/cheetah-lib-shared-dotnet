using System.Security.Cryptography.X509Certificates;
using Cheetah.ComponentTest.Kafka;
using Cheetah.ComponentTest.OpenSearch;
using Cheetah.ComponentTest.XUnit.Model;
using Microsoft.Extensions.Configuration;

namespace Cheetah.ComponentTest.XUnit
{
    [Trait("TestType", "IntegrationTests")]
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
            Thread.Sleep(3000);
            var readMessages = reader.ReadMessages(1, TimeSpan.FromSeconds(1));
            Assert.Single(readMessages);
            Assert.True(reader.VerifyNoMoreMessages(TimeSpan.FromSeconds(1)));
        }

        [Fact]
        public async Task WriteToOsAsync()
        {
            var model = new OpenSearchTestModel()
            {
                TestString = "Test string",
                TestInteger = 1234
            };
            
            var writer = OpenSearchWriterBuilder.Create<OpenSearchTestModel>()
                .WithOpenSearchConfigurationPrefix(string.Empty, configuration)
                .WithIndex("my_index")
                .Build();

            var reader = OpenSearchReaderBuilder.Create<OpenSearchTestModel>()
                .WithOpenSearchConfigurationPrefix(string.Empty, configuration)
                .WithIndex("my_index")
                .Build();

            await writer.WriteAsync(model);
            Thread.Sleep(5000);
            var readMessages = await reader.GetMessages(10);
            Assert.Single(readMessages);
            Assert.True(reader.CountAllMessagesInIndex() == 1);
            
            //Clean up
            reader.DeleteAllMessagesInIndex();
        }
    }
}
