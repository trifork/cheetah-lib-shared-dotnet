// See https://aka.ms/new-console-template for more information

using Cheetah.Kafka.ExampleProcessor.Models;
using Cheetah.Kafka.ExampleProcessor.Services;
using Cheetah.Kafka.Extensions;
using Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = new HostApplicationBuilder();
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings.development.json", true)
    .AddEnvironmentVariables();

builder.Services.AddCheetahKafka(builder.Configuration, options =>
    {
        options.ConfigureDefaultConsumer(config =>
        {
            config.AllowAutoCreateTopics = true;
            config.GroupId = "the-group";
        });
    })
    .WithKeyedConsumer<string, ExampleModel>("A", cfg => {
        cfg.GroupId = "a-group";
    })
    .WithKeyedConsumer<string, ExampleModel>("B", cfg =>
    {
        // cfg.GroupId = "b-group";
    })
    .WithProducer<string, ExampleModel>();

builder.Services.Configure<TopicConfig>(builder.Configuration.GetSection(TopicConfig.Position));
builder.Services.AddHostedService<ProducerService>();
builder.Services.AddHostedService<AConsumerService>();
builder.Services.AddHostedService<BConsumerService>();


await builder.Build().RunAsync();

namespace Config
{
    public class TopicConfig
    {
        public const string Position = "Config";

        public string TopicName { get; set; }
    }
}

