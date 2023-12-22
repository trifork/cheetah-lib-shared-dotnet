using System;
using Confluent.Kafka;

namespace Cheetah.Kafka
{
    public class KafkaClientFactoryOptions
    {
        internal Action<ClientConfig> DefaultClientConfigure { get; private set; } = config => { };
        internal Action<ProducerConfig> DefaultProducerConfigure { get; private set; } = config => { };
        internal Action<ConsumerConfig> DefaultConsumerConfigure { get; private set; } = config => { };
        internal Action<AdminClientConfig> DefaultAdminClientConfigure { get; private set; } = config => { };
        
        public KafkaClientFactoryOptions ConfigureDefaultClient(Action<ClientConfig> configure)
        {
            DefaultClientConfigure = configure;
            return this;
        }

        public KafkaClientFactoryOptions ConfigureDefaultProducer(Action<ProducerConfig> configure)
        {
            DefaultProducerConfigure = configure;
            return this;
        }

        public KafkaClientFactoryOptions ConfigureDefaultConsumer(Action<ConsumerConfig> configure)
        {
            DefaultConsumerConfigure = configure;
            return this;
        }

        public KafkaClientFactoryOptions ConfigureDefaultAdminClient(Action<AdminClientConfig> configure)
        {
            DefaultAdminClientConfigure = configure;
            return this;
        }
    }
}
