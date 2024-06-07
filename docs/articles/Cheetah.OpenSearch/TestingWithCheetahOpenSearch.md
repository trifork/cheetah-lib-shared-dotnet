# Testing with Cheetah.OpenSearch

In order to easily test other components which communicate with OpenSearch, Cheetah.OpenSearch includes a simple way to generate test clients that can be used to verify the behavior of other components.

> [!CAUTION]
> The clients generated using the methods described here are intended to be short-lived and used solely for testing purposes. They have not been tested to work in long-running applications and should not be used in production scenarios. Instead, use the clients registered through `serviceCollection.AddCheetahOpenSearch()`;

In order to create a test client, you'll need to give it proper configuration. This requires either an `OpenSearchConfig` instance or an `IConfiguration` with the same configuration requirements as described in see [Using Cheetah.OpenSearch | Configuration](UsingCheetahOpenSearch.md#configuration).

The following snippet creates a new client with the given configuration:

```csharp
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.Test.json")
    .AddEnvironmentVariables()
    .Build();

IOpenSearchClient testClient = OpenSearchTestClient.Create(configuration);
```

The returned client is a pre-configured OpenSearchClient from OpenSearch's own library - For more information on how to use the client, see: [Getting started with the high-level .NET client (OpenSearch.Client)](https://opensearch.org/docs/latest/clients/OSC-dot-net/)

This can be used either in isolation or in conjunction with other methods (Publishing messages to Kafka, calling endpoints on an API) to verify that another component either creates or reacts to changes in OpenSearch in the manner you expect.

It is also possible to supply an `OpenSearchClientOptions` instance to the creation of the test client in order to modify client behavior. This follows the same concept as [Using Cheetah.OpenSearch | Modifying Client Behavior](UsingCheetahOpenSearch.md#modifying-client-behavior).