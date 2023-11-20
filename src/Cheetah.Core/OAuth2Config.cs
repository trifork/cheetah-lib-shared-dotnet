namespace Cheetah.Core
{
    public class OAuth2Config
    {
        public string TokenEndpoint { get; set;  }
        public string ClientId { get; set;  }
        public string ClientSecret { get; set; }
        public string? AuthScope { get; set; }
    }
}
