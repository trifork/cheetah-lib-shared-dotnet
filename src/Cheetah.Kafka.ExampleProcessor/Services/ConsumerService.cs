using Cheetah.Kafka.ExampleProcessor.Models;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenSearch.Client;

namespace Cheetah.Kafka.ExampleProcessor.Services
{
    public class ConsumerService : BackgroundService
    {
        protected IOpenSearchClient OpensearchClient { get; }
        protected ILogger<ConsumerService> Logger { get; }
        protected IConsumer<string, ExampleModelAvro> Consumer { get; }

        public ConsumerService(
            IOpenSearchClient client,
            ILogger<ConsumerService> logger,
            IConsumer<string, ExampleModelAvro> consumer
        )
        {
            OpensearchClient = client;
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
                            IndexAndCommitAsync(result);
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

        private Task IndexAndCommitAsync(ConsumeResult<string, ExampleModelAvro> result)
        {
            return Task.WhenAll(
                OpensearchClient.IndexAsync(result.Message.Value, i => i.Index("test-index")),
                Task.Run(() => Consumer.Commit(result))
            );
        }
    }
}
