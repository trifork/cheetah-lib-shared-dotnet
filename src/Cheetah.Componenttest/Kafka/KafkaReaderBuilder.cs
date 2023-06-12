using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace Cheetah.ComponentTest.Kafka
{

    public class KafkaReaderBuilder
    {
        public static KafkaReaderBuilder<TKey, T> Create<TKey, T>()
        {
            return new KafkaReaderBuilder<TKey, T>();
        }

        public static KafkaReaderBuilder<Null, T> Create<T>()
        {
            return new KafkaReaderBuilder<Null, T>();
        }

        private KafkaReaderBuilder() { }
    }
    public class KafkaReaderBuilder<TKey, T>
    {
        private const string KAFKA_URL = "KAFKA:URL";
        private const string KAFKA_CLIENTID = "KAFKA:CLIENTID";
        private const string KAFKA_CLIENTSECRET = "KAFKA:CLIENTSECRET";
        private const string KAFKA_AUTH_ENDPOINT = "KAFKA:AUTHENDPOINT";
        private string? KafkaConfigurationPrefix;
        private string? Topic;
        private IConfiguration? Configuration;
        private string GroupId;

        public KafkaReaderBuilder<TKey, T> WithKafkaConfigurationPrefix(string prefix, IConfiguration configuration)
        {
            KafkaConfigurationPrefix = prefix;
            Configuration = configuration;
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

        public async Task<KafkaReader<TKey, T>> BuildAsync()
        {
            var reader = new KafkaReader<TKey, T>
            {
                Topic = Topic,
                ConsumerGroup = GroupId
            };
            if (KafkaConfigurationPrefix != null && Configuration != null)
            {
                if (!string.IsNullOrEmpty(KafkaConfigurationPrefix))
                {
                    reader.Server = Configuration.GetSection(KafkaConfigurationPrefix).GetValue<string>(KAFKA_URL);
                    reader.ClientId = Configuration.GetSection(KafkaConfigurationPrefix).GetValue<string>(KAFKA_CLIENTID);
                    reader.ClientSecret = Configuration.GetSection(KafkaConfigurationPrefix).GetValue<string>(KAFKA_CLIENTSECRET);
                    reader.AuthEndpoint = Configuration.GetSection(KafkaConfigurationPrefix).GetValue<string>(KAFKA_AUTH_ENDPOINT);
                }
                else
                {
                    reader.Server = Configuration.GetValue<string>(KAFKA_URL);
                    reader.ClientId = Configuration.GetValue<string>(KAFKA_CLIENTID);
                    reader.ClientSecret = Configuration.GetValue<string>(KAFKA_CLIENTSECRET);
                    reader.AuthEndpoint = Configuration.GetValue<string>(KAFKA_AUTH_ENDPOINT);
                }
            }
            else
            {
                throw new InvalidOperationException("KafkaConfigurationPrefix or Configuration is not set");
            }
            await reader.PrepareAsync();
            return reader;
        }
    }
}
