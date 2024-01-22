using Cheetah.Kafka.ExampleProcessor.Models;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cheetah.Kafka.ExampleProcessor.Services;

public class ProducerService : BackgroundService
{
    private readonly ILogger<ProducerService> _logger;
    private readonly IProducer<string, ExampleModel> _producer;

    public ProducerService(
        IProducer<string, ExampleModel> producer,
        ILogger<ProducerService> logger
    )
    {
        _logger = logger;
        _producer = producer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var message = new ExampleModel(
                    Guid.NewGuid().ToString(),
                    new Random().Next(0, 100),
                    DateTimeOffset.UtcNow
                );
                _logger.LogInformation(
                    $"Sending message: {message.Id} {message.Value} {message.Timestamp}"
                );
                _producer.Produce(
                    Constants.TopicName,
                    new Message<string, ExampleModel> { Key = message.Id, Value = message }
                );

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
        catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
        {
            // Ignore
        }
    }
}
