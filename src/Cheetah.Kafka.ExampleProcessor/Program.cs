// See https://aka.ms/new-console-template for more information

using Cheetah.Kafka.Avro;
using Cheetah.Kafka.ExampleProcessor.Models;
using Cheetah.Kafka.ExampleProcessor.Services;
using Cheetah.Kafka.Extensions;
using Cheetah.Kafka.Serialization;
using Cheetah.Kafka.Util;
using Cheetah.SchemaRegistry;
using Cheetah.SchemaRegistry.Extensions;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = new HostApplicationBuilder();
builder
    .Configuration.AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings.development.json", true)
    .AddEnvironmentVariables();

builder.Services.AddCheetahSchemaRegistry(builder.Configuration);

builder.Services.AddCheetahKafka(builder.Configuration, options =>
    {
        options.ConfigureDefaultSerializerProvider(AvroSerializerProvider.FromServices());
        options.ConfigureDefaultConsumer(config =>
        {
            config.AllowAutoCreateTopics = true;
            config.GroupId = "the-group";
        });
    })
    .WithKeyedConsumer<string, ExampleModel>("A", options =>
    {
        options.ConfigureClient(cfg =>
        {
            cfg.GroupId = "the-big-group";
        });
    })
    .WithKeyedConsumer<string, ExampleModel>("B")
    .WithProducer<string, ExampleModel>(options =>
    {
        options.SetSerializer(AvroSerializer.FromServices<ExampleModel>());
        options.ConfigureClient(cfg =>
        {
            cfg.BatchSize = 100;
        });
    });

builder.Services.AddHostedService<ProducerService>();
builder.Services.AddHostedService<AConsumerService>();
builder.Services.AddHostedService<BConsumerService>();

await builder.Build().RunAsync();
