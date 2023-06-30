using System;
using System.Collections.Generic;
using Cheetah.Core.Config;
using Cheetah.Core.Infrastructure.Services.OpenSearchClient;
using Cheetah.Core.Util;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Cheetah.ComponentTest.OpenSearch;

public class OpenSearchClient
{

    public OpenSearchClient(string osAddress, string clientId, string clientSecret, string authEndpoint)
    {
        OsAddress = osAddress;
        ClientId = clientId;
        ClientSecret = clientSecret;
        AuthEndpoint = authEndpoint;
    }

    internal string OsAddress { get; set; }
    internal string ClientId { get; set; }
    internal string ClientSecret { get; set; }
    internal string AuthEndpoint { get; set; }
    CheetahOpenSearchClient? Client { get; set; }

    
    public void Prepare()
    {
        var openSearchConfig = new OpenSearchConfig
        {
            AuthMode = OpenSearchConfig.OpenSearchAuthMode.OAuth2,
            Url = OsAddress,
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

    public void Index<T>(string index, ICollection<T> documents)
    {
        throw new NotImplementedException();
    }

    public int Count(string index)
    {
        throw new NotImplementedException();
    }

    public ICollection<T> Search<T>(string index)
    {
        throw new NotImplementedException();
    }

    public void ClearIndex(string index)
    {
        throw new NotImplementedException();
    }
}
