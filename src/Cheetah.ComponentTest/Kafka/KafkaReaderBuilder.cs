using Cheetah.Core.Infrastructure.Auth;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.Configuration;

namespace Cheetah.ComponentTest.Kafka
{

    public class KafkaReaderBuilder
    {
        public static KafkaReaderBuilder<TKey, T> Create<TKey, T>(IConfiguration configuration)
        {
            return new KafkaReaderBuilder<TKey, T>(configuration);
        }

        public static KafkaReaderBuilder<Null, T> Create<T>(IConfiguration configuration)
        {
            return new KafkaReaderBuilder<Null, T>(configuration);
        }

        private KafkaReaderBuilder() { }
    }
    public class KafkaReaderBuilder<TKey, T> : KafkaBuilderBase
    {
        private string? _consumerGroup;

        public KafkaReaderBuilder(IConfiguration configuration) : base(configuration)
        {
        }

        public KafkaReaderBuilder<TKey, T> WithTopic(string topic)
        {
            Topic = topic;
            return this;
        }

        public KafkaReaderBuilder<TKey, T> WithConsumerGroup(string consumerGroup)
        {
            _consumerGroup = consumerGroup;
            return this;
        }

        public KafkaReaderBuilder<TKey, T> UsingAvro(SchemaRegistryConfig? config = null)
        {
            UsingAvroInternal(config);
            return this;
        }

        public KafkaReader<TKey, T> Build()
        {
            ValidateInput();
         
            var tokenService = GetTokenService();
            
            var props = new KafkaReaderProps<T>()
            {
                ConsumerGroup = _consumerGroup,
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
