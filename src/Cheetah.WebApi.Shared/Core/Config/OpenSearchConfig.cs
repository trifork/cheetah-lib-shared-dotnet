namespace Cheetah.Shared.WebApi.Core.Config
{
    public class OpenSearchConfig
    {
        public const string Position = "OpenSearch";
        public string IndexNamePrefix { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string TokenEndpoint { get; set; } = string.Empty;
    }
}