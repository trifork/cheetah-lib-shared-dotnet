using System;
using System.Text.RegularExpressions;
using Cheetah.Auth.Authentication;
using Cheetah.Auth.Util;
using Cheetah.Kafka.Configuration;
using Cheetah.Kafka.Extensions;
using Cheetah.Kafka.Serdes;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Kafka.Testing
{
    /// <summary>
    /// A factory for creating <see cref="IKafkaTestReader{TKey, TValue}"/> and <see cref="IKafkaTestWriter{TKey,TValue}"/> instances.
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
        public static KafkaTestClientFactory Create(IConfiguration configuration,
            ClientFactoryOptions? options = null,
            ITokenService? tokenService = null,
            ILoggerFactory? loggerFactory = null)
        {
            var config = new KafkaConfig();
            configuration.Bind(KafkaConfig.Position, config);
            return Create(config, options, tokenService, loggerFactory);
        }

        /// <inheritdoc cref="Create(IConfiguration,ClientFactoryOptions?,Cheetah.Auth.Authentication.ITokenService?,Microsoft.Extensions.Logging.ILoggerFactory?)"/>
        public static KafkaTestClientFactory Create(
            KafkaConfig configuration,
            ClientFactoryOptions? options = null,
            ITokenService? tokenService = null,
            ILoggerFactory? loggerFactory = null,
            ISerializerProvider? serializerProvider = null,
            IDeserializerProvider? deserializerProvider = null)
        {
            var config = Options.Create(configuration);

            options ??= new ClientFactoryOptions();
            loggerFactory ??= LoggerFactory.Create(builder => builder.AddConsole());
            serializerProvider ??= new Utf8SerializerProvider();
            deserializerProvider ??= new Utf8DeserializerProvider();

            if (configuration.SaslMechanism == SaslMechanism.OAuthBearer)
            {
                tokenService ??= new CachedTokenProvider(configuration.OAuth2,
                    new OAuthTokenProvider(configuration.OAuth2, new DefaultHttpClientFactory()),
                    loggerFactory.CreateLogger<CachedTokenProvider>());

                tokenService.StartAsync();
            }

            var clientFactory = new KafkaClientFactory(
                tokenService,
                loggerFactory,
                config,
                options,
                serializerProvider,
                deserializerProvider
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
        /// <param name="valueSerializer">Optional valueSerializer. Defaults to valueSerializer from ISerializerProvider provided in the constructor</param>
        /// <typeparam name="T">The type of messages to produce</typeparam>
        /// <returns>The created <see cref="IKafkaTestWriter{Null,T}"/></returns>
        /// <exception cref="ArgumentException">Thrown if the provided topic is invalid</exception>
        public IKafkaTestWriter<Null, T> CreateTestWriter<T>(string topic, ISerializer<T>? valueSerializer = null)
        {
            return CreateTestWriter(topic, Serializers.Null, valueSerializer);
        }

        /// <summary>
        /// Creates an <see cref="IKafkaTestWriter{TKey, T}"/> for the provided topic.
        /// </summary>
        /// <param name="topic">The topic to produce messages to</param>
        /// <param name="keyFunction">A function which produces a key for the provided message</param>
        /// <param name="keySerializer">Optional keySerializer. Defaults to keySerializer from ISerializerProvider provided in the constructor</param>
        /// <param name="valueSerializer">Optional valueSerializer. Defaults to valueSerializer from ISerializerProvider provided in the constructor</param>
        /// <typeparam name="TKey">The type of key to produce</typeparam>
        /// <typeparam name="T">The type of messages to produce</typeparam>
        /// <returns>The created <see cref="IKafkaTestWriter{TKey,T}"/></returns>
        /// <exception cref="ArgumentException">Thrown if the provided topic is invalid</exception>
        [Obsolete("Using a keyFunction is deprecated, please use the method CreateTestWriter without keyFunction parameter, and the function WriteAsync(params Message<TKey, T>[] messages)")]
        public IKafkaTestWriter<TKey, T> CreateTestWriter<TKey, T>(
            string topic,
            Func<T, TKey> keyFunction,
            ISerializer<TKey>? keySerializer = null,
            ISerializer<T>? valueSerializer = null
        )
        {
            ValidateTopic(topic);
            ProducerOptionsBuilder<TKey, T> producerOptionsBuilder = new();
            if (keySerializer != null)
            {
                producerOptionsBuilder.SetKeySerializer(keySerializer);
            }
            if (valueSerializer != null)
            {
                producerOptionsBuilder.SetValueSerializer(valueSerializer);
            }
            var producer = ClientFactory.CreateProducer(producerOptionsBuilder.Build());

            return new KafkaTestWriter<TKey, T>(producer, keyFunction, topic);
        }


        /// <summary>
        /// Creates an <see cref="IKafkaTestWriter{TKey, T}"/> for the provided topic.
        /// </summary>
        /// <param name="topic">The topic to produce messages to</param>
        /// <param name="keySerializer">Optional keySerializer. Defaults to keySerializer from ISerializerProvider provided in the constructor</param>
        /// <param name="valueSerializer">Optional valueSerializer. Defaults to valueSerializer from ISerializerProvider provided in the constructor</param>
        /// <typeparam name="TKey">The type of key to produce</typeparam>
        /// <typeparam name="T">The type of messages to produce</typeparam>
        /// <returns>The created <see cref="IKafkaTestWriter{TKey,T}"/></returns>
        /// <exception cref="ArgumentException">Thrown if the provided topic is invalid</exception>
        public IKafkaTestWriter<TKey, T> CreateTestWriter<TKey, T>(
            string topic,
            ISerializer<TKey>? keySerializer = null,
            ISerializer<T>? valueSerializer = null
        )
        {
            ValidateTopic(topic);
            ProducerOptionsBuilder<TKey, T> producerOptionsBuilder = new();
            if (keySerializer != null)
            {
                producerOptionsBuilder.SetKeySerializer(keySerializer);
            }
            if (valueSerializer != null)
            {
                producerOptionsBuilder.SetValueSerializer(valueSerializer);
            }
            var producer = ClientFactory.CreateProducer(producerOptionsBuilder.Build());

            return new KafkaTestWriter<TKey, T>(producer, _ => default!, topic);
        }

        /// <summary>
        /// Creates an <see cref="IKafkaTestReader{Null, T}"/> for the provided topic. This reader will not read keys.
        /// </summary>
        /// <param name="topic">The topic to read messages from. </param>
        /// <param name="groupId">Optional group id to use. Defaults to a random Guid.</param>
        /// <param name="valueDeserializer">Optional valueDeserializer. Defaults to valueDeserializer from IDeserializerProvider provided in the constructor</param>
        /// <typeparam name="T">The type of message to read</typeparam>
        /// <returns>The created <see cref="IKafkaTestReader{Null, T}"/></returns>
        public IKafkaTestReader<Null, T> CreateTestReader<T>(string topic, string? groupId = null, IDeserializer<T>? valueDeserializer = null)
        {
            return CreateTestReader(topic, groupId, Deserializers.Null, valueDeserializer);
        }

        /// <summary>
        /// Creates an <see cref="IKafkaTestReader{TKey, TValue}"/> for the provided topic.
        /// </summary>
        /// <param name="topic">The topic to read messages from</param>
        /// <param name="groupId">Optional group id to use. Defaults to a random Guid.</param>
        /// <param name="keyDeserializer">Optional keyDeserializer. Defaults to keyDeserializer from IDeserializerProvider provided in the constructor</param>
        /// <param name="valueDeserializer">Optional valueDeserializer. Defaults to valueDeserializer from IDeserializerProvider provided in the constructor</param>
        /// <typeparam name="TKey">The type of key to read</typeparam>
        /// <typeparam name="TValue">The type of message to read</typeparam>
        /// <returns>The created <see cref="IKafkaTestReader{TKey, TValue}"/></returns>
        public IKafkaTestReader<TKey, TValue> CreateTestReader<TKey, TValue>(string topic, string? groupId = null, IDeserializer<TKey>? keyDeserializer = null, IDeserializer<TValue>? valueDeserializer = null)
        {
            ValidateTopic(topic);
            groupId ??= Guid.NewGuid().ToString();
            var consumerOptionsBuilder = new ConsumerOptionsBuilder<TKey, TValue>();
            consumerOptionsBuilder.ConfigureClient(DefaultReaderConfiguration(groupId));

            if (keyDeserializer != null)
            {
                consumerOptionsBuilder.SetKeyDeserializer(keyDeserializer);
            }
            if (valueDeserializer != null)
            {
                consumerOptionsBuilder.SetValueDeserializer(valueDeserializer);
            }

            var consumer = ClientFactory.CreateConsumer(
                consumerOptionsBuilder.Build()
            );
            return new KafkaTestReader<TKey, TValue>(consumer, topic);
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
