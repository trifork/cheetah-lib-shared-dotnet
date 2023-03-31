# Cheetah.Shared

This is a project containing shared functionality for projects such as

* cheetah-example-webapi
* cheetah-lib-templates

features offered by this library:

* Prometheus exposed on a kestrel server
* Helper methods for Authentication.
* Helper methods for building opensearch-search indices

## Running tests locally

To run unit tests locally run:
```sh
dotnet test --filter 'FullyQualifiedName!~Integration'
```

To run unit tests locally you need OpenSearch running on port 9200.
To run integration tests locally run:
```sh
dotnet test --filter 'FullyQualifiedName~Integration'
```

## Writing tests

Writing unit tests has nothing specific to it.

Writing integration tests has the following requirements:
- The test function must contain 'Integration' in its name (usually as a suffix)
- The test function must set-up its own data and tear it down afterwards
- Integration tests for OpenSearch require an OS instance running on port 9200

## Naming Strategies

In order to store data in ES, you need an Index (Index is the ES counterpart to a Database in the SQL world).
ES can scale to very large volumes of storage, but we have to keep in mind that different underlying storage
has different costs associated with it. e.g. SSD discs are faster than HDDs, but are more costly.

We are providing a number of different naming strategies for Indexes:

The `<>` indicates required param, while `[]` indicates optional. e.g `prefix` is always optional.
- `SimpleIndexNamingStrategy`: follows the pattern `<base>_[prefix]`.
    This is the simplest Index naming
- `CustomerIndexNamingStrategy`: follows the pattern `<base>_[prefix]_<customer>_*`.
    (For querying) This gives us all the Indexes for a customer - all years/months
- `YearResolutionIndexNamingStrategy`: follows the pattern `<base>_[prefix]_<customer>_<year>`.
    This builds on top of the `CustomerIndexNamingStrategy` but adds sharding based on the year
- `MonthResolutionIndexNamingStrategy`: follows the pattern `<base>_[prefix]_<customer>_<year>_<zero-padded month>`.
    This builds on top of the `YearResolutionIndexNamingStrategy` but adds sharding based on month as well.
- `YearResolutionWithWildcardIndexNamingStrategy`: follows the pattern `<base>_[prefix]_<customer>_<year>*`.
    (For querying) This gives us all the Indexes a given customer and year - all the months

### Hot and Cold Storage and Sharding
Usually, we refer to the underlying storage media as Hot or Cold storage.
Hot storage using faster hardware, but is more expensive.
Cold storage is slower, but cheaper.

To give some numbers for better understanding why we have this separation: Cold storage can be 5 times cheaper
than Hot storage, but it will also probably be 10-20 times slower to query.

Depending on your needs you might need to query the data for the past month frequently, but anything older than
that you query once a month. Therefore, it would make sense to save money by archiving data older than 1 month
in a Cold storage. We can easily achieve this if our data is separated in different shards (databases) based
on the month. i.e. each shard stores data for 1 month, so data for April and June will be stored in two different
shards. When a shard becomes more than a month old, we can archive it into cold storage.
