using Cheetah.ComponentTest.Kafka;
using Cheetah.ComponentTest.OpenSearch;
using Cheetah.ComponentTest.XUnit.Model;
using Cheetah.ComponentTest.XUnit.Model.Avro;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace Cheetah.ComponentTest.XUnit
{
    [Trait("TestType", "IntegrationTests")]
    public class WriterTest
    {
        readonly IConfiguration configuration;

        static readonly AvroObjectWithEnum AvroObjWithEnum = new() { EnumType = EnumTypeAvro.EnumType1, NullableInt = null, NullableString = null };

        static readonly AdvancedAvroObject AdvancedAvroObject = new()
        {
            Id = "SpecialId",
            Name = "AvroName",
            LongNumber = 11899823748932,
            AvroObjectWithEnum = AvroObjWithEnum
        };
        
        public WriterTest()
        {
            /*Dictionary<string, string> conf = new Dictionary<string, string>()
            {
                {"KAFKA:AUTHENDPOINT", "http://localhost:1752/oauth2/token"},
                {"KAFKA:CLIENTID", "ClientId" },
                {"KAFKA:CLIENTSECRET", "testsecret" },
                {"KAFKA:URL", "http://localhost:9093"},
                {"KAFKA:SCHEMAREGISTRYURL", "http://localhost:8081/apis/ccompat/v7"}
            };*/
            configuration = new ConfigurationBuilder()
                // .AddInMemoryCollection(conf)
                .AddEnvironmentVariables()
                .Build();
        }

        [Fact]
        public void WriteToQueue()
        {
            var writer = KafkaWriterBuilder.Create<string, string>()
                .WithKafkaConfiguration(configuration)
                .WithTopic("MyTopic")
                .WithKeyFunction(message => message)
                .Build();
            var writer2 = KafkaWriterBuilder.Create<string, string>()
                .WithKafkaConfiguration(configuration)
                .WithTopic("MyTopic2")
                .WithKeyFunction(message => message)
                .Build();

            var reader = KafkaReaderBuilder.Create<string, string>()
                .WithKafkaConfigurationPrefix(configuration)
                .WithTopic("MyTopic")
                .WithGroupId("Mygroup")
                .Build();

            var reader2 = KafkaReaderBuilder.Create<string, string>()
                .WithKafkaConfigurationPrefix(configuration)
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
        public async Task WriteToOsAsync()
        {
            var model = new OpenSearchTestModel()
            {
                TestString = "Test string",
                TestInteger = 1234
            };

            var indexPattern = "my_index";

            var openSearchConnector = OpenSearchConnectorBuilder.Create()
                .WithOpenSearchConfigurationPrefix(configuration)
                .Build();

            var reader = openSearchConnector.NewReader<OpenSearchTestModel>(indexPattern);

            var writer = openSearchConnector.NewWriter<OpenSearchTestModel>(indexPattern);

            reader.DeleteAllMessagesInIndex();
            await writer.WriteAsync(indexPattern, model);
            Thread.Sleep(5000);
            var readMessages = await reader.GetMessages(1, indexPattern);
            Assert.Single(readMessages);
            Assert.True(reader.CountAllMessagesInIndex() == 1);
            
            //Clean up
            reader.DeleteAllMessagesInIndex();
        }

        [Fact]
        public void WriteAvroTest()
        {
            // Arrange
            var avroModel = new SimpleAvroObject() { Name = "foo", Number = 100 };

            var writerAvro = KafkaWriterBuilder.Create<Null, SimpleAvroObject>()
                .WithKafkaConfiguration(configuration)
                .WithTopic("MyAvroTopic")
                .UsingAvro()
                .WithKeyFunction(message => null!)
                .Build();
            
            var readerAvro = KafkaReaderBuilder.Create<Null, SimpleAvroObject>()
                .WithKafkaConfigurationPrefix(configuration)
                .WithTopic("MyAvroTopic")
                .WithGroupId("MyAvroGroup")
                .UsingAvro()
                .Build();
            
            // Act
            writerAvro.Write(avroModel);
            var readMessages = readerAvro.ReadMessages(1, TimeSpan.FromSeconds(20));
            
            // Assert
            Assert.Single(readMessages);
            Assert.True(readerAvro.VerifyNoMoreMessages(TimeSpan.FromSeconds(20)));
        }

        [Fact]
        public void WriteComplexAvroObjTest()
        {
            // Arrange
            var writerAvro = KafkaWriterBuilder.Create<Null, AdvancedAvroObject>()
                .WithKafkaConfiguration(configuration)
                .WithTopic("MyAvroComplexTopic")
                .UsingAvro()
                .WithKeyFunction(message => null!)
                .Build();
            
            var readerAvro = KafkaReaderBuilder.Create<Null, AdvancedAvroObject>()
                .WithKafkaConfigurationPrefix(configuration)
                .WithTopic("MyAvroComplexTopic")
                .WithGroupId("MyAvroComplexGroup")
                .UsingAvro()
                .Build();
            
            // Act
            writerAvro.Write(AdvancedAvroObject);
            var readMessages = readerAvro.ReadMessages(1, TimeSpan.FromSeconds(20));
            
            // Assert
            Assert.Single(readMessages);
            Assert.True(readerAvro.VerifyNoMoreMessages(TimeSpan.FromSeconds(20)));
        }

        [Fact]
        public async Task WriteAsyncComplexAvroObjTest()
        {
            // Arrange
            var writerAvro = KafkaWriterBuilder.Create<Null, AdvancedAvroObject>()
                .WithKafkaConfiguration(configuration)
                .WithTopic("MyAvroComplexTopicAsync")
                .UsingAvro()
                .WithKeyFunction(message => null!)
                .Build();
            
            var readerAvro = KafkaReaderBuilder.Create<Null, AdvancedAvroObject>()
                .WithKafkaConfigurationPrefix(configuration)
                .WithTopic("MyAvroComplexTopicAsync")
                .WithGroupId("MyAvroComplexGroupAsync")
                .UsingAvro()
                .Build();
            
            // Act
            await writerAvro.WriteAsync(AdvancedAvroObject);
            var readMessages = readerAvro.ReadMessages(1, TimeSpan.FromSeconds(20));
            
            // Assert
            Assert.Single(readMessages);
            Assert.True(readerAvro.VerifyNoMoreMessages(TimeSpan.FromSeconds(20)));
        }
    }
}
