# Cheetah.OpenSearch.ExampleAPI

This API is only meant to be used as a very simple usage example of a reference for how to use `Cheetah.OpenSearch`.

It registers an `IOpenSearchClient` through dependency injection, using the `AddCheetahOpenSearch` extension method and injects it in `/Controllers/IndexController.cs`.
It also contains necessary configuration for OpenSearch in `appsettings.Development.json`.