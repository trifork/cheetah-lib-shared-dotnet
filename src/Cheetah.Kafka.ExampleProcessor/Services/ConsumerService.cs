using Cheetah.Kafka.ExampleProcessor.Models;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cheetah.Kafka.ExampleProcessor.Services;

public class AConsumerService : ConsumerService
{
    public AConsumerService(
        ILogger<ConsumerService> logger,
        [FromKeyedServices("A")] IConsumer<string, ExampleModelAvro> consumer
    )
        : base(logger, consumer) { }

    protected override void LogMessage(ExampleModelAvro message)
    {
        Logger.LogInformation(
            $"Received message in A: {message.id} {message.value} {message.timestamp}"
        );
    }
}

public class BConsumerService : ConsumerService
{
    public BConsumerService(
        ILogger<ConsumerService> logger,
        [FromKeyedServices("B")] IConsumer<string, ExampleModelAvro> consumer
    )
        : base(logger, consumer) { }

    protected override void LogMessage(ExampleModelAvro message)
    {
        Logger.LogInformation(
            $"Received message in B: {message.id} {message.value} {message.timestamp}"
        );
    }
}

public abstract class ConsumerService : BackgroundService
{
    protected ILogger<ConsumerService> Logger { get; }
    protected IConsumer<string, ExampleModelAvro> Consumer { get; }

    protected abstract void LogMessage(ExampleModelAvro message);

    public ConsumerService(
        ILogger<ConsumerService> logger,
        IConsumer<string, ExampleModelAvro> consumer
    )
    {
        Logger = logger;
        Consumer = consumer;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            return Task.Run(
                () =>
                {
                    Consumer.Assign(new TopicPartition(Constants.TopicName, 0));
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var result = Consumer.Consume(stoppingToken);
                        LogMessage(result.Message.Value);
                        Consumer.Commit(result);
                    }
                },
                stoppingToken
            );
        }
        catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
        {
            // Ignore
        }

        return Task.CompletedTask;
    }
}
