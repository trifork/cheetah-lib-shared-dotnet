# Testing with Cheetah.SchemaRegistry

In order to easily test other components which communicate with Kafka using Avro, Cheetah.SchemaRegistry includes a simple way to generate test clients that can be used to verify the behavior of other components.

The usage the same as [Testing with Cheetah.Kafka](../../Cheetah.Kafka/v2.1/TestingWithCheetahKafka.md) - only differing in setup.

Additional configuration described [here](./UsingCheetahSchemaRegistry.md#configuration) is required.

Otherwise you only need to exchange `KafkaTestClientFactory` with `AvroKafkaTestClientFactory`.