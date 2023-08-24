using Cheetah.ComponentTest.Kafka;
using Cheetah.ComponentTest.XUnit.Model.Avro;
using Microsoft.Extensions.Configuration;

namespace Cheetah.ComponentTest.XUnit;

[Trait("TestType", "IntegrationTests")]
[Collection("IntegrationTests")]
public class KafkaWriterReaderTests
{
    readonly IConfiguration _configuration;

    public KafkaWriterReaderTests()
    {
        var conf = new Dictionary<string, string>
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
        var readMessages = reader.ReadMessages(1, TimeSpan.FromSeconds(20));
        Assert.Single(readMessages);
        Assert.True(reader.VerifyNoMoreMessages(TimeSpan.FromSeconds(2)));
    }
        
    static AvroObjectWithEnum AvroObjWithEnum1 =>
        new() { EnumType = EnumTypeAvro.EnumType1, NullableInt = null, NullableString = null };

    static AvroObjectWithEnum AvroObjWithEnum2 =>
        new() { EnumType = EnumTypeAvro.EnumType2, NullableInt = 123, NullableString = "bar" };
        
    static AdvancedAvroObject AdvancedAvroObject1 => new()
    {
        Id = "Id", Name = "AvroName", LongNumber = 11899823748932, AvroObjectWithEnum = AvroObjWithEnum1
    };
    static readonly AdvancedAvroObject AdvancedAvroObject2 = new()
    {
        Id = "Id", Name = "Foo", LongNumber = 345342523454, AvroObjectWithEnum = AvroObjWithEnum2
    };

    [Fact]
    public async Task Should_WriteAndReadSimpleObjects_When_UsingAvro()
    {
        // Arrange
        var avroModel = new SimpleAvroObject() { Name = "foo", Number = 100 };
        
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
        var readMessages = readerAvro.ReadMessages(1, TimeSpan.FromSeconds(10));
            
        // Assert
        Assert.Single(readMessages);
        Assert.True(readerAvro.VerifyNoMoreMessages(TimeSpan.FromSeconds(2)));
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
        var readMessages = readerAvro.ReadMessages(1, TimeSpan.FromSeconds(10));
            
        // Assert
        Assert.Single(readMessages);
        Assert.True(readerAvro.VerifyNoMoreMessages(TimeSpan.FromSeconds(2)));
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
        var readMessages = readerAvro.ReadMessages(3, TimeSpan.FromSeconds(10));
            
        // Assert
        Assert.Equal(3, readMessages.Count());
        Assert.True(readerAvro.VerifyNoMoreMessages(TimeSpan.FromSeconds(2)));
    }

    [Fact]
    public async Task Should_ThrowArgumentException_When_AttemptingToWrite0Messages()
    {
        var writer = KafkaWriterBuilder.Create<string, string>(_configuration)
            .WithTopic("MyThrowingTopic")
            .WithKeyFunction(message => message)
            .Build();

        await Assert.ThrowsAsync<ArgumentException>(() => writer.WriteAsync());
    }

    [Theory]
    [InlineData("KAFKA:AUTHENDPOINT", false)]
    [InlineData("KAFKA:CLIENTID", false)]
    [InlineData("KAFKA:CLIENTSECRET", false)]
    [InlineData("KAFKA:URL", false)]
    [InlineData("KAFKA:SCHEMAREGISTRYURL", true)]

    public void Should_ThrowArgumentException_When_RequiredConfigurationIsMissing(string missingKey, bool isAvro)
    {
        var configurationDictionary = new Dictionary<string, string>
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
            
        Assert.Throws<ArgumentException>(() => writerBuilder.Build());
        Assert.Throws<ArgumentException>(() => readerBuilder.Build());
    }

    [Theory]
    [InlineData("https://")]
    [InlineData("http://")]
    [InlineData("ftp://")]
    [InlineData("ssh://")]
    [InlineData("://")]
    public void Should_ThrowArgumentException_When_KafkaUrlHasScheme(string kafkaUrlPrefix)
    {
        var configurationDictionary = new Dictionary<string, string>
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

        Assert.Throws<ArgumentException>(() => writerBuilder.Build());
        Assert.Throws<ArgumentException>(() => readerBuilder.Build());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("my!cool:topic#")]
    [InlineData("my$expensive$topic")]
    // 249 characters is the maximum allowed length, this is 250 'a's
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    public void Should_ThrowArgumentException_When_ProvidedInvalidTopicName(string topicName)
    {
        var writerBuilder = KafkaWriterBuilder.Create<string, string>(_configuration)
            .WithTopic(topicName)
            .WithKeyFunction(message => message);

        Assert.Throws<ArgumentException>(() => writerBuilder.Build());
    }
}
