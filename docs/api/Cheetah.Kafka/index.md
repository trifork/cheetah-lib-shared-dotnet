# Cheetah.Kafka

This space contains API documentation relating to the `Cheetah.Kafka` nuget package.

It's primary purpose is to make it easy to set up and configure Kafka clients for use with the Cheetah data platform, as well as providing a consistent way to register clients during application startup and make them available for dependency injection in other services.

For more information on using the package in general, see the article [Using Cheetah.Kafka](../../articles/Cheetah.Kafka/UsingCheetahKafka.md)

This package relies heavily on the [Confluent.Kafka](https://docs.confluent.io/kafka-clients/dotnet/current/overview.html) nuget package. See their documentation for information on how to produce/consume messages with the generated clients. 
