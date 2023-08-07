using System;
using Cheetah.Core.Config;
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

        private KafkaWriterBuilder() { }
    }
    public class KafkaWriterBuilder<TKey, T>
    {
        private const string KAFKA_URL = "KAFKA:URL";
        private const string KAFKA_CLIENTID = "KAFKA:CLIENTID";
        private const string KAFKA_CLIENTSECRET = "KAFKA:CLIENTSECRET";
        private const string KAFKA_OAUTHSCOPE = "KAFKA:OAUTHSCOPE";
        private const string KAFKA_AUTH_ENDPOINT = "KAFKA:AUTHENDPOINT";
        private const string? SCHEMA_REGISTRY_URL = "KAFKA:SCHEMAREGISTRYURL";
        private bool IsAvro;
        private SchemaRegistryConfig? SchemaRegistryConfig;
        private string? KafkaConfigurationPrefix;
        private string? Topic;
        private IConfiguration? Configuration;
        private Func<T, TKey>? KeyFunction;

        public KafkaWriterBuilder(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        internal KafkaWriterBuilder()
        {
        }

        public KafkaWriterBuilder<TKey, T> WithKafkaConfigurationPrefix(string prefix)
        {
            KafkaConfigurationPrefix = prefix;
            return this;
        }
        
        public KafkaWriterBuilder<TKey, T> WithTopic(string topic)
        {
            Topic = topic;
            return this;
        }

        public KafkaWriterBuilder<TKey, T> WithKeyFunction(Func<T, TKey> keyFunction)
        {
            KeyFunction = keyFunction;
            return this;
        }

        public KafkaWriterBuilder<TKey, T> UsingAvro()
        {
            IsAvro = true;
            return this;
        }
        public KafkaWriterBuilder<TKey, T> UsingAvro(SchemaRegistryConfig config)
        {
            SchemaRegistryConfig = config;
            return UsingAvro();
        }

        public KafkaWriter<TKey, T> Build()
        {
            var writer = new KafkaWriter<TKey, T>
            {
                Topic = Topic,
                KeyFunction = KeyFunction
            };
            
            if (Configuration == null)
            {
                throw new InvalidOperationException("No configuration provided. You must call 'WithKafkaConfiguration'");
            }

            var configurationSection = string.IsNullOrWhiteSpace(KafkaConfigurationPrefix)
                ? Configuration
                : Configuration.GetSection(KafkaConfigurationPrefix);

            writer.Server = configurationSection.GetValue<string>(KAFKA_URL);
            writer.ClientId = configurationSection.GetValue<string>(KAFKA_CLIENTID);
            writer.ClientSecret = configurationSection.GetValue<string>(KAFKA_CLIENTSECRET);
            writer.AuthEndpoint = configurationSection.GetValue<string>(KAFKA_AUTH_ENDPOINT);
           

            if (IsAvro)
            {
                SchemaRegistryConfig ??= new SchemaRegistryConfig
                {
                    Url = configurationSection.GetValue<string>(SCHEMA_REGISTRY_URL)
                };
                var schemaRegistry = new CachedSchemaRegistryClient(SchemaRegistryConfig);
                writer.Serializer = new AvroSerializer<T>(schemaRegistry).AsSyncOverAsync();
            }
            
            writer.Prepare();
            return writer;
        }
    }
}
