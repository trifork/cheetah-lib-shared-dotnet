using System.Threading.Tasks;
using Cheetah.Auth.Authentication;
using Cheetah.Auth.Util;
using Cheetah.Kafka;
using Cheetah.Kafka.Configuration;
using Cheetah.Kafka.Testing;
using Cheetah.SchemaRegistry.Avro;
using Cheetah.SchemaRegistry.Configuration;
using Cheetah.SchemaRegistry.Utils;
using Confluent.SchemaRegistry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Cheetah.SchemaRegistry.Testing
{
    /// <summary>
    /// Factory for creating Kafka clients for reading and writing Avro messages
    /// </summary>
    public class AvroKafkaTestClientFactory
    {
        /// <summary>
        /// Creates a <see cref="AvroKafkaTestClientFactory"/> using the provided <see cref="IConfiguration"/> instance
        /// </summary>
        /// <param name="configuration">The configuration to use</param>
        /// <param name="tokenService">An optional token service, used to retrieve access tokens</param>
        /// <param name="loggerFactory">An optional logger factory</param>
        /// <param name="options">An optional </param>
        /// <returns></returns>
        public static KafkaTestClientFactory Create(IConfiguration configuration,
            ClientFactoryOptions? options = null,
            ITokenService? tokenService = null,
            ILoggerFactory? loggerFactory = null)
        {
            var schemaConfig = new SchemaConfig();
            configuration.Bind(SchemaConfig.Position, schemaConfig);
            schemaConfig.Validate();

            var kafkaConfig = new KafkaConfig();
            configuration.Bind(KafkaConfig.Position, kafkaConfig);
            kafkaConfig.Validate();

            loggerFactory ??= LoggerFactory.Create(builder => builder.AddConsole());

            var schemaTokenService = new CachedTokenProvider(schemaConfig.OAuth2,
                new OAuthTokenProvider(schemaConfig.OAuth2, new DefaultHttpClientFactory()),
                loggerFactory.CreateLogger<CachedTokenProvider>());
            Task.Run(schemaTokenService.StartAsync);

            var authHeaderValueProvider = new OAuthHeaderValueProvider(schemaTokenService);
            var schemaRegistryClient = new CachedSchemaRegistryClient(schemaConfig.GetSchemaRegistryConfig(), authHeaderValueProvider);
            var serializerProvider = new AvroSerializerProvider(schemaRegistryClient);
            var deserializerProvider = new AvroDeserializerProvider(schemaRegistryClient);

            return KafkaTestClientFactory.Create(kafkaConfig, options, tokenService, loggerFactory, serializerProvider, deserializerProvider);
        }
    }
}
