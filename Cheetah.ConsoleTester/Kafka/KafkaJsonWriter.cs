using Cheetah.ComponentTest.TokenService;
using Cheetah.ConsoleTester.DataModel;
using Cheetah.Core.Infrastructure.Auth;
using Cheetah.Core.Infrastructure.Services.Kafka;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Cheetah.ConsoleTester.Kafka;

public class KafkaJsonWriter
{
    private static readonly ILogger Logger = new LoggerFactory().CreateLogger<KafkaJsonWriter>();
    string Topic { get; }
    private IProducer<string, string> Producer { get; }

    JsonKafkaConfig _config;
    
    public KafkaJsonWriter(JsonKafkaConfig config)
    {
        _config = config;
        Topic = config.Topic;

        Producer = new ProducerBuilder<string, string>(
                new ProducerConfig
                {
                    BootstrapServers = config.KafkaUrl,
                    SaslMechanism = SaslMechanism.OAuthBearer,
                    SecurityProtocol = SecurityProtocol.SaslPlaintext,
                })
            .AddCheetahOAuthentication(GetTokenService(), Logger)
            .Build();
    }

    public async Task WriteAsync()
    {
        foreach (var message in _config.Messages)
        {
            var kafkaMessage = new Message<string, string>
            {
                Value = message
            };
            await Producer.ProduceAsync(Topic, kafkaMessage);
        }
    }
    
    private ITokenService GetTokenService()
    {
        return new TestTokenService(
            _config.ClientId,
            _config.ClientSecret,
            _config.TokenEndpoint
        );
    }
}
