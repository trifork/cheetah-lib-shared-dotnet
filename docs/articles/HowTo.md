# Component test using Kafaka

How to get started using this libarys Kafaka `producer` and `comsumer` for making component tests.

## Build a Kafka Writer
To use a writer, which is this librarys abstration for a Kafka `producer`, it is a two step process. 

Building the writer takes a couple of required methods imputs to get started. Writer is build in the following way.

```c#
var writer = KafkaWriterBuilder.Create<string, string>()
                .WithKafkaConfigurationPrefix(string.Empty, configuration)
                .WithTopic("MyTopic")
                .WithKeyFunction(message => message)
                .Build();
```
`WithKafkaConfigurationPrefix` tells the builder which configuration is containing the required [value](https://docs.cheetah.trifork.dev/libraries/cheetah-lib-shared-dotnet/articles/CheetahComponentTest.html). The first parameter is a prefix, which can be used to run with different Kafka configurations for the same componentest, e.g. ```WithKafkaConfigurationPrefix("testPrefix", configuration)```.

The configuration would then need to carry the same prefix.

`WithTopic` defines the topic used for the writer. Since Kafka operates on a `producer` and `consumer`, these will be bound to a topic. So that the writer can't change topics or write to topis it's not assigned to.

`WithKeyFunction` is for specifying the event key used by the Kafka `producer`. Read more about event keys and main comcepts of Kafka [here.](https://kafka.apache.org/documentation/#intro_concepts_and_terms) If no key is desired you can use `KafkaWriterBuilder.Create<Null, InputModel>` when calling create. 

`Build` is the final call, and is setting up the writer with the configuration given. After this the writer will be ready to be used.

## Use a Kafka Writer

Because the writer is set up as a `producer` for only one topic, and the keys have been define before hand. The writer only has one job, and that is to write messages. Either one or a series of message.

```c#
writer.Writer(message);
```

## Build a Kafka Reader

The Kafka `consumer` uses many of the same configurations as the `producer`.

```c#
var reader = KafkaReaderBuilder.Create<string, string>()
            .WithKafkaConfigurationPrefix(string.Empty, configuration)
            .WithTopic("MyTopic")
            .WithGroupId("MyGroup")
            .Build();
```

`WithKafkaConfigurationPrefix` tells the builder which configuration is containing the required [value](https://docs.cheetah.trifork.dev/libraries/cheetah-lib-shared-dotnet/articles/CheetahComponentTest.html). The first parameter is a prefix, which can be used to run with different Kafka configurations for the same componentest, e.g. ```WithKafkaConfigurationPrefix("testPrefix", configuration)```.

The configuration would then need to carry the same prefix.

`WithTopic` defines the topic used for the writer. Since Kafka operates on a `producer` and `consumer`, these will be bound to a topic. So that the reader can't change topics or read from topis it's not assigned to.

`WithGroupId` is used to make sure the reader does not have to read the whole topic every time it is reading from the topic. This also means that if the writer writes to the topic and reader reads those messages, it is possible to use the same writer to write to the same topic, without having to do any clean up, as the reader keeps track of the offset of the topic, for the group. Read more about consumer groups and offsets [here.](https://docs.confluent.io/platform/current/clients/consumer.html)

## Using the Kafka Reader

Like the `producer`, the `consumer` is bound to one topic, and therefor is very simple to use. For testing purpose, to read from a topic it's required to give a count of how many messages the reader should read. This will make it possible to test if the correct number of messages was on the topic.

To read from a topic the following method is used.

```c#
reader.ReadMessages(numberOfMessages, TimeSpan.FromSeconds(numberOfSeconds));
```

If the reader reads the amount of messages that is specified, it is possible to check if there is still messages left unread on the topic. This is done in the following way.

```c#
reader.VerifyNoMoreMessages(TimeSpan.FromSeconds(numberOfSeconds))
```

This reads every messages on the topic until either `numberOfSeconds` is timed out or the tipic is EOF. 