// See https://aka.ms/new-console-template for more information

using Cheetah.Kafka.ExampleProcessor.Models;
using Cheetah.Kafka.ExampleProcessor.Services;
using Cheetah.Kafka.Extensions;
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
    .WithKeyedConsumer<string, ExampleModel>("B")
    .WithProducer<string, ExampleModel>();

builder.Services.AddHostedService<ProducerService>();
builder.Services.AddHostedService<AConsumerService>();
builder.Services.AddHostedService<BConsumerService>();


await builder.Build().RunAsync();
