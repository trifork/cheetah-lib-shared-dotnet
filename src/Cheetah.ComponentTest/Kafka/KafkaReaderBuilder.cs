using System;
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
    public class KafkaReaderBuilder<TKey, T>
    {
        private const string KAFKA_URL = "KAFKA:URL";
        private const string KAFKA_CLIENTID = "KAFKA:CLIENTID";
        private const string KAFKA_CLIENTSECRET = "KAFKA:CLIENTSECRET";
        private const string KAFKA_AUTH_ENDPOINT = "KAFKA:AUTHENDPOINT";
        private const string? SCHEMA_REGISTRY_URL = "KAFKA:SCHEMAREGISTRYURL";
        private bool IsAvro;
        private SchemaRegistryConfig? SchemaRegistryConfig;
        private string? KafkaConfigurationPrefix;
        private string? Topic;
        private IConfiguration? Configuration;
        private string? GroupId;

        public KafkaReaderBuilder(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public KafkaReaderBuilder<TKey, T> WithKafkaConfigurationPrefix(string prefix)
        {
            KafkaConfigurationPrefix = prefix;
            return this;
        }

        public KafkaReaderBuilder<TKey, T> WithTopic(string topic)
        {
            Topic = topic;
            return this;
        }

        public KafkaReaderBuilder<TKey, T> WithGroupId(string groupId)
        {
            GroupId = groupId;
            return this;
        }

        public KafkaReaderBuilder<TKey, T> UsingAvro()
        {
            IsAvro = true;
            return this;
        }

        public KafkaReaderBuilder<TKey, T> UsingAvro(SchemaRegistryConfig schemaRegistryConfig)
        {
            SchemaRegistryConfig = schemaRegistryConfig;
            return UsingAvro();
        }

        public KafkaReader<TKey, T> Build()
        {
            var reader = new KafkaReader<TKey, T>
            {
                Topic = Topic,
                ConsumerGroup = GroupId
            };

            if (Configuration == null)
            {
                throw new InvalidOperationException("No configuration provided. You must call 'WithKafkaConfiguration'");
            }

            var configurationSection = string.IsNullOrWhiteSpace(KafkaConfigurationPrefix)
                ? Configuration
                : Configuration.GetSection(KafkaConfigurationPrefix);
            
            reader.Server = Configuration.GetValue<string>(KAFKA_URL);
            reader.ClientId = Configuration.GetValue<string>(KAFKA_CLIENTID);
            reader.ClientSecret = Configuration.GetValue<string>(KAFKA_CLIENTSECRET);
            reader.AuthEndpoint = Configuration.GetValue<string>(KAFKA_AUTH_ENDPOINT);

            if (IsAvro)
            {
                SchemaRegistryConfig ??= new SchemaRegistryConfig()
                {
                    Url = configurationSection.GetValue<string>(SCHEMA_REGISTRY_URL)
                };
                var schemaRegistry = new CachedSchemaRegistryClient(SchemaRegistryConfig);
                reader.Serializer = new AvroDeserializer<T>(schemaRegistry).AsSyncOverAsync();
            }
            
            reader.Prepare();
            return reader;
        }
    }
}
