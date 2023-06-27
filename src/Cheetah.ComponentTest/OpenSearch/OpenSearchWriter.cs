using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Cheetah.Core.Infrastructure.Services.OpenSearchClient;
using Microsoft.Extensions.Logging;
using OpenSearch.Client;

namespace Cheetah.ComponentTest.OpenSearch;

public class OpenSearchWriter<T> where T : class
{
    private static readonly ILogger Logger = new LoggerFactory().CreateLogger<OpenSearchReader<T>>();
        
    internal string? IndexPattern { get; set; }
    internal CheetahOpenSearchClient? Client { get; set; }
    
    internal OpenSearchWriter<T> Prepare()
    {
        Logger.LogInformation("Preparing OpenSearch connection, writing to index '{Index}'", IndexPattern);
        
        // create the index if it doesn't already exists
        if (!Client.InternalClient.Indices.Exists(IndexPattern).Exists)
        {
            Client.InternalClient.Indices.Create(new CreateIndexRequest(IndexPattern));
        }

        return this;
    }

    public async Task WriteAsync(string indexPattern = "", params T[] messages)
    {
        if (Client == null) throw new ArgumentException("Client has not been  configured");

        await Client.InternalClient.IndexManyAsync(messages.Select(x => JsonSerializer.Serialize(x)), indexPattern);
    }
}
