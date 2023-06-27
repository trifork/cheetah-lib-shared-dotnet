using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cheetah.Core.Infrastructure.Services.OpenSearchClient;
using Microsoft.Extensions.Logging;

namespace Cheetah.ComponentTest.OpenSearch
{
    public class OpenSearchReader<T> where T : class
    {
        private static readonly ILogger Logger = new LoggerFactory().CreateLogger<OpenSearchReader<T>>();
        internal string? IndexPrefix { get; set; }
        internal CheetahOpenSearchClient? Client { get; set; }

        internal OpenSearchReader<T> Prepare()
        {
            Logger.LogInformation("Preparing OpenSearch to read from index '{Index}'", IndexPrefix);
            
            return this;
        }

        /// <summary>
        /// Returns messages matching the query.
        /// </summary>
        /// <param name="maxSize">How many results to include in the response. Default size is 10</param>
        /// <param name="indexPattern"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetMessages(int maxSize = 10, string indexPattern = "")
        {
            var messages = new List<T>();

            if (Client == null) throw new ArgumentException("Client has not been  configured");

            var response = await Client.InternalClient.SearchAsync<T>(s => s
                .Index(IndexPrefix + indexPattern)
                .Size(maxSize));

            if (response.IsValid)
            {
                messages.AddRange(response.Documents.Select(d =>d));
            }

            return messages;
        }

        public long CountAllMessagesInIndex(string indexPattern = "")
        {
            if (Client == null) throw new ArgumentException("Client has not been  configured");

            return Client.InternalClient.Count<string>(c => c
                .Index(IndexPrefix + indexPattern)
                .Query(q => q
                    .MatchAll())).Count;
        }

        public void DeleteAllMessagesInIndex(string indexPattern = "")
        {
            if (Client == null) throw new ArgumentException("Client has not been  configured");

            Client.InternalClient.DeleteByQueryAsync<T>(del => del
                .Index(IndexPrefix + indexPattern)
                .Query(q => q.QueryString(qs => qs.Query("*"))));
        }
    }
}

