using Cheetah.Core.Config;
using Cheetah.Core.Infrastructure.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.ComponentTest.Test
{
    public class ComponentKafkaTest : KafkaComponentTest<SimpleTestModel, SimpleTestModel>
    {
        public ComponentKafkaTest(ILogger logger, IOptions<ComponentTestConfig> componentTestConfig, IOptions<KafkaConfig> kafkaConfig, CheetahKafkaTokenService tokenService) : base(logger, componentTestConfig, kafkaConfig, tokenService)
        {
        }

        protected override int ExpectedResponseCount => 1;

        protected override TimeSpan TestTimeout => TimeSpan.FromMinutes(2);

        protected override IEnumerable<SimpleTestModel> GetMessagesToPublish()
        {
            return new List<SimpleTestModel>() { new SimpleTestModel() };
        }

        protected override TestResult ValidateResult(IEnumerable<SimpleTestModel> result)
        {
            var testResults = new[]
            {
                ValidateInteger(result),
                ValidateString(result),
                ValidateLong(result),
                ValidateDouble(result)
            };

            return testResults.Select(x => x.IsPassed == false).Any() ?
                testResults.First(x => x.IsPassed)
                : TestResult.Passed;
        }

        private static TestResult ValidateInteger(IEnumerable<SimpleTestModel> result)
        {
            return result.All(x => x.IntergerTest == int.MaxValue)
                ? TestResult.Passed
                : TestResult.Failed("Ineter test failed");
        }

        private static TestResult ValidateString(IEnumerable<SimpleTestModel> result)
        {
            return result.All(x => x.StringTest == "TestString")
                ? TestResult.Passed
                : TestResult.Failed("String test failed");
        }

        private static TestResult ValidateLong(IEnumerable<SimpleTestModel> result)
        {
            return result.All(x => x.LongTest == long.MaxValue)
                ? TestResult.Passed
                : TestResult.Failed("Long test failed");
        }

        private static TestResult ValidateDouble(IEnumerable<SimpleTestModel> result)
        {
            return result.All(x => x.DoubleTest == double.MaxValue)
                ? TestResult.Passed
                : TestResult.Failed("Double test failed");
        }
    }
}
