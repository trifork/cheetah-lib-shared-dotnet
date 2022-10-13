# Cheetah.Shared

This is an project containing shared functionallity for projects such as

* cheetah-example-webapi
* cheetah-lib-templates

features offered by this library:

* Prometheus exposed on an kestrel server
* Helper methods for Authentication.
* Helper methods for building elastic-search indicies

## Running tests locally

To run unit tests locally run:
```sh
dotnet test --filter 'FullyQualifiedName!~Integration'
```

To run unit tests locally you need ElasticSearch running on port 9200.
To run integration tests locally run:
```sh
dotnet test --filter 'FullyQualifiedName~Integration'
```

## Writing tests

Writing unit tests has nothing specific to it.

Writing integration tests has the following requirements:
- The test function must contain 'Integration' in its name (usually as a suffix)
- The test function must set-up its own data and tear it down afterwards
- Integration tests for ElasticSearch require an ES instance running on port 9200
