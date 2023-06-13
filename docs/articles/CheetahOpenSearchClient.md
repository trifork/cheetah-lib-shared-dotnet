# CheetahOpenSearchClient

This library provides a `CheetahOpenSearchClient` wrapper which contains a wrapped `OpenSearchClient` with some standard configuration and authentication setup.  
The internal client can be accessed directly at `_cheetahOpenSearchClient.InternalClient` for interacting with OpenSearch.

```c#
# Register the client for dependency injection
services.AddMemoryCache();
services.AddHttpClient();
services.AddTransient<IMetricReporter, MetricReporter>();
services.AddTransient<ICheetahOpenSearchClient, CheetahOpenSearchClient>();
```

## Configuration

You can call `_cheetahOpenSearchClient.SetJsonSerializerSettingsFactory` to manage how JSON is being serialized and deserialized.

You can see the default behavior in case you have not set your own implementation:

[!code-csharp[](../../src/Cheetah.Core/Infrastructure/Services/OpenSearchClient/CheetahOpenSearchClient.cs#GetJsonSerializerSettingsFactory)]