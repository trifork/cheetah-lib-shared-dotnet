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
