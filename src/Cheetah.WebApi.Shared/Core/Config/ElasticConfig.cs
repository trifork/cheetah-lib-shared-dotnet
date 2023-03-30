namespace Cheetah.Shared.WebApi.Core.Config
{
    public class ElasticConfig
    {
        public const string Position = "ElasticSearch";
        public string IndexNamePrefix { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}