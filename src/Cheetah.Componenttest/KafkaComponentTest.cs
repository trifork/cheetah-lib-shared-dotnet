using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cheetah.Core.Config;
using Cheetah.Core.Infrastructure.Auth;
using Cheetah.Core.Infrastructure.Services.Kafka;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Cheetah.ComponentTest
{
    /// <summary>
    /// <typeparam name="TIn">The type of the value of the produced message</typeparam>
    /// <typeparam name="TOut">The type of the value of the consumed message</typeparam>
    /// </summary>
    public abstract class KafkaComponentTest<TIn, TOut> : KafkaComponentTest<TIn, Null, TOut, Null>
    {
        protected KafkaComponentTest(ILogger logger, IOptions<ComponentTestConfig> componentTestConfig, IOptions<KafkaConfig> kafkaConfig, CheetahKafkaTokenService tokenService) : base(logger, componentTestConfig, kafkaConfig, tokenService)
        {
        }
    }
    /// <summary>
    /// <typeparam name="TIn">The type of the value of the produced message</typeparam>
    /// <typeparam name="TOut">The type of the value of the consumed message</typeparam>
    /// <typeparam name="TInKey">The type of the value of the produced message</typeparam>
    /// <typeparam name="TOutKey">The type of the value of the consumed message</typeparam>
    /// </summary>
    public abstract class KafkaComponentTest<TIn, TInKey, TOut, TOutKey> : ComponentTest
    {
        private readonly ComponentTestConfig _componentTestConfig;
        private readonly KafkaConfig _kafkaConfig;
        private readonly ILogger _logger;
        private readonly CheetahKafkaTokenService _tokenService;
        private IProducer<TInKey, TIn>? _producer;
        private IConsumer<TOutKey, TOut>? _consumer;

        /// <summary>
        /// Number of messages to be expected to be produced by the tested job
        /// </summary>
        protected abstract int ExpectedResponseCount { get; }

        /// <summary>
        /// Time to wait for additional messages to be produced by the tested job
        /// Optional, Default value: 2 seconds.
        /// </summary>
        protected virtual TimeSpan WaitTimeAfterConsume { get; } = TimeSpan.FromSeconds(2);

        protected KafkaComponentTest(ILogger logger, IOptions<ComponentTestConfig> componentTestConfig, IOptions<KafkaConfig> kafkaConfig, CheetahKafkaTokenService tokenService)
        {
            _componentTestConfig = componentTestConfig.Value;
            _kafkaConfig = kafkaConfig.Value;
            _logger = logger;
            _tokenService = tokenService;
        }

        internal override Task Arrange(CancellationToken cancellationToken)
        {
            Log.Information("Preparing kafka consumer, consuming from topic '{topic}'", _componentTestConfig.ConsumerTopic);
            _consumer = new ConsumerBuilder<TOutKey, TOut>(new ConsumerConfig
            {
                BootstrapServers = _kafkaConfig.Url,
                GroupId = _componentTestConfig.ConsumerGroup,
                AllowAutoCreateTopics = true,
                EnablePartitionEof = true,
                SaslMechanism = SaslMechanism.OAuthBearer,
                SecurityProtocol = SecurityProtocol.SaslPlaintext
            })
              .SetValueDeserializer(new Utf8Serializer<TOut>())
              .AddCheetahOAuthentication(_tokenService, _logger)
              .Build();

            _consumer.Assign(new TopicPartitionOffset(_componentTestConfig.ConsumerTopic, 0, Offset.End));

            while (!cancellationToken.IsCancellationRequested)
            {
                var consumeResult = _consumer.Consume(cancellationToken);
                if (consumeResult.IsPartitionEOF)
                {
                    break;
                }
            }

            Log.Information("Preparing kafka producer, producing to topic '{topic}'", _componentTestConfig.ProducerTopic);
            _producer = new ProducerBuilder<TInKey, TIn>(new ProducerConfig
            {
                BootstrapServers = _kafkaConfig.Url,
                SaslMechanism = SaslMechanism.OAuthBearer,
                SecurityProtocol = SecurityProtocol.SaslPlaintext
            })
            .SetValueSerializer(new Utf8Serializer<TIn>())
            .AddCheetahOAuthentication(_tokenService, _logger)
            .Build();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Define messages to publish to Kafka
        /// </summary>
        /// <returns>IEnumerable of class to be produced</returns>
        protected abstract IEnumerable<TIn> GetMessagesToPublish();

        internal sealed override async Task Act(CancellationToken cancellationToken)
        {
            var messages = GetMessagesToPublish().ToList();
            if (_producer == null) throw new ArgumentException("Arrange has not been called");

            foreach (var message in messages)
            {
                await _producer.ProduceAsync(
                    _componentTestConfig.ProducerTopic,
                    new Message<TInKey, TIn> { Value = message },
                    cancellationToken
                );
            }

            Log.Information($"Published {messages.Count} messages to Kafka");
        }

        private async Task<IEnumerable<TOut>> ConsumeMessages(CancellationToken cancellationToken)
        {
            var messages = new List<TOut>();

            Log.Information(
                "Consuming messages from '{topic}', expecting a total of {count} messages...",
                _componentTestConfig.ConsumerTopic,
                ExpectedResponseCount
            );
            if (_consumer == null) throw new ArgumentException("Arrange has not been called");
            while (
                messages.Count < ExpectedResponseCount && !cancellationToken.IsCancellationRequested
            )
            {
                var consumeResult = _consumer.Consume(cancellationToken);

                if (consumeResult.IsPartitionEOF)
                {
                    continue;
                }

                messages.Add(consumeResult.Message.Value);
            }
            Log.Information(
                "Successfully consumed {messageCount} messages from Kafka.",
                messages.Count
            );

            Log.Information(
                "Waiting {waitTimeAfterConsume}, then checking for any additional, unexpected messages...",
                WaitTimeAfterConsume
            );
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

            Log.Information(
                "Found {additionalMessageCount} additional messages!",
                additionalMessages.Count
            );
            if (additionalMessages.Any())
            {
                // TODO: Find or create a proper exception type for this.
                throw new Exception(
                    $"Received a total of {messages.Count + additionalMessages.Count} messages, but only expected {ExpectedResponseCount}"
                );
            }

            _consumer.Close();
            _consumer.Dispose();

            return messages;
        }

        internal sealed override async Task<TestResult> Assert(CancellationToken cancellationToken)
        {
            var messages = await ConsumeMessages(cancellationToken);
            return ValidateResult(messages);
        }

        /// <summary>
        /// Define how to validate results from Kafka
        /// </summary>
        /// <param name="result">IEnumerable of the consumed messages</param>
        /// <returns></returns>
        protected abstract TestResult ValidateResult(IEnumerable<TOut> result);
    }
}
