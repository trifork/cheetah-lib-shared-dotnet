# CheetahComponentTest

This library provide `KafkaComponentTest`, an extendable class containing logic for publishing and consuming data from topics, specefied in `ComponentTestConfig`.

`ComponentTestConfig` comes with some default values, but requires 2 parameter:

- ConsumerTopic
- ProducerTopic

`ConsumerTopic` is the topic the `KafkaComponentTest` comsume from.

`ProducerTopic` is the topic the `KafkaComponentTest` produce to.

## Configuration for using NuGet package

To use the NuGet package inside the container, the packageSource must be added to a NuGet.config that is being copied into the container.

```bash
<configuration>
  <packageSources>
    <add key="trifork-github" value="https://nuget.pkg.github.com/trifork/index.json" />
  </packageSources>
<packageSourceCredentials>
      <trifork-github>
        <add key="Username" value="%GITHUB_ACTOR%" />
        <add key="ClearTextPassword" value="%GITHUB_TOKEN%" />
      </trifork-github>
    </packageSourceCredentials>
</configuration>
```

`GITHUB_ACTOR` and `GITHUB_TOKEN` needs to be environment variables explained [here](https://docs.cheetah.trifork.dev/getting-started/guided-tour/prerequisites).

## Running test in docker compose

To run the componentest in docker compose, it's required to pull the NuGet package.

```bash
# Example for docker compose componenttest
example-component-test:
    build:
      context: .
      dockerfile: Dockerfile
      args:
        # Nuget restore outside Visual Studio
        - GITHUB_ACTOR=${GITHUB_ACTOR:-Missing required GITHUB_ACTOR}
        - GITHUB_TOKEN=${GITHUB_TOKEN:-Missing required GITHUB_TOKEN}
    environment:
      KAFKA__BOOTSTRAPSERVER: kafka_Server:Port_number
      KAFKA__CONSUMERTOPIC: ConsumerTopic
      KAFKA__PRODUCERTOPIC: ProducerTopic
    depends_on:
      kafka:
        condition: service_healthy
```

Example of a docker file, copying `NuGet-CI.Config` into the container.

```bash
FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

ARG GITHUB_ACTOR
ARG GITHUB_TOKEN

WORKDIR /src
COPY "NuGet-CI.Config" "NuGet.config"
COPY ["ComponentTest.csproj", "ComponentTest/"]
RUN dotnet restore "ComponentTest.csproj"
COPY . .
WORKDIR "/src/ComponentTest"
RUN dotnet build "ComponentTest.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ComponentTest.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ComponentTest.dll"]
```

# Code examples

## Creating a test

### Making a class for producing or consuming data.

The class for consuming or producing data in the component test can be a simple C# class, no imports need.

```c#
public class ExampleProducerData
    {
        public int ExampleInteger { get; set; }
        public string ExampleString { get; set; }
    }
```

```c#
public class ExampleConsumerData
    {
        public int ExampleInteger { get; set; }
    }
```

It's only important they contain the properies you wish to test. And they don't have to be identical. Shown above is an example of the producer object having a property the consumer doesn't have. As it's not important to test. Could be it's not in the topic after an operation done between the two objects. 

### Making the test case

```c#
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
```

Above is an auto implemented version of the KafkaComponentTest, with `ExampleProducerData` and `ExampleConsumerData` as parameters. This doesn't have to be a class, but could be a primitive type. 

It's not necessary, to check if the result count is less than or more than `ExpectedResponseCount`, as the framework will fail if the correct number of messages isn't recieved.

### Setting up the compoent test runner

There are two ways of adding test to the component test runner. Either using `AddAllTests()` or add them each individually like below.

```c#
## Create testrunner
await new ComponentTestRunner()
    .AddTest<ExampleComponentTest>()
    .WithConfiguration<ComponentTestConfig>(ComponentTestConfig.Position)
    .RunAsync(args);
```