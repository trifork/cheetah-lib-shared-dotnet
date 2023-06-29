using System;
using Cheetah.Core.Config;
using Cheetah.Core.Infrastructure.Services.OpenSearchClient;
using Cheetah.Core.Util;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Cheetah.ComponentTest.OpenSearch;

public class OpenSearchConnector
{
    internal string? Server { get; set; }
    internal string? ClientId { get; set; }
    internal string? ClientSecret { get; set; }
    internal string? AuthEndpoint { get; set; }
    CheetahOpenSearchClient? Client { get; set; }
    
    public void Prepare()
    {
        if (Server == null || ClientId == null || ClientSecret == null || AuthEndpoint == null)
        {
            throw new InvalidOperationException("Server, ClientId, ClientSecret and AuthEndpoint must be set");
        }
        var openSearchConfig = new OpenSearchConfig
        {
            Url = Server, // Oauth2
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
    }

    public OpenSearchReader<T> NewReader<T>(string indexPrefix = "") where T : class
    {
        return new OpenSearchReader<T>()
        {
            Client = this.Client,
            IndexPrefix = indexPrefix
        }.Prepare();
    }

    public OpenSearchWriter<T> NewWriter<T>(string indexPattern) where T : class
    {
        return new OpenSearchWriter<T>()
        {
            Client = this.Client,
            IndexPattern = indexPattern
        }.Prepare();
    }
}
