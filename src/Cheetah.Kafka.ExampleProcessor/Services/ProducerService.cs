using Cheetah.Kafka.ExampleProcessor.Models;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cheetah.Kafka.ExampleProcessor.Services
{
    public partial class ProducerService : BackgroundService
    {
        private readonly ILogger<ProducerService> _logger;
        private readonly IProducer<string, ExampleModelAvro> _producer;

        [LoggerMessage(Level = LogLevel.Information, Message = "Sending message: {messageId} {messageValue} {messageTimestamp}")]
        private static partial void LogSendingMessage(ILogger logger, string messageId, double messageValue, long messageTimestamp);

        public ProducerService(
            IProducer<string, ExampleModelAvro> producer,
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
                    var message = new ExampleModelAvro
                    {
                        id = Guid.NewGuid().ToString(),
                        value = new Random().Next(0, 100),
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    };

                    LogSendingMessage(_logger, message.id, message.value, message.timestamp);
                    _producer.Produce(
                        Constants.TopicName,
                        new Message<string, ExampleModelAvro> { Key = message.id, Value = message }
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
}
