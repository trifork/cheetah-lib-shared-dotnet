# CheetahComponentTest

This library provide `KafkaComponentTest`, an extendable class containing logic for publishing and consuming data from topics, specefied in `KafkaConfiguration`.

`KafkaConfiguration` comes with some default values, but require 3 parameter:

- BootstrapServer
- ConsumerTopic
- ProducerTopic

`BootstrapServer` is the server for kafka.

`ConsumerTopic` is the topic the `KafkaComponentTest` comsume from.

`ProducerTopic` is the topic the `KafkaComponentTest` produce to.

```c#
# Create testrunner
await new ComponentTestRunner()
    .AddTest<ExampleTest>()
    .AddTest<AnotherExampleTest>()
    .WithConfiguration<KafkaConfiguration>(KafkaConfiguration.Position)
    .RunAsync(args);
```

# Running test in docker compose

```bash
# Example for docker compose componentest
example-component-test:
    build:
      context: .
      dockerfile: MappingJobTest/Dockerfile
    environment:
      KAFKA__BOOTSTRAPSERVER: kafka_Server:Port_number
      KAFKA__CONSUMERTOPIC: ConsumerTopic
      KAFKA__PRODUCERTOPIC: ProducerTopic
    depends_on:
      kafka:
        condition: service_healthy
```