using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Cheetah.Core.Config;
using Cheetah.Core.Infrastructure.Services.OpenSearchClient;
using Cheetah.Core.Util;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenSearch.Client;
using OpenSearch.Net;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Cheetah.ComponentTest.OpenSearch;

public class OpenSearchWriter<T> where T : class
{
    private static readonly ILogger Logger = new LoggerFactory().CreateLogger<OpenSearchReader<T>>();
        
    internal string? IndexName { get; set; }
    internal string? Server { get; set; }
    internal string? ClientId { get; set; }
    internal string? ClientSecret { get; set; }
    internal string? AuthEndpoint { get; set; }
    CheetahOpenSearchClient? Client { get; set; }
    
    internal void Prepare()
    {
        Logger.LogInformation("Preparing OpenSearch connection, writing to index '{Index}'", IndexName);

        var openSearchConfig = new OpenSearchConfig
        {
            Url = Server,
            // Oauth2
            ClientId = ClientId,
            ClientSecret = ClientSecret,
            TokenEndpoint = AuthEndpoint
        };
            
        var options = Options.Create(openSearchConfig);
        var env = new HostingEnvironment { EnvironmentName = Environments.Development };
            
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var httpClientFactory = new DefaultHttpClientFactory();
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug);
            builder.AddConsole();
        });
            
        var logger = loggerFactory.CreateLogger<CheetahOpenSearchClient>();
            
        Client = new(memoryCache, httpClientFactory, options, env, logger);

        // create the index if it doesn't already exists
        if (!Client.InternalClient.Indices.Exists(IndexName).Exists)
        {
            Client.InternalClient.Indices.Create(new CreateIndexRequest(IndexName));
        }
    }

    public async Task WriteAsync(T message)
    {
        if (Client == null) throw new ArgumentException("Client has not been  configured");
        
        await Client.InternalClient.IndexAsync( message,  i => i.Index(IndexName));
    }

    public async Task WriteAsync(params T[] messages)
    {
        if (Client == null) throw new ArgumentException("Client has not been  configured");

        await Client.InternalClient.IndexManyAsync(messages.Select(x => JsonSerializer.Serialize(x)), IndexName);
    }
}
