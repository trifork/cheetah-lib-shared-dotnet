# Configuration

First you should register options

```c#
//Options
var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
services.Configure<OAuthConfig>(configuration.GetSection(OAuthConfig.Position));
services.Configure<KafkaConfig>(configuration.GetSection(KafkaConfig.Position));
```

Each setting can be overwritten by environment variables as `<position>__<property>=value` e.g. `Kafka__KafkaUrl=kafka:19092`.

See [Available configuration](../api/Cheetah.Shared.WebApi.Core.Config.yml) in api docs.
