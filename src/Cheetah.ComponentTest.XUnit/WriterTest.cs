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
        public void WriteToQueue()
        {
            var writer = KafkaWriterBuilder.Create<string, string>()
                .WithKafkaConfigurationPrefix(string.Empty, configuration)
                .WithTopic("MyTopic")
                .WithKeyFunction(message => message)
                .Build();
            var writer2 = KafkaWriterBuilder.Create<string, string>()
                .WithKafkaConfigurationPrefix(string.Empty, configuration)
                .WithTopic("MyTopic2")
                .WithKeyFunction(message => message)
                .Build();

            var reader = KafkaReaderBuilder.Create<string, string>()
                .WithKafkaConfigurationPrefix(string.Empty, configuration)
                .WithTopic("MyTopic")
                .WithGroupId("Mygroup")
                .Build();

            var reader2 = KafkaReaderBuilder.Create<string, string>()
                .WithKafkaConfigurationPrefix(string.Empty, configuration)
                .WithTopic("MyTopic2")
                .WithGroupId("Mygroup2")
                .Build();

            writer.Write("Message4");
            writer2.Write("Message4");
            var readMessages = reader.ReadMessages(1, TimeSpan.FromSeconds(20));
            Assert.Single(readMessages);
            Assert.True(reader.VerifyNoMoreMessages(TimeSpan.FromSeconds(20)));
            var readMessages2 = reader2.ReadMessages(1, TimeSpan.FromSeconds(20));
            Assert.Single(readMessages2);
            Assert.True(reader2.VerifyNoMoreMessages(TimeSpan.FromSeconds(20)));
        }

        [Fact]
        public void WriteToOsAsync()
        {
            var documents = new List<OpenSearchTestModel>()
            {
                new OpenSearchTestModel("Document 1", 2),
                new OpenSearchTestModel("Document 2", 3),
                new OpenSearchTestModel("Document 3", 4),
                new OpenSearchTestModel("Document 4", 5),
            };

            var opensearchClient = OpenSearchClientBuilder
                .Create()
                .WithOpenSearchConfigurationPrefix(configuration)
                .Build();

            opensearchClient.Index("test-index", documents);
            Thread.Sleep(2000);
            Assert.Equal(documents.Count, opensearchClient.Count("test*"));
            Assert.All(opensearchClient.Search<OpenSearchTestModel>("test*"), d => documents.Contains(d.Source));

            opensearchClient.DeleteIndex("test*");
            Thread.Sleep(2000);
            Assert.Equal(0, opensearchClient.Count("test*"));
        }
    }
}
