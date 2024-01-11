using Cheetah.ComponentTest.Kafka;
using Cheetah.ComponentTest.TokenService;
using Cheetah.ConsoleTester.DataModel;
using Cheetah.Core.Infrastructure.Auth;
using Cheetah.Core.Infrastructure.Services.Kafka;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Cheetah.ConsoleTester.Kafka;

public class KafkaJsonReader
{
    private static readonly ILogger Logger = new LoggerFactory().CreateLogger<KafkaJsonReader>();
    internal string Topic { get; }
    private IConsumer<Null, string> Consumer { get; }
    
    JsonKafkaConfig _config;
    
    public KafkaJsonReader(JsonKafkaConfig config)
    {
        Topic = _config.Topic;
        Logger.LogInformation("Preparing kafka producer, producing to topic '{Topic}'", Topic);
        Consumer = new ConsumerBuilder<Null, string>(new ConsumerConfig
            {
                BootstrapServers = _config.KafkaUrl,
                GroupId = "ConsoleTestGroup",
                SaslMechanism = SaslMechanism.OAuthBearer,
                SecurityProtocol = SecurityProtocol.SaslPlaintext,
                EnablePartitionEof = true,
                AllowAutoCreateTopics = true,
                AutoOffsetReset = AutoOffsetReset.Latest
            })
            .AddCheetahOAuthentication(GetTokenService(), Logger)
            .Build();

        Consumer.Assign(new TopicPartition(Topic, 0));
        CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(2));
        var cancellationToken = cancellationTokenSource.Token;
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = Consumer.Consume(cancellationToken);
                if (consumeResult.IsPartitionEOF)
                {
                    break;
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
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
