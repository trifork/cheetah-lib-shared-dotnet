using System;
using Cheetah.Core.Infrastructure.Auth;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.Configuration;

namespace Cheetah.ComponentTest.Kafka
{
    public class KafkaWriterBuilder
    {
        public static KafkaWriterBuilder<TKey, T> Create<TKey, T>(IConfiguration configuration)
        {
            return new KafkaWriterBuilder<TKey, T>(configuration);
        }

        public static KafkaWriterBuilder<T> Create<T>(IConfiguration configuration)
        {
            return new KafkaWriterBuilder<T>(configuration);
        }

        private KafkaWriterBuilder() { }
    }

    public class KafkaWriterBuilder<T> : KafkaWriterBuilder<Null, T>
    {
        public KafkaWriterBuilder(IConfiguration configuration) : base(configuration)
        {
            this.WithKeyFunction(_ => null!);
        }
    }

    public class KafkaWriterBuilder<TKey, T> : KafkaBuilderBase
    {
        Func<T, TKey> _keyFunction = _ => default;
        ISerializer<T> _serializer = new Utf8Serializer<T>();
        public KafkaWriterBuilder(IConfiguration configuration) : base(configuration)
        {
        }

        public KafkaWriterBuilder<TKey, T> WithTopic(string topic)
        {
            Topic = topic;
            return this;
        }

        public KafkaWriterBuilder<TKey, T> WithKeyFunction(Func<T, TKey> keyFunction)
        {
            _keyFunction = keyFunction;
            return this;
        }

        public KafkaWriterBuilder<TKey, T> UsingAvro(SchemaRegistryConfig? config = null)
        {
            SchemaRegistryConfig schemaRegistryConfig = config ?? new SchemaRegistryConfig
            {
                Url = Configuration.GetValue<string>(SCHEMA_REGISTRY_URL_KEY)
            };
            IAuthenticationHeaderValueProvider authHeaderValueProvider = new OAuthHeaderValueProvider(GetTokenService());
            ISchemaRegistryClient schemaRegistryClient = new CachedSchemaRegistryClient(schemaRegistryConfig, authHeaderValueProvider);
            _serializer = new AvroSerializer<T>(schemaRegistryClient).AsSyncOverAsync();
            return this;
        }

        public KafkaWriter<TKey, T> Build()
        {
            ValidateInput();
            var tokenService = GetTokenService();

            return new KafkaWriter<TKey, T>(
                Topic,
                _keyFunction,
                Configuration.GetValue<string>(KAFKA_URL_KEY) ?? throw new ArgumentNullException("Kafka key must not be null"),
                tokenService,
                IsAvro
                    ? GetAvroSerializer(tokenService)
                    : new Utf8Serializer<T>()
            );
        }

        private ISerializer<T> GetAvroSerializer(ITokenService tokenService)
        {
            var authHeaderValueProvider = new OAuthHeaderValueProvider(tokenService);
            var schemaRegistryClient = new CachedSchemaRegistryClient(SchemaRegistryConfig, authHeaderValueProvider);
            return new AvroSerializer<T>(schemaRegistryClient).AsSyncOverAsync();
        }
    }
}
