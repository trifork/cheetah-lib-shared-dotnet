using Cheetah.Core.Config;
using Cheetah.Core.Infrastructure.Services.OpenSearchClient;
using Cheetah.Core.Util;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenSearch.Client;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Cheetah.ComponentTest.OpenSearch
{
    public class OpenSearchReader
    {
        private static readonly ILogger Logger = new LoggerFactory().CreateLogger<OpenSearchReader>();
        
        internal string? Index { get; set; }
        internal string? Server { get; set; }
        internal string? ClientId { get; set; }
        internal string? ClientSecret { get; set; }
        internal string? AuthEndpoint { get; set; }
        CheetahOpenSearchClient? Client { get; set; }

        internal void Prepare()
        {
            Logger.LogInformation("Preparing OpenSearch connection, writing to index '{Index}'", Index);

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

            Client.InternalClient.Indices.Create(new CreateIndexRequest(Index));
        }
        
    }
}

