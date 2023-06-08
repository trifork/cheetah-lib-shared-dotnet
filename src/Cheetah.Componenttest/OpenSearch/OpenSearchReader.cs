using Microsoft.Extensions.Logging;

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

        internal void Prepare()
        {
            Logger.LogInformation("Preparing OpenSearch connection, wring to index '{Index}'", Index);
            
            
        }
        
    }
}

