using System;
using System.Threading;
using System.Threading.Tasks;
using Cheetah.Auth.Authentication;
using Cheetah.Kafka.Configuration;
using Cheetah.Kafka.Extensions;
using Cheetah.Kafka.Util;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Kafka
{
    public interface ISerializerFactory
    {
        ISerializer<T> GetSerializer<T>();
        IDeserializer<T> GetDeserializer<T>();
    }
    
    /// <summary>
    /// 
    /// </summary>
    public class Utf8SerializerFactory : ISerializerFactory {
        public IDeserializer<T> GetDeserializer<T>()
        {
            return new Utf8Serializer<T>();
        }

        public ISerializer<T> GetSerializer<T>()
        {
            return new Utf8Serializer<T>();
        }
    }


    public class AdminClientOptions : KafkaClientOptions<AdminClientConfig>
    {
        
    }

    public class ProducerClientOptions : KafkaClientOptions<ProducerConfig>
    {
        
    }

    public class ConsumerClientOptions : KafkaClientOptions<ConsumerConfig>
    {
        
    }

    public class KafkaClientOptions<T> where T : ClientConfig
    {
        private Action<T> _configureAction = _ => { };
        private Func<IServiceProvider, ISerializerFactory>? _serializerFactoryFactory;

        public void ConfigureClient(Action<T> configureAction)
        {
            _configureAction = configureAction;
        }

        public void SetSerializerFactory(Func<IServiceProvider, ISerializerFactory> serializerFactoryFactory)
        {
            _serializerFactoryFactory = serializerFactoryFactory;
        }
    } 
    
    /// <summary>
    /// Factory for creating Kafka clients
    /// </summary>
    public class KafkaClientFactory
    {
        private readonly ITokenService _kafkaTokenService;
        private readonly ILogger<KafkaClientFactory> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly KafkaConfig _config;
        private readonly KafkaClientFactoryOptions _options;
        private readonly ISerializerFactory _serializerFactory;

        /// <summary>
        /// Creates a new instance of <see cref="KafkaClientFactory"/>
        /// </summary>
        /// <param name="kafkaKafkaTokenService">The token service to use for kafka</param>
        /// <param name="loggerFactory">The logger factory used to create necessary loggers</param>
        /// <param name="config">The configuration to use when creating clients</param>
        /// <param name="options">The options to use when creating clients</param>
        /// <param name="serializerFactory"></param>
        public KafkaClientFactory(
            [FromKeyedServices("Kafka")] ITokenService kafkaKafkaTokenService,
            ILoggerFactory loggerFactory,
            IOptions<KafkaConfig> config,
            KafkaClientFactoryOptions options,
            ISerializerFactory serializerFactory)
        {
            _kafkaTokenService = kafkaKafkaTokenService;
            _loggerFactory = loggerFactory;
            _config = config.Value;
            _config.Validate();
            _options = options;
            _serializerFactory = serializerFactory;
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
        public IProducer<TKey, TValue> CreateProducer<TKey, TValue>(Action<ProducerClientOptions>? configAction = null, ISerializer<TValue>? serializer = null)
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
            var configInstance = new ProducerConfig(GetDefaultConfig());
            _options.DefaultProducerConfigure(configInstance);
            configAction?.Invoke(configInstance);
            
            return new ProducerBuilder<TKey, TValue>(configInstance)
                .AddCheetahOAuthentication(GetTokenRetrievalFunction(), _loggerFactory.CreateLogger<IProducer<TKey, TValue>>())
                .SetValueSerializer(serializer ?? _serializerFactory.GetSerializer<TValue>());
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
            var config = new ConsumerConfig(GetDefaultConfig());
            _options.DefaultConsumerConfigure(config);
            configAction?.Invoke(config);
            
            return new ConsumerBuilder<TKey, TValue>(config)
                .AddCheetahOAuthentication(GetTokenRetrievalFunction(), _loggerFactory.CreateLogger<IConsumer<TKey, TValue>>())
                .SetValueDeserializer(deserializer ?? _serializerFactory.GetDeserializer<TValue>());
        }
        
        /// <summary>
        /// Creates a pre-configured <see cref="IAdminClient"/>/>
        /// </summary>
        /// <returns>A pre-configured <see cref="IAdminClient"/></returns>
        public IAdminClient CreateAdminClient(Action<AdminClientConfig>? configAction = null)
        {
            return CreateAdminClientBuilder(configAction).Build();
        }
        
        /// <summary>
        /// Creates a pre-configured <see cref="AdminClientBuilder"/>/>
        /// </summary>
        /// <param name="configAction">Optional action to modify the used <see cref="AdminClientConfig"/></param>
        /// <returns>A pre-configured <see cref="AdminClientBuilder"/></returns>
        public AdminClientBuilder CreateAdminClientBuilder(Action<AdminClientConfig>? configAction = null)
        {
            var config = new AdminClientConfig(GetDefaultConfig());
            _options.DefaultAdminClientConfigure(config);
            configAction?.Invoke(config);
            
            return new AdminClientBuilder(config)
                .AddCheetahOAuthentication(GetTokenRetrievalFunction(), _loggerFactory.CreateLogger<IAdminClient>());
        }

        // Developer note:
        // The configuration structure used here ensures that a new instance of the configuration is created for each client.
        // This is important because using the copy-constructors provided by Confluent.Kafka will modify the underlying dictionary in-place.
        // This means that if we were to use the same configuration instance for each client, we would end up with a situation where
        // the configuration for the first client would be modified by the second client, and so on.
        private ClientConfig GetDefaultConfig() => _config.GetClientConfig();
        
        private Func<Task<(string AccessToken, long Expiration, string Principal)>> GetTokenRetrievalFunction() {
            return async () =>
            {
                var response = await _kafkaTokenService.RequestAccessTokenAsync(CancellationToken.None);
                return (response.AccessToken, response.Expiration, _config.Principal);
            };
        }
    }
}
