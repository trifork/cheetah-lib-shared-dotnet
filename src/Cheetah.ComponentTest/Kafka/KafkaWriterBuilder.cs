using System;
using Microsoft.Extensions.Configuration;

namespace Cheetah.ComponentTest.Kafka
{

    public class KafkaWriterBuilder
    {
        public static KafkaWriterBuilder<TKey, T> Create<TKey, T>()
        {
            return new KafkaWriterBuilder<TKey, T>();
        }

        private KafkaWriterBuilder() { }
    }
    public class KafkaWriterBuilder<TKey, T>
    {
        private const string KAFKA_URL = "KAFKA:URL";
        private const string KAFKA_CLIENTID = "KAFKA:CLIENTID";
        private const string KAFKA_CLIENTSECRET = "KAFKA:CLIENTSECRET";
        private const string KAFKA_SCOPE = "KAFKA:SCOPE";
        private const string KAFKA_AUTH_ENDPOINT = "KAFKA:AUTHENDPOINT";
        private string? KafkaConfigurationPrefix;
        private string? Topic;
        private IConfiguration? Configuration;
        private Func<T, TKey>? KeyFunction;

        internal KafkaWriterBuilder()
        {
        }

        public KafkaWriterBuilder<TKey, T> WithKafkaConfigurationPrefix(string prefix, IConfiguration configuration)
        {
            KafkaConfigurationPrefix = prefix;
            Configuration = configuration;
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

        public KafkaWriter<TKey, T> Build()
        {
            var writer = new KafkaWriter<TKey, T>
            {
                Topic = Topic,
                KeyFunction = KeyFunction
            };
            if (KafkaConfigurationPrefix != null && Configuration != null)
            {
                if (!string.IsNullOrEmpty(KafkaConfigurationPrefix))
                {
                    writer.Server = Configuration.GetSection(KafkaConfigurationPrefix).GetValue<string>(KAFKA_URL);
                    writer.ClientId = Configuration.GetSection(KafkaConfigurationPrefix).GetValue<string>(KAFKA_CLIENTID);
                    writer.ClientSecret = Configuration.GetSection(KafkaConfigurationPrefix).GetValue<string>(KAFKA_CLIENTSECRET);
                    writer.Scope = Configuration.GetSection(KafkaConfigurationPrefix).GetValue<string>(KAFKA_SCOPE);
                    writer.AuthEndpoint = Configuration.GetSection(KafkaConfigurationPrefix).GetValue<string>(KAFKA_AUTH_ENDPOINT);
                }
                else
                {
                    writer.Server = Configuration.GetValue<string>(KAFKA_URL);
                    writer.ClientId = Configuration.GetValue<string>(KAFKA_CLIENTID);
                    writer.ClientSecret = Configuration.GetValue<string>(KAFKA_CLIENTSECRET);
                    writer.Scope = Configuration.GetValue<string>(KAFKA_SCOPE);
                    writer.AuthEndpoint = Configuration.GetValue<string>(KAFKA_AUTH_ENDPOINT);
                }
            }
            else
            {
                throw new InvalidOperationException("KafkaConfigurationPrefix or Configuration is not set");
            }
            writer.Prepare();
            return writer;
        }
    }
}
