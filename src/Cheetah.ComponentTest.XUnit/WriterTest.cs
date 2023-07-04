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

        static readonly AvroObjectWithEnum AvroObjWithEnum1 = new() { EnumType = EnumTypeAvro.EnumType1, NullableInt = null, NullableString = null };

        static readonly AdvancedAvroObject AdvancedAvroObject1 = new()
        {
            Id = "Id",
            Name = "AvroName",
            LongNumber = 11899823748932,
            AvroObjectWithEnum = AvroObjWithEnum1
        };
        
        static readonly AvroObjectWithEnum AvroObjWithEnum2 = new() { EnumType = EnumTypeAvro.EnumType2, NullableInt = 123, NullableString = "bar" };

        static readonly AdvancedAvroObject AdvancedAvroObject2 = new()
        {
            Id = "Id",
            Name = "Foo",
            LongNumber = 345342523454,
            AvroObjectWithEnum = AvroObjWithEnum2
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
            var writer = KafkaWriterBuilder.Create<string, string>(configuration)
                .WithTopic("MyTopic")
                .WithKeyFunction(message => message)
                .Build();
            var writer2 = KafkaWriterBuilder.Create<string, string>(configuration)
                .WithTopic("MyTopic2")
                .WithKeyFunction(message => message)
                .Build();

            var reader = KafkaReaderBuilder.Create<string, string>(configuration)
                .WithTopic("MyTopic")
                .WithGroupId("Mygroup")
                .Build();

            var reader2 = KafkaReaderBuilder.Create<string, string>(configuration)
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
            var indexName = "test-index";

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

            // the Index call takes around 365ms and the DeleteIndex calls take around 165ms so they seem to be running synchronously
            // which means we can use them without Thread.Sleep which is great
            opensearchClient.DeleteIndex(indexName);
            Assert.Equal(0, opensearchClient.Count(indexName));

            opensearchClient.Index(indexName, documents);
            opensearchClient.RefreshIndex(indexName);
            Assert.Equal(documents.Count, opensearchClient.Count(indexName));
            Assert.All(opensearchClient.Search<OpenSearchTestModel>(indexName), d => documents.Contains(d.Source));

            opensearchClient.DeleteIndex(indexName);
            opensearchClient.RefreshIndex(indexName);
            Assert.Equal(0, opensearchClient.Count(indexName));
        }

        [Fact]
        public void WriteAvroTest()
        {
            // Arrange
            var avroModel = new SimpleAvroObject() { Name = "foo", Number = 100 };

            var writerAvro = KafkaWriterBuilder.Create<Null, SimpleAvroObject>(configuration)
                .WithTopic("MyAvroTopic")
                .UsingAvro()
                .WithKeyFunction(message => null!)
                .Build();
            
            var readerAvro = KafkaReaderBuilder.Create<Null, SimpleAvroObject>(configuration)
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
            var writerAvro = KafkaWriterBuilder.Create<Null, AdvancedAvroObject>(configuration)
                .WithTopic("AvroAdvancedTopic")
                .UsingAvro()
                .WithKeyFunction(message => null!)
                .Build();
            
            var readerAvro = KafkaReaderBuilder.Create<Null, AdvancedAvroObject>(configuration)
                .WithTopic("AvroAdvancedTopic")
                .WithGroupId("AvroAdvancedGroup")
                .UsingAvro()
                .Build();
            
            // Act
            writerAvro.Write(AdvancedAvroObject1);
            var readMessages = readerAvro.ReadMessages(1, TimeSpan.FromSeconds(20));
            
            // Assert
            Assert.Single(readMessages);
            Assert.True(readerAvro.VerifyNoMoreMessages(TimeSpan.FromSeconds(20)));
        }

        [Fact]
        public async Task WriteAdvancedAvroObjAsyncTest()
        {
            // Arrange
            var writerAvro = KafkaWriterBuilder.Create<Null, AdvancedAvroObject>(configuration)
                .WithTopic("AvroTopicAsync")
                .UsingAvro()
                .WithKeyFunction(message => null!)
                .Build();
            
            var readerAvro = KafkaReaderBuilder.Create<Null, AdvancedAvroObject>(configuration)
                .WithTopic("AvroTopicAsync")
                .WithGroupId("AvroGroupAsync")
                .UsingAvro()
                .Build();
            
            // Act
            await writerAvro.WriteAsync(AdvancedAvroObject2);
            var readMessages = readerAvro.ReadMessages(1, TimeSpan.FromSeconds(20));
            
            // Assert
            Assert.Single(readMessages);
            Assert.True(readerAvro.VerifyNoMoreMessages(TimeSpan.FromSeconds(20)));
        }

        [Fact]
        public async Task WriteMultipleAdvancedAvroObjAsyncTest()
        {
            // Arrange
            var writerAvro = KafkaWriterBuilder.Create<Null, AdvancedAvroObject>(configuration)
                .WithTopic("AvroTopicAsync_2")
                .UsingAvro()
                .WithKeyFunction(message => null!)
                .Build();
            
            var readerAvro = KafkaReaderBuilder.Create<Null, AdvancedAvroObject>(configuration)
                .WithTopic("AvroTopicAsync_2")
                .WithGroupId("AvroGroupAsync_2")
                .UsingAvro()
                .Build();
            
            // Act
            await writerAvro.WriteAsync(AdvancedAvroObject1);
            await writerAvro.WriteAsync(AdvancedAvroObject2);
            await writerAvro.WriteAsync(AdvancedAvroObject1);
            var readMessages = readerAvro.ReadMessages(3, TimeSpan.FromSeconds(20));
            
            // Assert
            Assert.Equal(3, readMessages.Count());
            Assert.True(readerAvro.VerifyNoMoreMessages(TimeSpan.FromSeconds(20)));
        }
    }
}
