using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Cheetah.Auth.Authentication;
using Cheetah.Auth.Util;
using Cheetah.Kafka.Configuration;
using Cheetah.Kafka.Extensions;
using Confluent.Kafka;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Kafka.Testing
{
    /// <summary>
    /// A factory for creating <see cref="IKafkaTestReader{T}"/> and <see cref="IKafkaTestWriter{TKey,T}"/> instances.
    /// </summary>
    /// <remarks>
    /// This should only be used for testing purposes.
    /// </remarks>
    public class KafkaTestClientFactory
    {
        /// <summary>
        /// The internal <see cref="KafkaClientFactory"/> used to create Writers and Readers.
        /// </summary>
        /// <remarks>
        /// This property is exposed to allow more advanced functionality than <see cref="KafkaTestClientFactory"/> and the clients it creates
        /// provides and should <b>NOT</b> be used in production to generate actual clients. Use <see cref="ServiceCollectionExtensions.AddCheetahKafka"/> instead.
        /// </remarks>
        public KafkaClientFactory ClientFactory { get; }

        /// <summary>
        /// Creates a <see cref="KafkaTestClientFactory"/> using the provided <see cref="IConfiguration"/> instance
        /// </summary>
        /// <param name="configuration">The configuration to use</param>
        /// <param name="tokenService">An optional token service, used to retrieve access tokens</param>
        /// <param name="loggerFactory">An optional logger factory</param>
        /// <param name="options">An optional </param>
        /// <returns></returns>
        public static KafkaTestClientFactory Create(
            IConfiguration configuration,
            KafkaClientFactoryOptions? options = null,
            ITokenService? tokenService = null,
            ILoggerFactory? loggerFactory = null
        )
        {
            var config = new KafkaConfig();
            configuration.Bind(KafkaConfig.Position, config);
            return Create(config, options, tokenService, loggerFactory);
        }

        /// <inheritdoc cref="Create(Microsoft.Extensions.Configuration.IConfiguration,Cheetah.Kafka.KafkaClientFactoryOptions?,Cheetah.Auth.Authentication.ITokenService?,Microsoft.Extensions.Logging.ILoggerFactory?)"/>
        public static KafkaTestClientFactory Create(
            KafkaConfig configuration,
            KafkaClientFactoryOptions? options = null,
            ITokenService? tokenService = null,
            ILoggerFactory? loggerFactory = null
        )
        {
            var config = Options.Create(configuration);
            options ??= new KafkaClientFactoryOptions();
            loggerFactory ??= LoggerFactory.Create(builder => builder.AddConsole());

            tokenService ??= new CachedTokenProvider(
                new OAuthTokenProvider(Options.Create(configuration.OAuth2), new DefaultHttpClientFactory(),
                    "kafka-test-client"),
                loggerFactory.CreateLogger<CachedTokenProvider>());

            tokenService.FetchTokenAsync();
            
            var clientFactory = new KafkaClientFactory(
                tokenService,
                loggerFactory,
                config,
                options
            );
            return new KafkaTestClientFactory(clientFactory);
        }

        private KafkaTestClientFactory(KafkaClientFactory clientFactory)
        {
            ClientFactory = clientFactory;
        }

        /// <summary>
        /// Creates an <see cref="IKafkaTestWriter{Null, T}"/> for the provided topic. This writer will not produce keys.
        /// </summary>
        /// <param name="topic">The topic to produce messages to</param>
        /// <typeparam name="T">The type of messages to produce</typeparam>
        /// <returns>The created <see cref="IKafkaTestWriter{Null,T}"/></returns>
        /// <exception cref="ArgumentException">Thrown if the provided topic is invalid</exception>
        public IKafkaTestWriter<Null, T> CreateTestWriter<T>(string topic)
        {
            return CreateTestWriter<Null, T>(topic, _ => null!);
        }

        /// <summary>
        /// Creates an <see cref="IKafkaTestWriter{TKey, T}"/> for the provided topic.
        /// </summary>
        /// <param name="topic">The topic to produce messages to</param>
        /// <param name="keyFunction">A function which produces a key for the provided message</param>
        /// <typeparam name="TKey">The type of key to produce</typeparam>
        /// <typeparam name="T">The type of messages to produce</typeparam>
        /// <returns>The created <see cref="IKafkaTestWriter{TKey,T}"/></returns>
        /// <exception cref="ArgumentException">Thrown if the provided topic is invalid</exception>
        public IKafkaTestWriter<TKey, T> CreateTestWriter<TKey, T>(
            string topic,
            Func<T, TKey> keyFunction
        )
        {
            ValidateTopic(topic);
            var producer = ClientFactory.CreateProducer<TKey, T>();
            return new KafkaTestWriter<TKey, T>(producer, keyFunction, topic);
        }

        /// <inheritdoc cref="CreateTestWriter{T}"/>
        /// <summary>
        /// Creates an <see cref="IKafkaTestWriter{Null, T}"/> for the provided topic, which serializes messages using Avro. This writer will not produce keys.
        /// </summary>
        public IKafkaTestWriter<Null, T> CreateAvroTestWriter<T>(string topic)
        {
            return CreateAvroTestWriter<Null, T>(topic, _ => null!);
        }

        /// <inheritdoc cref="CreateTestWriter{TKey, T}"/>
        /// <summary>
        /// Creates an <see cref="IKafkaTestWriter{TKey, T}"/> for the provided topic, which serializes messages using Avro.
        /// </summary>
        public IKafkaTestWriter<TKey, T> CreateAvroTestWriter<TKey, T>(
            string topic,
            Func<T, TKey> keyFunction
        )
        {
            ValidateTopic(topic);
            var producer = ClientFactory.CreateAvroProducer<TKey, T>();
            return new KafkaTestWriter<TKey, T>(producer, keyFunction, topic);
        }

        /// <summary>
        /// Creates an <see cref="IKafkaTestReader{T}"/> for the provided topic. This reader will not read keys.
        /// </summary>
        /// <param name="topic">The topic to read messages from. </param>
        /// <param name="groupId">Optional group id to use. Defaults to a random Guid.</param>
        /// <typeparam name="T">The type of message to read</typeparam>
        /// <returns>The created <see cref="IKafkaTestReader{T}"/></returns>
        public IKafkaTestReader<T> CreateTestReader<T>(string topic, string? groupId = null)
        {
            return CreateTestReader<Null, T>(topic, groupId);
        }

        /// <summary>
        /// Creates an <see cref="IKafkaTestReader{T}"/> for the provided topic.
        /// </summary>
        /// <param name="topic">The topic to read messages from</param>
        /// <param name="groupId">Optional group id to use. Defaults to a random Guid.</param>
        /// <typeparam name="TKey">The type of key to read</typeparam>
        /// <typeparam name="T">The type of message to read</typeparam>
        /// <returns>The created <see cref="IKafkaTestReader{T}"/></returns>
        public IKafkaTestReader<T> CreateTestReader<TKey, T>(string topic, string? groupId = null)
        {
            ValidateTopic(topic);
            groupId ??= Guid.NewGuid().ToString();

            var consumer = ClientFactory.CreateConsumer<TKey, T>(
                DefaultReaderConfiguration(groupId)
            );
            return new KafkaTestReader<TKey, T>(consumer, topic);
        }

        /// <inheritdoc cref="CreateTestReader{T}"/>
        /// <summary>
        /// Creates an <see cref="IKafkaTestReader{T}"/> for the provided topic, which deserializes messages using Avro. This reader will not read keys.
        /// </summary>
        public IKafkaTestReader<T> CreateAvroTestReader<T>(string topic, string? groupId = null)
        {
            return CreateAvroTestReader<Null, T>(topic, groupId);
        }

        /// <inheritdoc cref="CreateTestReader{TKey, T}"/>
        /// <summary>
        /// Creates an <see cref="IKafkaTestReader{T}"/> for the provided topic, which deserializes messages using Avro.
        /// </summary>
        public IKafkaTestReader<T> CreateAvroTestReader<TKey, T>(
            string topic,
            string? groupId = null
        )
        {
            ValidateTopic(topic);
            groupId ??= Guid.NewGuid().ToString();

            var consumer = ClientFactory.CreateAvroConsumer<TKey, T>(
                DefaultReaderConfiguration(groupId)
            );
            return new KafkaTestReader<TKey, T>(consumer, topic);
        }

        private static Action<ConsumerConfig> DefaultReaderConfiguration(string groupId)
        {
            return cfg =>
            {
                cfg.GroupId = groupId;
                cfg.AutoOffsetReset = AutoOffsetReset.Latest;
                cfg.EnablePartitionEof = true;
                cfg.AllowAutoCreateTopics = true;
            };
        }

        private static void ValidateTopic(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                throw new ArgumentException("A topic must be provided");
            }

            var hasOnlyValidCharacters = Regex.Match(topic, "^[a-zA-Z0-9\\._\\-]+$");
            if (!hasOnlyValidCharacters.Success)
            {
                throw new ArgumentException(
                    $"Received topic with invalid characters '{topic}'. Topic names can only contain alphanumeric characters, '.', '-' and '_'."
                );
            }

            if (topic.Length > 249)
            {
                throw new ArgumentException("Topic names cannot exceed 249 characters");
            }
        }
    }
}
