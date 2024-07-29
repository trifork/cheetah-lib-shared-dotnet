// See https://aka.ms/new-console-template for more information

using Cheetah.Kafka.ExampleProcessor.Models;
using Cheetah.Kafka.ExampleProcessor.Services;
using Cheetah.Kafka.Extensions;
using Cheetah.OpenSearch.Extensions;
using Cheetah.OpenSearch.Util;
using Cheetah.SchemaRegistry.Avro;
using Cheetah.SchemaRegistry.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

var builder = new HostApplicationBuilder();
builder
    .Configuration.AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings.Development.json", true)
    .AddEnvironmentVariables();

builder.Services.AddCheetahSchemaRegistry(builder.Configuration);

builder.Services.AddCheetahKafka(builder.Configuration, options =>
    {
        options.ConfigureDefaultDeserializerProvider(AvroDeserializerProvider.FromServices());
        options.ConfigureDefaultConsumer(config =>
        {
            config.AllowAutoCreateTopics = true;
        });
    })
    .WithConsumer<string, ExampleModelAvro>(options =>
    {
        options.SetValueDeserializer(AvroDeserializer.FromServices<ExampleModelAvro>());
        options.ConfigureClient(cfg =>
        {
            cfg.GroupId = "the-big-group";
        });
    });
builder.Services.AddCheetahOpenSearch(
    builder.Configuration,
    cfg =>
    {
        cfg.WithConnectionSettings(settings =>
        {
            if (builder.Environment.IsDevelopment())
            {
                settings.DisableDirectStreaming();
            }
        });
        cfg.WithJsonSerializerSettings(settings =>
        {
            settings.MissingMemberHandling = MissingMemberHandling.Error;
            settings.Converters.Add(new UtcDateTimeConverter());
        });
    }
);

builder.Services.AddHostedService<ConsumerService>();

await builder.Build().RunAsync();
