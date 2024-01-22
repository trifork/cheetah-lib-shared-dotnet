# Using Cheetah.Kafka

`Cheetah.Kafka` primary purpose is to bootstrap and configure Kafka clients and make them available for dependency injection.

This is done through the `AddCheetahKafka` extension method on an `IServiceCollection`.

In an ASP.Net application, all you need to get started is the following line in your `Program.cs`:

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

    protected override Task ExecutyAsync(CancellationToken stoppingToken)
    {
        // Consume message using _consumer
    }
}
```


## Configuration

You'll need to provide the following configuration to use `Cheetah.Kafka`:

| Key                        	| Description                                                  	| Example                                                                       	| Required 	|
|----------------------------	|--------------------------------------------------------------	|-------------------------------------------------------------------------------	|----------	|
| `Kafka__Url`                  	| The url to kafka. Must *not* include a scheme prefix.        	| `kafka:9092`                                                                  	| ✓        	|
| `Kafka__OAuth2__ClientId`      	| The Client Id to use when retrieving tokens using OAuth2     	| `kafka-client`                                                                	| ✓        	|
| `Kafka__OAuth2__ClientSecret`  	| The Client Secret to use when retrieving tokens using OAuth2 	| `kafka-secret`                                                                	| ✓        	|
| `Kafka__OAuth2__TokenEndpoint` 	| The endpoint where tokens should be retrieved from           	| `http://keycloak:1852/realms/local-development/protocol/openid-connect/token` 	| ✓        	|
| `Kafka__OAuth2__Scope`         	| The scope that the requested token should have               	| `kafka`                                                                       	|          	|

The below example configuration can be placed in an `appsettings.json` to supply the necessary keys in development. These should be supplied through environment variables when running through docker-compose or in cluster.

```json
"Kafka": {
    "Url": "localhost:19092",
    "OAuth2":{
        "ClientId": "clientid",
        "ClientSecret": "clientsecret",
        "TokenEndpoint": "http://localhost:1752/oauth2/token",
        "Scope": "kafka" 
    }
}
```