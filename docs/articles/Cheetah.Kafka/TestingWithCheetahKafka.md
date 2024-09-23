# Testing with Cheetah.Kafka

In order to easily test other components which communicate with Kafka, Cheetah.Kafka includes a simple way to generate test clients that can be used to verify the behavior of other components.

> [!CAUTION]
> The clients generated using the methods described here are intended to be short-lived and used solely for testing purposes. They have not been tested to work in long-running applications and should not be used in production scenarios. Instead, use the clients registered through `serviceCollection.AddCheetahKafka()`;

To get started we first create a `KafkaTestClientFactory`. This requires either a `KafkaConfig` instance or an `IConfiguration` with the same configuration requirements as described in [Using Cheetah.Kafka | Configuration](UsingCheetahKafka.md#configuration).    

```csharp
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.Test.json")
    .AddEnvironmentVariables()
    .Build();

var testClientFactory = KafkaTestClientFactory.Create(configuration);
```

## Creating writers and readers
With a `KafkaTestClientFactory` in hand, test writers and readers are easily created using the `CreateTestReader` and `CreateTestWriter` methods.

The example below creates a reader and a writer, both reading from and writing to the `MyTopic` topic, with the reader joining the `MyConsumerGroup` consumer group.

It then writes a message to the topic using the writer and retrieves the message using the reader, expecting 1 message with a maximum timeout of 5 seconds.

```csharp
var writer = testClientFactory.CreateTestWriter<ExampleModel>("MyTopic");
var reader = testClientFactory.CreateTestReader<ExampleModel>("MyTopic", "MyConsumerGroup")

var message = new Message<Null, ExampleModel>()
{
    Value = new ExampleModel()
};

await Writer.WriteAsync(message);

var messages = reader.ReadMessages(1, TimeSpan.FromSeconds(5));
```

The above example uses `null` keys for the messages that are sent. In order to write and read messages with keys, supply a second type parameter to the `CreateTestWriter` and `CreateTestReader` methods. When working with non Json (de)serialization, you will also need to supply key and/or value (de)serlializers:

Example of using a string as key, and value as integer.
```csharp
var keyedWriter = testClientFactory.CreateTestWriter<string, int>("MyTopic", keySerializer: Serializers.Utf8, valueSerializer: Serializers.Int32);
```

## Creating low-level clients
For more advanced test cases, you might want to skip the abstraction of the `IKafkaTestReader` and `IKafkaTestWriter`, opting instead for a fully-featured `IConsumer` or `IProducer`.

This can be achieved through the `ClientFactory` property on the `KafkaTestClientFactory`. This exposes the internal factory used to create clients, and allows you to create consumers, producers and admin clients as well as their respective builders, allowing for more custom behavior compared to the relatively limited feature set of the writers and readers.

Please be reminded that clients created this way are not intended for production scenarios.