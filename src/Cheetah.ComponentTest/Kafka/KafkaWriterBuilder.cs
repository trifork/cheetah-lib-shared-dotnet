using System;
using Cheetah.Auth.Authentication;
using Cheetah.Kafka;
using Cheetah.Kafka.Util;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.Configuration;

namespace Cheetah.ComponentTest.Kafka
{
    /// <summary>
    /// Static utility class used to create new instances of <see cref="KafkaWriterBuilder{TKey, T}"/>
    /// </summary>
    public static class KafkaWriterBuilder
    {
        /// <summary>
        /// Creates a new <see cref="KafkaWriterBuilder{TKey, T}"/>
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
        /// <typeparam name="T">The message type that the writer will produce</typeparam>
        /// <returns>A new <see cref="KafkaWriterBuilder{TKey, T}"/></returns>
        public static KafkaWriterBuilder<TKey, T> Create<TKey, T>(IConfiguration configuration)
        {
            return new KafkaWriterBuilder<TKey, T>(configuration);
        }

        /// <summary>
        /// Creates a new <see cref="KafkaWriterBuilder{Null, T}"/>. The resulting writer will publish messages without a key.
        /// </summary>
        /// <param name="configuration">An <see cref="IConfiguration"/> to extract necessary configuration from.</param>
        /// <typeparam name="T">The message type that the writer will produce</typeparam>
        /// <returns>A new <see cref="KafkaWriterBuilder{Null, T}"/></returns>
        public static KafkaWriterBuilder<Null, T> Create<T>(IConfiguration configuration)
        {
            return Create<Null, T>(configuration).WithKeyFunction(_ => null!);
        }
    }

    /// <summary>
    /// Builder used for building a <see cref="KafkaWriter{TKey, T}"/>.
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
    /// <typeparam name="TKey">The type of key to use for the message. Must be a value type.</typeparam>
    /// <typeparam name="T">The message type that the writer will produce</typeparam>
    public class KafkaWriterBuilder<TKey, T> : KafkaBuilderBase
    {
        private Func<T, TKey>? KeyFunction { get; set; }
        internal KafkaWriterBuilder(IConfiguration configuration) : base(configuration)
        {
        }

        /// <summary>
        /// Sets the topic the writer will produce messages to
        /// </summary>
        /// <remarks>Allowed characters are all alphanumeric characters, '.', '-', and '_'. Length cannot exceed 249 characters.</remarks>
        /// <param name="topic">The topic that the writer should produce to</param>
        /// <returns>The <see cref="KafkaWriterBuilder{TKey, T}"/> for method chaining.</returns>
        public KafkaWriterBuilder<TKey, T> WithTopic(string topic)
        {
            Topic = topic;
            return this;
        }

        /// <summary>
        /// Sets the key function that the writer will use to extract the key from the message.
        /// </summary>
        /// <param name="keyFunction">A function that extracts an appropriate <c>TKey</c> from a given <c>T</c></param>
        /// <returns>The <see cref="KafkaWriterBuilder{TKey, T}"/> for method chaining.</returns>
        public KafkaWriterBuilder<TKey, T> WithKeyFunction(Func<T, TKey> keyFunction)
        {
            KeyFunction = keyFunction;
            return this;
        }

        /// <summary>
        /// Specifies that the resulting writer should use AVRO serialization.
        /// </summary>
        /// <remarks>
        /// Requires that the configuration key <c>KAFKA:SCHEMAREGISTRYURL</c> is set or that an appropriate <see cref="SchemaRegistryConfig"/> is provided.
        /// </remarks>
        /// <param name="config">Optional configuration for schema registry</param>
        /// <returns>The <see cref="KafkaWriterBuilder{TKey, T}"/> for method chaining.</returns>
        public KafkaWriterBuilder<TKey, T> UsingAvro(SchemaRegistryConfig? config = null)
        {
            UsingAvroInternal(config);
            return this;
        }

        /// <summary>
        /// Builds a <see cref="KafkaWriter{TKey, T}"/> based on the methods called on the builder and the provided configuration.
        /// </summary>
        /// <remarks><c>WithTopic(string)</c> and <c>WithKeyFunction(Func&lt;T, TKey&gt;)</c> must have been called prior to this.</remarks>
        /// <returns>The built <see cref="KafkaWriter{TKey, T}"/></returns>
        public IKafkaWriter<T> Build()
        {
            ValidateInput();

            var tokenService = GetTokenService();
            var writerProps = new KafkaWriterProps<TKey, T>
            {
                Topic = Topic,
                KeyFunction = KeyFunction,
                KafkaUrl = Configuration.GetValue<string>(KAFKA_URL_KEY),
                TokenService = tokenService,
                Serializer = IsAvro
                    ? GetAvroSerializer(tokenService)
                    : new Utf8Serializer<T>(),
            };

            return new KafkaWriter<TKey, T>(writerProps);
        }

        private ISerializer<T> GetAvroSerializer(ITokenService tokenService)
        {
            var authHeaderValueProvider = new OAuthHeaderValueProvider(tokenService);
            var schemaRegistryClient = new CachedSchemaRegistryClient(SchemaRegistryConfig, authHeaderValueProvider);
            return new AvroSerializer<T>(schemaRegistryClient).AsSyncOverAsync();
        }
    }
}
