# Cheetah.Kafka.ExampleProcessor

This application is only meant to be used as a very simple example for how to use `Cheetah.Kafka`.

It registers an `IKafkaClientFactory` through dependency injection, using the `AddCheetahKafkaClientFactory` extension method and injects it in `/ConsumerService.cs` and `/ProducerService.cs`.

These services use the client factory to create a Consumer and Producer, respectively, and use them to consume and produce messages.

It also contains necessary configuration for Kafka in `appsettings.Development.json`.