using System;
using System.Collections.Generic;
using System.Linq;
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


        /// <summary>
        /// Returns the expected number of messages from the index
        /// this reader was initialized with
        /// </summary>
        /// <param name="expectedSize"></param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetMessages(int expectedSize)
        {
            var messages = new List<string>();

            if (Client == null) throw new ArgumentException("Client has not been  configured");
            
            var response = await Client.InternalClient.SearchAsync<string>(s => s.Index(Index).Size(50));

            if (response.IsValid)
            {
                messages.AddRange(response.Documents.Select(d => d.ToString()));
            }

            return messages;
        }

        public long CountAllMessagesInIndex()
        {
            if (Client == null) throw new ArgumentException("Client has not been  configured");

            return Client.InternalClient.Count<string>(c => c
                .Index(Index)
                .Query(q => q
                    .MatchAll())).Count;
        }
    }
}

