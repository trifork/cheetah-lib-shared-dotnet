# Using Cheetah.Kafka

`Cheetah.Kafka` primary purpose is to bootstrap and configure Kafka clients and make them available for dependency injection.

## Prerequisites
- Basic understanding of Kafka and [the Apache Kafka .NET Client](https://docs.confluent.io/kafka-clients/dotnet/current/overview.html)

## Getting started

In an .NET application with some sort of `HostBuilder` startup, all you need to get started is the following lines in your `Program.cs`:
The snippet below will add Kafka's required dependencies and then register a pre-configured `IConsumer<ExampleKeyModel, ExampleValueModel>` for dependency injection:

```csharp
builder.Services
    .AddCheetahKafka(builder.Configuration)
    .WithConsumer<ExampleKeyModel, ExampleValueModel>();
```

This will add necessary dependencies and register an `IConsumer<ExampleKeyModel, ExampleValueModel>`, where `ExampleKeyModel` and `ExampleValueModel` are the key and value types of consumed messages.

This consumer can then be injected into other services like so:

```csharp
public class MyService {
    private readonly IConsumer<ExampleKeyModel, ExampleValueModel> _consumer;

    public MyService(IConsumer<ExampleKeyModel, ExampleValueModel> consumer)
    {
        _consumer = consumer;
    }
}
```

The injected consumer is pre-configured to:
* Authenticate towards Kafka using OAuth2
* Deserialize message values and keys from json with UTF8-encoding.

The same concept applies for producers and admin clients. The following example shows a producer and admin client being registered and injected:

```csharp
// in Program.cs
builder.Services.AddCheetahKafka(builder.Configuration)
    .WithProducer<string, ExampleModel>()
    .WithAdminClient();

// in MyService.cs
public class MyService {
    private readonly IProducer<string, ExampleModel> _producer;
    private readonly IAdminClient _adminClient;

    public MyService(IProducer<string, ExampleModel> producer, IAdminClient adminClient){
        _producer = producer;
        _adminClient = adminClient;
    }
}
```

## Configuration

You'll need to provide the following configuration to use `Cheetah.Kafka`:

| Key                            | Description                                                  | Example                                                                       | Required |
|--------------------------------|--------------------------------------------------------------|-------------------------------------------------------------------------------|----------|
| `Kafka__Url`                   | The url to kafka. Must *not* include a scheme prefix.        | `kafka:19092`                                                                 | ✓        |
| `Kafka__SecurityProtocol`      | Security protocol used to connect to Kafka.                  | `SaslPlaintext`                                                               |          |
| `Kafka__SaslMechanism`         | Sasl mechanism used to authenticate towards Kafka.           | `OAuthBearer`                                                                 |          |
| `Kafka__OAuth2__ClientId`      | The Client Id to use when retrieving tokens using OAuth2     | `default-access`                                                              | When `SaslMechanism=OAuthBearer`         |
| `Kafka__OAuth2__ClientSecret`  | The Client Secret to use when retrieving tokens using OAuth2 | `default-access-secret`                                                       | When `SaslMechanism=OAuthBearer`         |
| `Kafka__OAuth2__TokenEndpoint` | The endpoint where tokens should be retrieved from           | `http://keycloak:1852/realms/local-development/protocol/openid-connect/token` | When `SaslMechanism=OAuthBearer`         |
| `Kafka__OAuth2__Scope`         | The scope that the requested token should have               | `kafka`                                                                       | When `SaslMechanism=OAuthBearer`         |

The below example configuration can be placed in an `appsettings.json` to supply the necessary keys in development. These should be supplied through environment variables when running through docker-compose or in cluster.

```json
"Kafka": {
    "Url": "kafka:19092",
    "SecurityProtocol": "SaslPlaintext",
    "SaslMechanism": "OAuthBearer",
    "OAuth2":{
        "ClientId": "default-access",
        "ClientSecret": "default-access-secret",
        "TokenEndpoint": "http://keycloak:1852/realms/local-development/protocol/openid-connect/token",
        "Scope": "kafka" 
    }
}
```

## Configuring client behavior

It is possible to configure client behavior both generally for all clients, for specific types of clients and for individual clients.

The default configuration used by generated clients can be modified when calling `AddCheetahKafka`, while the configuration for individual clients are modified when they are registered.

The below snippet shows configuration of all clients to allow auto-creation of topics, specifies that all created consumer should be in the `my-group` consumer group, then registers two consumers, one of which is explicitly told to use the `the-group` consumer group, and to serialize the key into a string type using the Confluent Kafka Utf8 Deserializer instead of the default json deserializer.

```csharp
[...]
using Confluent.Kafka;

[...]

builder.Services.AddCheetahKafka(builder.Configuration, options => 
    {
        options.ConfigureDefaultClient(config => {
            config.AllowAutoCreateTopics = true;
        });
        options.ConfigureDefaultConsumer(config => {
            config.GroupId = "my-group";
        });
    })
    .WithConsumer<string, ExampleModel>(options => {
        options.SetKeyDeserializer(sp => Deserializers.Utf8);
        options.ConfigureClient(cfg =>
        {
            cfg.GroupId = "the-group";
        });
    })
    .WithConsumer<KeyModel, OtherModel>();
```

From here, one would be able to inject `IConsumer<string, ExampleModel>` and `IConsumer<KeyModel, OtherModel>` in other services. The same configuration is possible for producers and admin clients.

## Multiple clients with the same signature

In some scenarios, you want multiple clients with the same signature (e.g. `IProducer<string, ExampleModel>`), but with different configurations.

While it is possible to register a given producer type multiple times by repeatedly calling `WithProducer` and inject them all using `IEnumerable<IProducer<TKey, TValue>>`, it is usually necessary to be able to differentiate differently configured producers.

This can be resolved by using [.NET's Keyed Services](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-8.0#keyed-services). The below example registers two different producers with different batch sizes, each with a different key:

```csharp
builder.Services.AddCheetahKafka(builder.Configuration)
    .WithKeyedProducer<string, ExampleModel>("BigBatch", cfg => {
        cfg.BatchSize = 9001;
    })
    .WithKeyedProducer<string, ExampleModel>("SmallBatch", cfg => {
        cfg.BatchSize = 100;
    });
```

These can then be injected into a service using the `[FromKeyedServices]` attribute:

```csharp
public class MyService {
    private readonly IProducer<string, ExampleModel> _bigBatchProducer;
    private readonly IProducer<string, ExampleModel> _smallBatchProducer;

    public MyService(
        [FromKeyedServices("BigBatch")] IProducer<string, ExampleModel> bigBatchProducer,
        [FromKeyedServices("SmallBatch")] IProducer<string, ExampleModel> smallBatchProducer){
            _bigBatchProducer = bigBatchProducer;
            _smallBatchProducer = smallBatchProducer;
        }
}
```

## Using KafkaClientFactory

> [!NOTE]
> The following method of using the package is both possible and valid, but makes it the reader's responsibility to handle client lifetimes and individual client configuration
>
> We recommend registering and configuring clients using the methods described in the previous sections.

Internally, the package uses a `KafkaClientFactory` to create the clients that get registered. This factory can be dependency injected into order services to create clients without injecting them directly:

```csharp
// In Program.cs
builder.Services.AddCheetahKafka(builder.Configuration);

// In MyService.cs
public class MyService {
    private readonly KafkaClientFactory _factory;

    public MyService(KafkaClientFactory factory){
        _factory = factory;
    }

    public void PublishMessage<TKey, TValue>(TKey key, TValue message, string topicName){
        var producer = _factory.CreateProducer<TKey, TValue>();
        producer.Produce(topicName, new Message<TKey, TValue> { Key = key, Value = message });
    }
}
```

The above snippet uses the KafkaClientFactory and essentially enables MyService to publish any type of Message.

Bear in mind that creating and disposing producers this often is not generally recommended and that the above example is primarily to showcase what *can* be done and not what *should* be done.