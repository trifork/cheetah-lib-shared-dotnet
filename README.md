# Cheetah.Shared (Nuget Package)

This is a project containing shared functionality for .net projects interacting with the data platform.

See reference projects

* <https://github.com/trifork/cheetah-example-webapi>
* <https://github.com/trifork/cheetah-example-alertservice>

and other utilities using the library:

* <https://github.com/trifork/cheetah-lib-templates-dotnet>

features offered by this library:

* Prometheus exposed on a kestrel server
* Helper methods for connecting and authentication against OpenSearch
* Helper methods for Authentication against Kafka
* Helper methods for building OpenSearch-search indices

## Functionality

### OpenSearch

This library provides a `CheetahOpenSearchClient` wrapper which contains a wrapped `OpenSearchClient` with some standard configuration and authentication setup.  
The internal client can be accessed directly at `_cheetahOpenSearchClient.InternalClient` for interacting with OpenSearch.

```c#
# Register the client for dependency injection
services.AddMemoryCache();
services.AddHttpClient();
services.AddTransient<ICheetahOpenSearchClient, CheetahOpenSearchClient>();
```

#### OpenSearch OAuth2 authentication

To enable Oauth2 authentication you can provide the following options through environment variables:

* `OpenSearch__AuthMode=OAuth2` - Token endpoint used to obtain token for authentication and authorization
* `OpenSearch__TokenEndpoint` - Token endpoint used to obtain token for authentication and authorization
* `OpenSearch__ClientId` - Client id used to obtain JWT from token endpoint
* `OpenSearch__ClientSecret` - Client secret used to obtain JWT from token endpoint

If these environment variables are not provided, the `CheetahOpenSearchClient`  will try to communicate with OpenSearch using basic auth.

#### OpenSearch Naming Strategies

In order to store data in OpenSearch, you need an Index.
We are providing a number of different naming strategies for querying Indexes:

The `<>` indicates required param, while `[]` indicates optional. e.g `prefix` is always optional.
* `SimpleIndexNamingStrategy`: follows the pattern `<base>_[prefix]`.
    This is the simplest Index naming
* `CustomerIndexNamingStrategy`: follows the pattern `<base>_[prefix]_<customer>_*`.
    (For querying) This gives us all the Indexes for a customer - all years/months
* `YearResolutionIndexNamingStrategy`: follows the pattern `<base>_[prefix]_<customer>_<year>`.
    This builds on top of the `CustomerIndexNamingStrategy` but adds sharding based on the year
* `MonthResolutionIndexNamingStrategy`: follows the pattern `<base>_[prefix]_<customer>_<year>_<zero-padded month>`.
    This builds on top of the `YearResolutionIndexNamingStrategy` but adds sharding based on month as well.
* `YearResolutionWithWildcardIndexNamingStrategy`: follows the pattern `<base>_[prefix]_<customer>_<year>*`.
    (For querying) This gives us all the Indexes a given customer and year - all the months

See an example at <https://github.com/trifork/cheetah-example-webapi>.

### Kafka

We are using the Confluent.Net client library and have added an additional extension method for authentication.  
<https://github.com/trifork/cheetah-example-alertservice> has a working example of using the library to connect to kafka.

#### Kafka OAuth2 authentication

```c#
# Setup a consumer or producer with OAuth
var consumer = new ConsumerBuilder<Ignore, string>(config)
                   ...
                    .AddCheetahOAuthentication(localProvider)
                    .Build();
```

To enable Oauth2 authentication you should also provide the following options through environment variables:

* `Kafka__TokenEndpoint` - Token endpoint used to obtain token for authentication and authorization
* `Kafka__ClientId` - Client id used to obtain JWT from token endpoint
* `Kafka__ClientSecret` - Client secret used to obtain JWT from token endpoint
