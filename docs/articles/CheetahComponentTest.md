# CheetahComponentTest

This library provide `KafkaComponentTest`, an extendable class containing logic for publishing and consuming data from topics, specefied in `KafkaConfiguration`.

`KafkaConfiguration` comes with some default values, but require 3 parameter:

- BootstrapServer
- ConsumerTopic
- ProducerTopic

`BootstrapServer` is the server for kafka.

`ConsumerTopic` is the topic the `KafkaComponentTest` comsume from.

`ProducerTopic` is the topic the `KafkaComponentTest` produce to.

```c#
## Create testrunner
await new ComponentTestRunner()
    .AddTest<ExampleTest>()
    .AddTest<AnotherExampleTest>()
    .WithConfiguration<KafkaConfiguration>(KafkaConfiguration.Position)
    .RunAsync(args);
```

## Configuration for using NuGet package

To use the NuGet package this configuration must be added to the `NuGet.Config` file:

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

Example of a docker file, copying `NuGet.Config` into the container.

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
