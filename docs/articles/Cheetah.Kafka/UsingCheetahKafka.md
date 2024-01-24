# Using Cheetah.Kafka

`Cheetah.Kafka` primary purpose is to bootstrap and configure Kafka clients and make them available for dependency injection.

## Prerequisites
- Basic understanding of Kafka and [the Apache Kafka .NET Client](https://docs.confluent.io/kafka-clients/dotnet/current/overview.html)

## Getting started
 
In an .NET application with some sort of `HostBuilder` startup, all you need to get started is the following line in your `Program.cs`:

```csharp
builder.Services.AddCheetahKafka(builder.Configuration);
```

This will register a `KafkaClientFactory` for dependency injection, which can be used to instantiate clients.

Although using `KafkaClientFactory` directly is possible and a perfectly valid way to use the library, we instead recommend registering your clients explicitly.

The snippet below will add Kafka's required dependencies and then register a pre-configured `IConsumer<string, ExampleModel>` for dependency injection:

```csharp
// In Program.cs
builder.Services.AddCheetahKafka(builder.Configuration)
    .WithConsumer<string, ExampleModel>();

builder.Services.AddHostedService<MyConsumerService>();

// In MyConsumerService.cs
public class MyConsumerService : BackgroundService {
    private readonly IConsumer<string, ExampleModel> _consumer;

    public MyConsumerService(IConsumer<string, ExampleModel> consumer)
    {
        _consumer = consumer;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Consume message using _consumer
    }
}
```

The injected consumer is pre-configured to:
* Authenticate towards Kafka using OAuth2
* Serialize message values using an `Utf8Serializer<ExampleModel>`

The same concept applies for registering and injecting `IProducer<TKey, TValue>` and `IAdminClient` instances.

## Configuration

You'll need to provide the following configuration to use `Cheetah.Kafka`:

| Key                        	| Description                                                  	| Example                                                                       	| Required 	|
|----------------------------	|--------------------------------------------------------------	|-------------------------------------------------------------------------------	|----------	|
| `Kafka__Url`                  	| The url to kafka. Must *not* include a scheme prefix.        	| `kafka:19092`                                                                  	| ✓        	|
| `Kafka__OAuth2__ClientId`      	| The Client Id to use when retrieving tokens using OAuth2     	| `default-access`                                                                	| ✓        	|
| `Kafka__OAuth2__ClientSecret`  	| The Client Secret to use when retrieving tokens using OAuth2 	| `default-access-secret`                                                                	| ✓        	|
| `Kafka__OAuth2__TokenEndpoint` 	| The endpoint where tokens should be retrieved from           	| `http://keycloak:1852/realms/local-development/protocol/openid-connect/token` 	| ✓        	|
| `Kafka__OAuth2__Scope`         	| The scope that the requested token should have               	| `kafka`                                                                       	|          	|

The below example configuration can be placed in an `appsettings.json` to supply the necessary keys in development. These should be supplied through environment variables when running through docker-compose or in cluster.

```json
"Kafka": {
    "Url": "kafka:19092",
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

The below snippet shows configuration of all clients to allow auto-creation of topics, specifies that all created consumer should be in the `my-group` consumer group, then registers two consumers, one of which is explicitly told to use the `the-group` consumer group.

```csharp
builder.Services.AddCheetahKafka(builder.Configuration, options => 
    {
        options.ConfigureDefaultClient(config => {
            config.AllowAutoCreateTopics = true;
        });
        options.ConfigureDefaultConsumer(config => {
            config.GroupId = "my-group";
        });
    })
    .WithConsumer<string, ExampleModel>(config => {
        config.GroupId = "the-group";
    })
    .WithConsumer<string, OtherModel>();
```

From here, one would be able to inject `IConsumer<string, ExampleModel>` and `IConsumer<string, OtherModel>` in other services. The same configuration is possible for producers and admin clients.

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