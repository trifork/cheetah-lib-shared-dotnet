# Using Cheetah.OpenSearch

The primary purpose of Cheetah.OpenSearch is to simplify the registration and configuration of OpenSearch clients for use with the Cheetah data platform.

## Prerequisites

- Basic understanding of OpenSearch and the [high-level OpenSearch .NET client](https://opensearch.org/docs/latest/clients/OSC-dot-net/)

## Getting Started

In an .NET application with some sort of `HostBuilder` startup, all you need to get started is the following line in your `Program.cs`:

```csharp
builder.Services.AddCheetahOpenSearch(builder.Configuration);
```

This will make an `IOpenSearchClient` available for dependency injection in other services like in this API Controller which has an endpoint that retrieves the names of all indexes that follow a specific index pattern:

```csharp
[ApiController]
[Route("[controller]")]
public class IndexController : ControllerBase
{
    readonly IOpenSearchClient _client;
    public IndexController(IOpenSearchClient client)
    {
        _client = client;
    }

    [HttpGet("indices/{indexPattern}")]
    public async Task<IActionResult> GetIndices([FromRoute] string indexPattern)
    {
        var response = await _client.Indices.GetAsync(indexPattern);
        var indexNames = response.Indices.Select(x => x.Key.Name);
        return Ok(indexNames);
    }
}
```

## Configuration

In order for Cheetah.OpenSearch to work, you must provide the following configuration:

| Key                                	| Description                                                                                      	| Example      	| Required               	|
|------------------------------------	|--------------------------------------------------------------------------------------------------	|--------------	|------------------------	|
| OpenSearch:Url                     	| The url to OpenSearch.                                                                           	| `kafka:9092` 	| x                      	|
| OpenSearch:AuthMode                	| The authentication method to use. Valid values: `None`, `Basic`, `OAuth2`. Default: `Basic`      	| `OAuth2`     	|                        	|
| OpenSearch:UserName                	| The username to use when authenticating using `Basic` authentication.                            	| `admin`      	| When `AuthMode=Basic`  	|
| OpenSearch:Password                	| The password to use when authenticating using `Basic` authentication.                            	| `p4$$w0rD    	| When `AuthMode=Basic`  	|
| OpenSearch:OAuth2:TokenEndpoint    	| The endpoint to retrieve tokens from when using `OAuth2` authentication.                         	| `kafka`      	| When `AuthMode=OAuth2` 	|
| OpenSearch:OAuth2:ClientId         	| The client id to retrieve tokens for when using `OAuth2` authentication.                         	|              	| When `AuthMode=OAuth2` 	|
| OpenSearch:OAuth2:ClientSecret     	| The client secret to use when retrieving tokens for `OAuth2` authentication.                     	|              	| When `AuthMode=OAuth2` 	|
| OpenSearch:OAuth2:Scope            	| The scope to request when retrieving tokens for `OAuth2` authentication.                         	|              	| When `AuthMode=OAuth2` 	|
| OpenSearch:OAuth2:ClockSkewSeconds 	| The number of seconds of clock skew to allow for when validating `OAuth2` tokens. Default: `300` 	|              	|                        	|
| OpenSearch:DisableTlsValidation    	| Whether or not to disable TLS validation towards OpenSearch. Default: `false`                    	| `true`       	|                        	|
| OpenSearch:CaCertificatePath       	| The path to the CA certificate used to validate OpenSearch's certificate.                        	|              	|                        	|

The below example configuration can be placed in an `appsettings.json` to supply the necessary keys in development. These should be supplied through environment variables when running through docker-compose or in cluster.

```json
"OpenSearch": {
    "Url": "http://opensearch:9200",
    "AuthMode": "OAuth2",
    "OAuth2":{
        "ClientId": "default-access",
        "ClientSecret": "default-access-secret",
        "TokenEndpoint": "http://keycloak:1852/realms/local-development/protocol/openid-connect/token",
        "Scope": "opensearch"
    }
}
```

## Modifying client behavior

It is possible to modify the registered client's behavior through its options, which in the snippet below is used to modify the serialization behavior of the client:

```csharp
builder.Services.AddCheetahOpenSearch(builder.Configuration, cfg =>
{
    cfg.WithJsonSerializerSettings(settings => {
        settings.MissingMemberHandling = MissingMemberHandling.Error;
        settings.Converters.Add(new UtcDateTimeConverter());
    });
});
```

This specifies that the serialization should throw when a missing member is encountered and that it should add a custom converter for handling DateTimes.

The `UtcDateTimeConverter` comes as part of `Cheetah.Kafka` and makes it possible for the client to serialize epoch millis timestamps into DateTimes and vice-versa. It was previously a built-in converter in `Cheetah.Shared.WebApi`, which means that if you're migrating from there, you might need to manually add this converter like in the snippet above.