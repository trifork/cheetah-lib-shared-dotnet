using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Core;

namespace Common;

public abstract class KafkaComponentTest<TIn, TOut> : ComponentTest
{
    private readonly KafkaConfiguration _configuration;
    private IProducer<Null, TIn> _producer;
    private IConsumer<Null, TOut> _consumer;

    /// <summary>
    /// Number of messages to be expected to be produced by the tested job
    /// </summary>
    protected abstract int ExpectedResponseCount { get; }
    
    /// <summary>
    /// Time to wait for additional messages to be produced by the tested job
    /// Optional, Default value: 2 seconds.
    /// </summary>
    protected virtual TimeSpan WaitTimeAfterConsume { get; } = TimeSpan.FromSeconds(2);

    protected KafkaComponentTest(IOptions<KafkaConfiguration> configuration)
    {
        _configuration = configuration.Value;
    }
    
    internal override Task Arrange(CancellationToken cancellationToken)
    {
        Log.Information("Preparing kafka consumer, consuming from topic '{topic}'", _configuration.ConsumerTopic);
        _consumer = new ConsumerBuilder<Null, TOut>(new ConsumerConfig
        {
            BootstrapServers = _configuration.BootstrapServer,
            GroupId = _configuration.ConsumerGroup,
            AllowAutoCreateTopics = true,
            EnablePartitionEof = true
        })
            .SetValueDeserializer(new Utf8Serializer<TOut>())
            .Build();

        _consumer.Assign(new TopicPartitionOffset(_configuration.ConsumerTopic, 0, Offset.End));

        while (!cancellationToken.IsCancellationRequested)
        {
            var consumeResult = _consumer.Consume(cancellationToken);
            if (consumeResult.IsPartitionEOF)
            {
                break;
            }
        }
        
        Log.Information("Preparing kafka producer, producing to topic '{topic}'", _configuration.ProducerTopic);
        _producer = new ProducerBuilder<Null, TIn>(new ProducerConfig
        {
            BootstrapServers = _configuration.BootstrapServer
        }).SetValueSerializer(new Utf8Serializer<TIn>()).Build();

        return Task.CompletedTask;
    }

    protected abstract IEnumerable<TIn> GetMessagesToPublish();

    internal sealed override async Task Act(CancellationToken cancellationToken)
    {
        var messages = GetMessagesToPublish().ToList();

        foreach (var message in messages)
        {
            await _producer.ProduceAsync(_configuration.ProducerTopic, new Message<Null, TIn> { Value = message }, cancellationToken);
        }

        Log.Information($"Published {messages.Count} messages to Kafka");
    }

    internal sealed override async Task<TestResult> Assert(CancellationToken cancellationToken)
    {
        var messages = await ConsumeMessages(cancellationToken);
        return ValidateResult(messages);
    }

    private async Task<IEnumerable<TOut>> ConsumeMessages(CancellationToken cancellationToken)
    {
        var messages = new List<TOut>();

        Log.Information("Consuming messages from '{topic}', expecting a total of {count} messages...", _configuration.ConsumerTopic, ExpectedResponseCount);
        while (messages.Count < ExpectedResponseCount && !cancellationToken.IsCancellationRequested)
        {
            var consumeResult = _consumer.Consume(cancellationToken);

            if (consumeResult.IsPartitionEOF)
            {
                continue;
            }
            
            messages.Add(consumeResult.Message.Value);
        }
        Log.Information("Successfully consumed {messageCount} messages from Kafka.", messages.Count);
        
        Log.Information("Waiting {waitTimeAfterConsume}, then checking for any additional, unexpected messages...", WaitTimeAfterConsume);
        await Task.Delay(WaitTimeAfterConsume, cancellationToken);
        
        var additionalMessages = new List<TOut>();
        while (!cancellationToken.IsCancellationRequested)
        {
            var consumeResult = _consumer.Consume(cancellationToken);

            if (consumeResult.IsPartitionEOF)
            {
                break;
            }
            
            additionalMessages.Add(consumeResult.Message.Value);
        }
        
        Log.Information("Found {additionalMessageCount} additional messages!", additionalMessages.Count);
        if (additionalMessages.Any())
        {
            // TODO: Find or create a proper exception type for this.
            throw new Exception($"Received a total of {messages.Count + additionalMessages.Count} messages, but only expected {ExpectedResponseCount}");
        }
        
        _consumer.Close();
        _consumer.Dispose();
        
        return messages;
    }

    protected abstract TestResult ValidateResult(IEnumerable<TOut> result);
}