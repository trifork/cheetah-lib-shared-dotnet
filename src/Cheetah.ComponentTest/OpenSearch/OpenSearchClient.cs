using System;
using System.Collections.Generic;
using System.Linq;
using Cheetah.Core.Config;
using Cheetah.Core.Infrastructure.Services.OpenSearchClient;
using Cheetah.Core.Util;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenSearch.Client;
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

        var openSearchConfig = new OpenSearchConfig
        {
            AuthMode = OpenSearchConfig.OpenSearchAuthMode.OAuth2,
            Url = OsAddress,
            ClientId = ClientId,
            ClientSecret = ClientSecret,
            TokenEndpoint = AuthEndpoint
        };
            
        Client = PrepareClient();
    }

    internal string OsAddress { get; set; }
    internal string ClientId { get; set; }
    internal string ClientSecret { get; set; }
    internal string AuthEndpoint { get; set; }
    CheetahOpenSearchClient Client { get; set; }

    
    public CheetahOpenSearchClient PrepareClient()
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
            
        return new CheetahOpenSearchClient(memoryCache, httpClientFactory, options, env, logger);
    }

    public void Index<T>(string index, ICollection<T> documents) where T : class
    {
        Client.InternalClient.Bulk(b => b
            .Index(index)
            .CreateMany<T>(documents)
        );
    }

    // TODO: overload count with possible match query
    public long Count(string index)
    {
        return Client.InternalClient.Count<object>(q => q.Index(index)).Count;
    }

    public IReadOnlyCollection<IHit<T>> Search<T>(string index, int maxSize = 100) where T : class
    {
        return Client.InternalClient.Search<T>(q => q
            .Index(index)
            .Size(maxSize)
        ).Hits;
    }

    public void DeleteIndex(string index)
    {
        Client.InternalClient.Indices.Delete(index);
    }
}
