// See https://aka.ms/new-console-template for more information

using Cheetah.Kafka.ExampleProcessor.Models;
using Cheetah.Kafka.ExampleProcessor.Services;
using Cheetah.Kafka.Extensions;
using Cheetah.SchemaRegistry.Avro;
using Cheetah.SchemaRegistry.Extensions;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = new HostApplicationBuilder();
builder
    .Configuration.AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings.Development.json", true)
    .AddEnvironmentVariables();

builder.Services.AddCheetahSchemaRegistry(builder.Configuration);

builder.Services.AddCheetahKafka(builder.Configuration, options =>
    {
        options.ConfigureDefaultSerializerProvider(AvroSerializerProvider.FromServices());
        options.ConfigureDefaultDeserializerProvider(AvroDeserializerProvider.FromServices());
        options.ConfigureDefaultConsumer(config =>
        {
            config.AllowAutoCreateTopics = true;
            config.GroupId = "the-group";
        });
    })
    .WithKeyedConsumer<string, ExampleModelAvro>("A", options =>
    {
        options.SetValueDeserializer(AvroDeserializer.FromServices<ExampleModelAvro>());
        options.ConfigureClient(cfg =>
        {
            cfg.GroupId = "the-big-group";
        });
    })
    .WithKeyedConsumer<string, ExampleModelAvro>("B", options =>
    {
        options.SetKeyDeserializer(_ => Deserializers.Utf8);
        options.SetValueDeserializer(AvroDeserializer.FromServices<ExampleModelAvro>());

    })
    .WithProducer<string, ExampleModelAvro>(options =>
    {
        options.SetKeySerializer(_ => Serializers.Utf8);
        options.SetValueSerializer(AvroSerializer.FromServices<ExampleModelAvro>());
        options.ConfigureClient(cfg =>
        {
            cfg.BatchSize = 100;
        });
    });

builder.Services.AddHostedService<ProducerService>();
builder.Services.AddHostedService<AConsumerService>();
builder.Services.AddHostedService<BConsumerService>();

await builder.Build().RunAsync();
