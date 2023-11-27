using System;
using System.Collections.Generic;
using Cheetah.Core.Authentication;
using Cheetah.Kafka.Config;
using Cheetah.Kafka.Extensions;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Kafka
{
    public class KafkaClientFactory
    {
        private readonly ITokenService _tokenService;
        private readonly ILogger _logger;
        private readonly KafkaConfig _config;

        public KafkaClientFactory(ITokenService tokenService, ILogger<KafkaClientFactory> logger, IOptions<KafkaConfig> config)
        {
            _tokenService = tokenService;
            _logger = logger;
            _config = config.Value;
        }

        public IProducer<TKey, T> CreateProducer<TKey, T>(Action<ProducerConfig>? configAction = null, ISerializer<T>? serializer = null)
        {
            return CreateProducerBuilder<TKey, T>(configAction, serializer).Build();
        }
        
        public ProducerBuilder<TKey, T> CreateProducerBuilder<TKey, T>(Action<ProducerConfig>? configAction = null, ISerializer<T>? serializer = null)
        {
            var config = _config.ToProducerConfig();
            configAction?.Invoke(config);
            
            return new ProducerBuilder<TKey, T>(config)
                .AddCheetahOAuthentication(_tokenService, _logger)
                .SetValueSerializer(serializer ?? new Utf8Serializer<T>());
        }
        
        public IConsumer<TKey, T> CreateConsumer<TKey, T>(Action<ConsumerConfig>? configAction = null, IDeserializer<T>? deserializer = null)
        {
            return CreateConsumerBuilder<TKey, T>(configAction, deserializer).Build();
        }

        public ConsumerBuilder<TKey, T> CreateConsumerBuilder<TKey, T>(Action<ConsumerConfig>? configAction = null, IDeserializer<T>? deserializer = null)
        {
            var config = _config.ToConsumerConfig();
            configAction?.Invoke(config);
            
            return new ConsumerBuilder<TKey, T>(config)
                .AddCheetahOAuthentication(_tokenService, _logger)
                .SetValueDeserializer(deserializer ?? new Utf8Serializer<T>());
        }
        
        public IAdminClient CreateAdminClient()
        {
            return CreateAdminClientBuilder().Build();
        }
        
        public AdminClientBuilder CreateAdminClientBuilder()
        {
            return new AdminClientBuilder(_config.ToConsumerConfig())
                .AddCheetahOAuthentication(_tokenService, _logger);
        }
    }
}
