using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cheetah.ComponentTest.Kafka;
using Cheetah.ComponentTest.Test.Model.Avro;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Cheetah.ComponentTest.Test
{
    [Trait("TestType", "IntegrationTests")]
    [Collection("IntegrationTests")]
    public class KafkaWriterReaderTests
    {
        readonly IConfiguration _configuration;

        public KafkaWriterReaderTests()
        {
            var conf = new Dictionary<string, string?>
            {
                { "KAFKA:AUTHENDPOINT", "http://localhost:1752/oauth2/token" },
                { "KAFKA:CLIENTID", "ClientId" },
                { "KAFKA:CLIENTSECRET", "testsecret" },
                { "KAFKA:URL", "localhost:9092" },
                { "KAFKA:SCHEMAREGISTRYURL", "http://localhost:8081/apis/ccompat/v7" }
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(conf)
                .AddEnvironmentVariables()
                .Build();
        }

        [Fact]
        public async Task Should_WriteAndRead_When_UsingJson()
        {
            var writer = KafkaWriterBuilder.Create<string, string>(_configuration)
                .WithTopic("MyJsonTopic")
                .WithKeyFunction(message => message)
                .Build();

            var reader = KafkaReaderBuilder.Create<string, string>(_configuration)
                .WithTopic("MyJsonTopic")
                .WithConsumerGroup("MyConsumerGroup")
                .Build();

            await writer.WriteAsync("Message4");
            var readMessages = reader.ReadMessages(1, TimeSpan.FromSeconds(5));
            readMessages.Should().HaveCount(1);
            reader.VerifyNoMoreMessages(TimeSpan.FromSeconds(1)).Should().BeTrue();
        }

        static AvroObjectWithEnum AvroObjWithEnum1 =>
            new AvroObjectWithEnum { EnumType = EnumTypeAvro.EnumType1, NullableInt = null, NullableString = null };

        static AvroObjectWithEnum AvroObjWithEnum2 =>
            new AvroObjectWithEnum { EnumType = EnumTypeAvro.EnumType2, NullableInt = 123, NullableString = "bar" };

        static AdvancedAvroObject AdvancedAvroObject1 => new AdvancedAvroObject
        {
            Id = "Id",
            Name = "AvroName",
            LongNumber = 11899823748932,
            AvroObjectWithEnum = AvroObjWithEnum1
        };

        static readonly AdvancedAvroObject AdvancedAvroObject2 = new AdvancedAvroObject
        {
            Id = "Id",
            Name = "Foo",
            LongNumber = 345342523454,
            AvroObjectWithEnum = AvroObjWithEnum2
        };

        [Fact]
        public async Task Should_WriteAndReadSimpleObjects_When_UsingAvro()
        {
            // Arrange
            var avroModel = new SimpleAvroObject { Name = "foo", Number = 100 };

            var writerAvro = KafkaWriterBuilder.Create<SimpleAvroObject>(_configuration)
                .WithTopic("SimpleAvroTopic")
                .UsingAvro()
                .Build();

            var readerAvro = KafkaReaderBuilder.Create<SimpleAvroObject>(_configuration)
                .WithTopic("SimpleAvroTopic")
                .WithConsumerGroup("MyAvroGroup")
                .UsingAvro()
                .Build();

            // Act
            await writerAvro.WriteAsync(avroModel);
            var readMessages = readerAvro.ReadMessages(1, TimeSpan.FromSeconds(5));

            // Assert
            readMessages.Should().HaveCount(1);
            readerAvro.VerifyNoMoreMessages(TimeSpan.FromSeconds(1)).Should().BeTrue();
        }

        [Fact]
        public async Task Should_WriteAndReadAdvancedObjects_When_UsingAvro()
        {
            // Arrange
            var writerAvro = KafkaWriterBuilder.Create<AdvancedAvroObject>(_configuration)
                .WithTopic("AvroAdvancedTopic")
                .UsingAvro()
                .Build();

            var readerAvro = KafkaReaderBuilder.Create<AdvancedAvroObject>(_configuration)
                .WithTopic("AvroAdvancedTopic")
                .WithConsumerGroup("AvroAdvancedGroup")
                .UsingAvro()
                .Build();

            // Act
            await writerAvro.WriteAsync(AdvancedAvroObject1);
            var readMessages = readerAvro.ReadMessages(1, TimeSpan.FromSeconds(5));

            // Assert
            readMessages.Should().HaveCount(1);
            readerAvro.VerifyNoMoreMessages(TimeSpan.FromSeconds(1)).Should().BeTrue();
        }

        [Fact]
        public async Task Should_WriteAndReadMultipleAdvancedObjects_When_UsingAvro()
        {
            // Arrange
            var writerAvro = KafkaWriterBuilder.Create<AdvancedAvroObject>(_configuration)
                .WithTopic("AvroTopicAsync_2")
                .UsingAvro()
                .Build();

            var readerAvro = KafkaReaderBuilder.Create<AdvancedAvroObject>(_configuration)
                .WithTopic("AvroTopicAsync_2")
                .WithConsumerGroup("AvroGroupAsync_2")
                .UsingAvro()
                .Build();

            // Act
            await writerAvro.WriteAsync(AdvancedAvroObject1);
            await writerAvro.WriteAsync(AdvancedAvroObject2);
            await writerAvro.WriteAsync(AdvancedAvroObject1);
            var readMessages = readerAvro.ReadMessages(3, TimeSpan.FromSeconds(5));

            // Assert
            readMessages.Count().Should().Be(3);
            readerAvro.VerifyNoMoreMessages(TimeSpan.FromSeconds(1)).Should().BeTrue();
        }

        [Fact]
        public async Task Should_ThrowArgumentException_When_AttemptingToWrite0Messages()
        {
            var writer = KafkaWriterBuilder.Create<string, string>(_configuration)
                .WithTopic("MyThrowingTopic")
                .WithKeyFunction(message => message)
                .Build();

            await writer.Invoking(w => w.WriteAsync())
                .Should()
                .ThrowAsync<ArgumentException>("it should not be possible to write 0 messages");
        }

        [Theory]
        [InlineData("KAFKA:AUTHENDPOINT", false)]
        [InlineData("KAFKA:CLIENTID", false)]
        [InlineData("KAFKA:CLIENTSECRET", false)]
        [InlineData("KAFKA:URL", false)]
        [InlineData("KAFKA:SCHEMAREGISTRYURL", true)]
        public void Should_ThrowArgumentException_When_RequiredConfigurationIsMissing(string missingKey, bool isAvro)
        {
            var configurationDictionary = new Dictionary<string, string?>
            {
                { "KAFKA:AUTHENDPOINT", "http://localhost:1752/oauth2/token" },
                { "KAFKA:CLIENTID", "ClientId" },
                { "KAFKA:CLIENTSECRET", "testsecret" },
                { "KAFKA:URL", "localhost:9092" },
                { "KAFKA:SCHEMAREGISTRYURL", "http://localhost:8081/apis/ccompat/v7" }
            };

            configurationDictionary.Remove(missingKey);

            var invalidConfiguration = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationDictionary)
                .Build();

            var writerBuilder = KafkaWriterBuilder.Create<string, string>(invalidConfiguration)
                .WithTopic("MyThrowingTopic")
                .WithKeyFunction(message => message);

            var readerBuilder = KafkaReaderBuilder.Create<string, string>(invalidConfiguration)
                .WithTopic("MyThrowingTopic")
                .WithConsumerGroup("MyConsumerGroup");

            if (isAvro)
            {
                writerBuilder.UsingAvro();
                readerBuilder.UsingAvro();
            }

            writerBuilder.Invoking(wb => wb.Build()).Should()
                .Throw<ArgumentException>(
                    "the builder should not successfully build if required configuration is missing");
            readerBuilder.Invoking(rb => rb.Build()).Should()
                .Throw<ArgumentException>(
                    "the builder should not successfully build if required configuration is missing");
        }

        [Theory]
        [InlineData("https://")]
        [InlineData("http://")]
        [InlineData("ftp://")]
        [InlineData("ssh://")]
        [InlineData("://")]
        public void Should_ThrowArgumentException_When_KafkaUrlHasScheme(string kafkaUrlPrefix)
        {
            var configurationDictionary = new Dictionary<string, string?>
            {
                { "KAFKA:AUTHENDPOINT", "http://localhost:1752/oauth2/token" },
                { "KAFKA:CLIENTID", "ClientId" },
                { "KAFKA:CLIENTSECRET", "testsecret" },
                { "KAFKA:URL", kafkaUrlPrefix + "localhost:9092" },
                { "KAFKA:SCHEMAREGISTRYURL", "http://localhost:8081/apis/ccompat/v7" }
            };

            var invalidConfiguration = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationDictionary)
                .Build();
            var writerBuilder = KafkaWriterBuilder.Create<string, string>(invalidConfiguration)
                .WithTopic("MyThrowingTopic")
                .WithKeyFunction(message => message);

            var readerBuilder = KafkaReaderBuilder.Create<string, string>(invalidConfiguration)
                .WithTopic("MyThrowingTopic")
                .WithConsumerGroup("MyConsumerGroup");
            writerBuilder.Invoking(wb => wb.Build()).Should()
                .Throw<ArgumentException>(
                    "the builder should not successfully build if the kafka url has a scheme prefix");
            readerBuilder.Invoking(rb => rb.Build()).Should()
                .Throw<ArgumentException>(
                    "the builder should not successfully build if the kafka url has a scheme prefix");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("my!cool:topic#")]
        [InlineData("my$expensive$topic")]
        // 249 characters is the maximum allowed length, this is 250 'a's
        [InlineData(
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        public void Should_ThrowArgumentException_When_ProvidedInvalidTopicName(string? topicName)
        {
            var writerBuilder = KafkaWriterBuilder.Create<string, string>(_configuration)
                .WithTopic(topicName)
                .WithKeyFunction(message => message);

            var readerBuilder = KafkaReaderBuilder.Create<string, string>(_configuration)
                .WithTopic(topicName)
                .WithConsumerGroup("MyConsumerGroup");

            writerBuilder.Invoking(wb => wb.Build())
                .Should().Throw<ArgumentException>(
                    "the builder should not successfully build if given an invalid topic");

            readerBuilder.Invoking(rb => rb.Build())
                .Should().Throw<ArgumentException>(
                    "the builder should not successfully build if given an invalid topic");
        }
    }
}
