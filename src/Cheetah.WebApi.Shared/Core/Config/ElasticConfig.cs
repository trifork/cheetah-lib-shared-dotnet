namespace Cheetah.template.WebApi.Core.Config
{
    public class ElasticConfig
    {
        public const string Position = "ElasticSearch";
        public string IndexNamePrefix { get; set; }
        public string Url { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}