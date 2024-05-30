using Cheetah.OpenSearch.Extensions;
using Cheetah.OpenSearch.Util;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddCheetahOpenSearch(
    builder.Configuration,
    cfg =>
    {
        cfg.DisableDirectStreaming = builder.Environment.IsDevelopment();
        cfg.WithJsonSerializerSettings(settings =>
        {
            settings.MissingMemberHandling = MissingMemberHandling.Error;
            settings.Converters.Add(new UtcDateTimeConverter());
        });
    }
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
