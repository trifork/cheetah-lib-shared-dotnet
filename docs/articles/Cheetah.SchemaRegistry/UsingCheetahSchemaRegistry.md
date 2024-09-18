# Using Cheetah.SchemaRegistry

`Cheetah.SchemaRegistry` primary purpose is to bootstrap and configure Schema Registry clients, and (De)serializers and make them available for dependency injection. `Cheetah.SchemaRegistry` depends on `Cheetah.Kafka`, and should be used in combination with clients injected by [Cheetah.Kafka](../Cheetah.Kafka/latest/UsingCheetahKafka.md).

## Prerequisites
- Basic understanding of Schema Registry and [the Apache Kafka .NET Client](https://docs.confluent.io/kafka-clients/dotnet/current/overview.html)

## Getting started

In an .NET application with some sort of `HostBuilder` startup, all you need to get started is the following lines in your `Program.cs`:
The snippet below will add SchemaRegistry's and Kafka's required dependencies and then register a pre-configured `IProducer<string, ExampleModelAvro>` for dependency injection, which uses Avro Serialization utilizing a SchemaRegistry client:

```csharp
builder.Services.AddCheetahSchemaRegistry(builder.Configuration);

builder.Services.AddCheetahKafka(builder.Configuration, options => 
    .WithProducer<string, ExampleModelAvro>(options =>
    {
        options.SetKeySerializer(Serializers.Utf8);
        options.SetValueSerializer(AvroSerializer.FromServices<ExampleModelAvro>());
    });
```

This will add necessary dependencies and register an `IProducer<string, ExampleModelAvro>`, where `string` and `ExampleModelAvro` are the key and value types of produced messages.

For futher information an usage see [Using Cheetah.Kafka](../Cheetah.Kafka/latest/UsingCheetahKafka.md) and [Testing with Cheetah.Kafka](../Cheetah.Kafka/latest/TestingWithCheetahKafka.md).

## Configuration

You'll need to provide the following configuration to use `Cheetah.SchemaRegistry`:

| Key                            | Description                                                  | Example                                                                       | Required |
|--------------------------------|--------------------------------------------------------------|-------------------------------------------------------------------------------|----------|
| `SchemaRegistry__Url`                   | The url to schemaregistry.        | `localhost:8081/apis/ccompat/v7`                                                                 | ✓        |                                                             |          |
| `SchemaRegistry__OAuth2__ClientId`      | The Client Id to use when retrieving tokens using OAuth2     | `default-access`                                                              | ✓         |
| `SchemaRegistry__OAuth2__ClientSecret`  | The Client Secret to use when retrieving tokens using OAuth2 | `default-access-secret`                                                       | ✓         |
| `SchemaRegistry__OAuth2__TokenEndpoint` | The endpoint where tokens should be retrieved from           | `http://keycloak:1852/realms/local-development/protocol/openid-connect/token` | ✓         |
| `SchemaRegistry__OAuth2__Scope`         | The scope that the requested token should have               | `schema-registry`                                                                       | ✓         |

The below example configuration can be placed in an `appsettings.json` to supply the necessary keys in development. These should be supplied through environment variables when running through docker-compose or in cluster.

```json
"SchemaRegistry": {
    "Url": "localhost:8081/apis/ccompat/v7",
    "OAuth2":{
      "ClientId": "default-access",
      "ClientSecret": "default-access-secret",
      "TokenEndpoint": "http://localhost:1852/realms/local-development/protocol/openid-connect/token ",
      "Scope": "schema-registry"
    }
}
```
