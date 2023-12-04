using System;
using System.Threading;
using System.Threading.Tasks;
using Cheetah.Auth.Authentication;
using Cheetah.Kafka.Configuration;
using Cheetah.Kafka.Extensions;
using Cheetah.Kafka.Util;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Kafka
{
    /// <summary>
    /// Factory for creating Kafka clients
    /// </summary>
    public class KafkaClientFactory
    {
        private readonly ITokenService _tokenService;
        private readonly ILogger<KafkaClientFactory> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly KafkaConfig _config;

        /// <summary>
        /// Creates a new instance of <see cref="KafkaClientFactory"/>
        /// </summary>
        /// <param name="tokenService">The token service to use</param>
        /// <param name="authLogger">The logger to use when logging authentication-related messages</param>
        /// <param name="config">The configuration to use when creating clients</param>
        public KafkaClientFactory(ITokenService tokenService, ILoggerFactory loggerFactory, IOptions<KafkaConfig> config)
        {
            _tokenService = tokenService;
            _loggerFactory = loggerFactory;
            _config = config.Value;
            _logger = _loggerFactory.CreateLogger<KafkaClientFactory>();
        }

        /// <summary>
        /// Creates a pre-configured <see cref="IProducer{TKey,TValue}"/>/>
        /// </summary>
        /// <param name="configAction">Optional action used to modify the configuration</param>
        /// <param name="serializer">Optional serializer to use. If not supplied, a <see cref="Utf8Serializer{T}"/> will be used</param>
        /// <typeparam name="TKey">The type of message key that the resulting producer will produce</typeparam>
        /// <typeparam name="TValue">The type of message value that the resulting producer will produce</typeparam>
        /// <returns>A pre-configured <see cref="IProducer{TKey,TValue}"/></returns>
        public IProducer<TKey, TValue> CreateProducer<TKey, TValue>(Action<ProducerConfig>? configAction = null, ISerializer<TValue>? serializer = null)
        {
            return CreateProducerBuilder<TKey, TValue>(configAction, serializer).Build();
        }
        
        /// <summary>
        /// Creates a pre-configured <see cref="ProducerBuilder{TKey,TValue}"/>/>
        /// </summary>
        /// <inheritdoc cref="KafkaClientFactory.CreateConsumer{TKey, TValue}"/>
        /// <returns>A pre-configured <see cref="ProducerBuilder{TKey, TValue}"/></returns>
        public ProducerBuilder<TKey, TValue> CreateProducerBuilder<TKey, TValue>(Action<ProducerConfig>? configAction = null, ISerializer<TValue>? serializer = null)
        {
            var config = _config.ToProducerConfig();
            configAction?.Invoke(config);
            
            return new ProducerBuilder<TKey, TValue>(config)
                .AddCheetahOAuthentication(TokenRetrievalFunction, _loggerFactory.CreateLogger<IProducer<TKey, TValue>>())
                .SetValueSerializer(serializer ?? new Utf8Serializer<TValue>());
        }
        
        /// <summary>
        /// Creates a pre-configured <see cref="IConsumer{TKey,TValue}"/>/>
        /// </summary>
        /// <param name="configAction">Optional action used to modify the configuration</param>
        /// <param name="deserializer">Optional deserializer to use. If not supplied, a <see cref="Utf8Serializer{T}"/> will be used</param>
        /// <typeparam name="TKey">The type of message key that the resulting consumer will consume</typeparam>
        /// <typeparam name="TValue">The type of message value that the resulting consumer will consume</typeparam>
        /// <returns>A pre-configured <see cref="IConsumer{TKey,TValue}"/></returns>
        public IConsumer<TKey, TValue> CreateConsumer<TKey, TValue>(Action<ConsumerConfig>? configAction = null, IDeserializer<TValue>? deserializer = null)
        {
            return CreateConsumerBuilder<TKey, TValue>(configAction, deserializer).Build();
        }

        /// <summary>
        /// Creates a pre-configured <see cref="ConsumerBuilder{TKey,TValue}"/>/>
        /// </summary>
        /// <inheritdoc cref="KafkaClientFactory.CreateConsumer{TKey, TValue}"/>
        /// <returns>A pre-configured <see cref="ConsumerBuilder{TKey,TValue}"/></returns>
        public ConsumerBuilder<TKey, TValue> CreateConsumerBuilder<TKey, TValue>(Action<ConsumerConfig>? configAction = null, IDeserializer<TValue>? deserializer = null)
        {
            var config = _config.ToConsumerConfig();
            configAction?.Invoke(config);
            
            return new ConsumerBuilder<TKey, TValue>(config)
                .AddCheetahOAuthentication(TokenRetrievalFunction, _loggerFactory.CreateLogger<IConsumer<TKey, TValue>>())
                .SetValueDeserializer(deserializer ?? new Utf8Serializer<TValue>());
        }
        
        /// <summary>
        /// Creates a pre-configured <see cref="IAdminClient"/>/>
        /// </summary>
        /// <returns>A pre-configured <see cref="IAdminClient"/></returns>
        public IAdminClient CreateAdminClient()
        {
            return CreateAdminClientBuilder().Build();
        }
        
        /// <summary>
        /// Creates a pre-configured <see cref="AdminClientBuilder"/>/>
        /// </summary>
        /// <returns>A pre-configured <see cref="AdminClientBuilder"/></returns>
        public AdminClientBuilder CreateAdminClientBuilder()
        {
            return new AdminClientBuilder(_config.ToConsumerConfig())
                .AddCheetahOAuthentication(TokenRetrievalFunction, _loggerFactory.CreateLogger<IAdminClient>());
        }
        
        // Convenience to avoid spreading lambdas
        private Func<Task<(string AccessToken, long Expiration, string? PrincipalName)?>> TokenRetrievalFunction => 
            () => _tokenService.RequestAccessTokenAsync(CancellationToken.None);  
    }
}
