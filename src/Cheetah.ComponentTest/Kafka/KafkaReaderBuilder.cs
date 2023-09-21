using Cheetah.Core.Infrastructure.Auth;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.Configuration;

namespace Cheetah.ComponentTest.Kafka
{
    /// <summary>
    /// Static utility class used to create new instances of <see cref="KafkaReaderBuilder{TKey, T}"/>
    /// </summary>
    public static class KafkaReaderBuilder
    {
        /// <summary>
        /// Creates a new <see cref="KafkaReaderBuilder{TKey, T}"/>
        /// </summary>
        /// <remarks>
        /// <para>
        /// Retrieves necessary configuration from the provided <see cref="IConfiguration"/>
        /// </para>
        /// <para>
        /// Requires the following keys to be set in the provided configuration:
        /// <list type="table">
        ///     <listheader>
        ///        <term>Key</term>
        ///        <description>Description</description>
        ///     </listheader>
        ///     <item>
        ///         <term><c>KAFKA:URL</c></term>
        ///         <description>Required - The URL where Kafka can be reached. Must not contain any scheme prefix.</description>
        ///     </item>
        ///     <item>
        ///         <term><c>KAFKA:CLIENTID</c></term>
        ///         <description>Required - The clientId to use when authenticating towards Kafka</description>
        ///     </item>
        ///     <item>
        ///         <term><c>KAFKA:CLIENTSECRET</c></term>
        ///         <description>Required - The client secret to use when authenticating towards Kafka</description>
        ///     </item>
        ///     <item>
        ///         <term><c>KAFKA:AUTHENDPOINT</c></term>
        ///         <description>Required - The endpoint to retrieve authentication tokens from</description>
        ///     </item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <param name="configuration">An <see cref="IConfiguration"/> to extract necessary configuration from.</param>
        /// <typeparam name="TKey">The type of key to use for the message, must be a value type.</typeparam>
        /// <typeparam name="T">The message type that the reader will consume</typeparam>
        /// <returns>A new <see cref="KafkaReaderBuilder{TKey, T}"/></returns>
        public static KafkaReaderBuilder<TKey, T> Create<TKey, T>(IConfiguration configuration)
        {
            return new KafkaReaderBuilder<TKey, T>(configuration);
        }

        /// <summary>
        /// Creates a new <see cref="KafkaReaderBuilder{Null, T}"/>. The resulting reader will consume messages without requiring a key to be specified.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Retrieves necessary configuration from the provided <see cref="IConfiguration"/>
        /// </para>
        /// <para>
        /// Requires the following keys to be set in the provided configuration:
        /// <list type="table">
        ///     <listheader>
        ///        <term>Key</term>
        ///        <description>Description</description>
        ///     </listheader>
        ///     <item>
        ///         <term><c>KAFKA:URL</c></term>
        ///         <description>Required - The URL where Kafka can be reached. Must not contain any scheme prefix.</description>
        ///     </item>
        ///     <item>
        ///         <term><c>KAFKA:CLIENTID</c></term>
        ///         <description>Required - The clientId to use when authenticating towards Kafka</description>
        ///     </item>
        ///     <item>
        ///         <term><c>KAFKA:CLIENTSECRET</c></term>
        ///         <description>Required - The client secret to use when authenticating towards Kafka</description>
        ///     </item>
        ///     <item>
        ///         <term><c>KAFKA:AUTHENDPOINT</c></term>
        ///         <description>Required - The endpoint to retrieve authentication tokens from</description>
        ///     </item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <param name="configuration">An <see cref="IConfiguration"/> to extract necessary configuration from.</param>
        /// <typeparam name="T">The message type that the reader will consume</typeparam>
        /// <returns>A new <see cref="KafkaReaderBuilder{Null, T}"/></returns>
        public static KafkaReaderBuilder<Null, T> Create<T>(IConfiguration configuration)
        {
            return Create<Null, T>(configuration);
        }
    }

    /// <summary>
    /// Builder used for building a <see cref="KafkaReader{TKey, T}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of key to use for the message. Must be a value type.</typeparam>
    /// <typeparam name="T">The message type that the reader will consume</typeparam>
    public class KafkaReaderBuilder<TKey, T> : KafkaBuilderBase
    {
        string? ConsumerGroup { get; set; }

        internal KafkaReaderBuilder(IConfiguration configuration) : base(configuration)
        {
        }

        /// <summary>
        /// Sets the topic the reader will consume messages from
        /// </summary>
        /// <remarks>Allowed characters are all alphanumeric characters, '.', '-', and '_'. Length cannot exceed 249 characters.</remarks>
        /// <param name="topic">The topic that the reader should consume from</param>
        /// <returns>The <see cref="KafkaReaderBuilder{TKey, T}"/> for method chaining.</returns>
        public KafkaReaderBuilder<TKey, T> WithTopic(string topic)
        {
            Topic = topic;
            return this;
        }

        /// <summary>
        /// Sets the consumer group that the reader should be part of
        /// </summary>
        /// <param name="consumerGroup">The consumer group that reader should be part of</param>
        /// <returns>The <see cref="KafkaReaderBuilder{TKey, T}"/> for method chaining.</returns>
        public KafkaReaderBuilder<TKey, T> WithConsumerGroup(string consumerGroup)
        {
            ConsumerGroup = consumerGroup;
            return this;
        }

        /// <summary>
        /// Specifies that the resulting reader should use AVRO deserialization.
        /// </summary>
        /// <remarks>
        /// Requires that the configuration key <c>KAFKA:SCHEMAREGISTRYURL</c> is set or that an appropriate <see cref="SchemaRegistryConfig"/> is provided.
        /// </remarks>
        /// <param name="config">Optional configuration for schema registry</param>
        /// <returns>The <see cref="KafkaReaderBuilder{TKey, T}"/> for method chaining.</returns>
        public KafkaReaderBuilder<TKey, T> UsingAvro(SchemaRegistryConfig? config = null)
        {
            UsingAvroInternal(config);
            return this;
        }

        /// <summary>
        /// Builds a <see cref="KafkaReader{TKey, T}"/> based on the methods called on the builder and the provided configuration.
        /// </summary>
        /// <remarks><c>WithTopic(string)</c> and <c>WithConsumerGroup(string)</c> must have been called prior to this.</remarks>
        /// <returns>The built <see cref="KafkaReader{TKey, T}"/></returns>
        public KafkaReader<TKey, T> Build()
        {
            ValidateInput();
         
            var tokenService = GetTokenService();
            
            var props = new KafkaReaderProps<T>()
            {
                ConsumerGroup = ConsumerGroup,
                Topic = Topic,
                Deserializer = IsAvro 
                    ? GetAvroDeserializer(tokenService) 
                    : new Utf8Serializer<T>(),
                KafkaUrl = Configuration.GetValue<string>(KAFKA_URL_KEY),
                TokenService = tokenService
            };

            return new KafkaReader<TKey, T>(props);
        }
        
        private IDeserializer<T> GetAvroDeserializer(ITokenService tokenService)
        {
            var authHeaderValueProvider = new OAuthHeaderValueProvider(tokenService);
            var schemaRegistryClient = new CachedSchemaRegistryClient(SchemaRegistryConfig, authHeaderValueProvider);
            return new AvroDeserializer<T>(schemaRegistryClient).AsSyncOverAsync();
        }
    }
}
