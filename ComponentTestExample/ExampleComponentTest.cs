using Cheetah.ComponentTest;
using Cheetah.Core.Config;
using Cheetah.Core.Infrastructure.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ComponentTestExample
{
    public class ExampleComponentTest : KafkaComponentTest<ExampleProducerData, ExampleConsumerData>
    {
        public ExampleComponentTest(ILogger logger, IOptions<ComponentTestConfig> componentTestConfig, IOptions<KafkaConfig> kafkaConfig, CheetahKafkaTokenService tokenService) : base(logger, componentTestConfig, kafkaConfig, tokenService)
        {
        }

        protected override int ExpectedResponseCount => 1;

        protected override TimeSpan TestTimeout => TimeSpan.FromMinutes(2);

        protected override IEnumerable<ExampleProducerData> GetMessagesToPublish()
        {
            return new[] { new ExampleProducerData() { ExampleInteger = 10, } };
        }

        protected override TestResult ValidateResult(IEnumerable<ExampleConsumerData> result)
        {
            if (result.All(x => x.ExampleInteger != 10)) 
                return TestResult.Failed("Ineteger is wrong value");

            return TestResult.Passed;
        }
    }
}
