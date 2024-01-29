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
        [FromKeyedServices("A")] IConsumer<string, ExampleModel> consumer
    )
        : base(logger, consumer) { }

    protected override void LogMessage(ExampleModel message)
    {
        Logger.LogInformation(
            $"Received message in A: {message.Id} {message.Value} {message.Timestamp}"
        );
    }
}

public class BConsumerService : ConsumerService
{
    public BConsumerService(
        ILogger<ConsumerService> logger,
        [FromKeyedServices("B")] IConsumer<string, ExampleModel> consumer
    )
        : base(logger, consumer) { }

    protected override void LogMessage(ExampleModel message)
    {
        Logger.LogInformation(
            $"Received message in B: {message.Id} {message.Value} {message.Timestamp}"
        );
    }
}

public abstract class ConsumerService : BackgroundService
{
    protected ILogger<ConsumerService> Logger { get; }
    protected IConsumer<string, ExampleModel> Consumer { get; }

    protected abstract void LogMessage(ExampleModel message);

    public ConsumerService(
        ILogger<ConsumerService> logger,
        IConsumer<string, ExampleModel> consumer
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
