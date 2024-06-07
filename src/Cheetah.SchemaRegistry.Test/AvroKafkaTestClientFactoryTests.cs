using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cheetah.Kafka.Testing;
using Cheetah.SchemaRegistry.Test.TestModels.Avro;
using Cheetah.SchemaRegistry.Testing;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Cheetah.SchemaRegistry.Test
{
    [Trait("TestType", "IntegrationTests")]
    [Collection("IntegrationTests")]
    public class AvroKafkaTestClientFactoryTests
    {
        readonly KafkaTestClientFactory _testClientFactory;

        public AvroKafkaTestClientFactoryTests()
        {
            var localConfig = new Dictionary<string, string?>
            {
                { "KAFKA:URL", "localhost:9092" },
                { "KAFKA:OAUTH2:CLIENTID", "default-access" },
                { "KAFKA:OAUTH2:CLIENTSECRET", "default-access-secret" },
                { "KAFKA:OAUTH2:TOKENENDPOINT", "http://localhost:1852/realms/local-development/protocol/openid-connect/token " },
                { "KAFKA:OAUTH2:SCOPE", "kafka" },
                { "KAFKA:SECURITYPROTOCOL", "SaslPlaintext" },
                { "KAFKA:SASLMECHANISM", "OAuthBearer" },
                { "SCHEMAREGISTRY:URL", "http://localhost:8081/apis/ccompat/v7" },
                { "SCHEMAREGISTRY:OAUTH2:CLIENTID", "default-access" },
                { "SCHEMAREGISTRY:OAUTH2:CLIENTSECRET", "default-access-secret" },
                { "SCHEMAREGISTRY:OAUTH2:TOKENENDPOINT", "http://localhost:1852/realms/local-development/protocol/openid-connect/token " },
                { "SCHEMAREGISTRY:OAUTH2:SCOPE", "schema-registry" },
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(localConfig)
                .AddEnvironmentVariables()
                .Build();

            _testClientFactory = AvroKafkaTestClientFactory.Create(configuration);
        }


        static AvroObjectWithEnum AvroObjWithEnum1 =>
            new AvroObjectWithEnum
            {
                EnumType = EnumTypeAvro.EnumType1,
                NullableInt = null,
                NullableString = null
            };

        static AvroObjectWithEnum AvroObjWithEnum2 =>
            new AvroObjectWithEnum
            {
                EnumType = EnumTypeAvro.EnumType2,
                NullableInt = 123,
                NullableString = "bar"
            };

        static AdvancedAvroObject AdvancedAvroObject1 =>
            new AdvancedAvroObject
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

            var writerAvro = _testClientFactory.CreateTestWriter<SimpleAvroObject>(
                "SimpleAvroTopic"
            );
            var readerAvro = _testClientFactory.CreateTestReader<SimpleAvroObject>(
                "SimpleAvroTopic",
                "MyAvroGroup"
            );

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
            var writerAvro = _testClientFactory.CreateTestWriter<string, AdvancedAvroObject>(
                "AvroAdvancedTopic",
                o => o.Id
            );
            var readerAvro = _testClientFactory.CreateTestReader<string, AdvancedAvroObject>(
                "AvroAdvancedTopic",
                "AvroAdvancedGroup"
            );

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
            var writerAvro = _testClientFactory.CreateTestWriter<AdvancedAvroObject>(
                "AvroTopicAsync_2"
            );
            var readerAvro = _testClientFactory.CreateTestReader<AdvancedAvroObject>(
                "AvroTopicAsync_2"
            );

            // Act
            await writerAvro.WriteAsync(AdvancedAvroObject1);
            await writerAvro.WriteAsync(AdvancedAvroObject2);
            await writerAvro.WriteAsync(AdvancedAvroObject1);
            var readMessages = readerAvro.ReadMessages(3, TimeSpan.FromSeconds(5));

            // Assert
            readMessages.Count().Should().Be(3);
            readerAvro.VerifyNoMoreMessages(TimeSpan.FromSeconds(1)).Should().BeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData("my!cool:topic#")]
        [InlineData("my$expensive$topic")]
        // 249 characters is the maximum allowed length, this is 250 'a's
        [InlineData(
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
        )]
        public void Should_ThrowArgumentException_When_ProvidedInvalidTopicName(string topicName)
        {
            _testClientFactory
                .Invoking(factory => factory.CreateTestWriter<string>(topicName))
                .Should()
                .Throw<ArgumentException>(
                    "the factory should not be able to create clients with invalid topic names"
                );

            _testClientFactory
                .Invoking(factory => factory.CreateTestReader<string>(topicName))
                .Should()
                .Throw<ArgumentException>(
                    "the factory should not be able to create clients with invalid topic names"
                );

        }


    }
}
